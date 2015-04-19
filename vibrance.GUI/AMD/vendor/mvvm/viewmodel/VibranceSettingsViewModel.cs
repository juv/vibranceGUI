using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using Newtonsoft.Json;
using gui.app.gpucontroller.amd;
using gui.app.utils;
using System.Diagnostics;
using vibrance.GUI.AMD.vendor;
using System.Collections;

namespace gui.app.mvvm.model
{
    public class VibranceSettingsViewModel : ViewModelBase
    {
        private static readonly object _padlock = new object();

        private readonly AmdAdapter _gpuAdapter;

        private VibranceSettings _model;

        private readonly string _settingsFileFullName;

        private readonly Action<string> _addLogItem;

        private bool windowsAlreadySet = false;

        private int lastSetIngameVibranceLevel;

        private IntPtr lastSetCsgoHandle;

        public VibranceSettingsViewModel(Action<string> addLogItem, AmdAdapter gpuAdapter)
        {
            _addLogItem = addLogItem;
            _gpuAdapter = gpuAdapter;
            _model = new VibranceSettings();
            SettingsName = "amd_settings.cfg";
            _settingsFileFullName = Path.Combine(CommonUtils.GetVibrance_GUI_AppDataPath(), SettingsName);
        }

        private IntPtr GetCsGoHandle()
        {
            return FindWindow(IntPtr.Zero, "Counter-Strike: Global Offensive");
        }
        
        public void RefreshVibranceStatus(IntPtr foregroundHwnd)
        {
            IntPtr csgoHandle = this.GetCsGoHandle();

            if (csgoHandle == foregroundHwnd || (csgoHandle != IntPtr.Zero && this.Model.KeepVibranceOnWhenCsGoIsStarted))
            {
                windowsAlreadySet = false;

                if (csgoHandle != lastSetCsgoHandle || Model.IngameVibranceLevel != lastSetIngameVibranceLevel) 
                {
                    var displayName = GetDisplayName(csgoHandle);
                    _gpuAdapter.SetSaturationOnDisplay(Model.IngameVibranceLevel, displayName);
                    lastSetIngameVibranceLevel = Model.IngameVibranceLevel;
                    lastSetCsgoHandle = csgoHandle;
                    _addLogItem(string.Format("Vibrance set to '{0}' for '{1}'", Model.IngameVibranceLevel, displayName));
                }
            }
            else if (windowsAlreadySet == false)
            {
                if (this.Model.KeepVibranceOnWhenCsGoIsStarted == false || csgoHandle == IntPtr.Zero)
                {
                    windowsAlreadySet = true;
                    foreach (var screen in Screen.AllScreens)
                    {
                        _gpuAdapter.SetSaturationOnDisplay(Model.WindowsVibranceLevel, screen.DeviceName);
                        _addLogItem(string.Format("Vibrance set to '{0}' for '{1}'", Model.WindowsVibranceLevel, screen.DeviceName));
                    }

                    lastSetIngameVibranceLevel = -1;
                    lastSetCsgoHandle = IntPtr.Zero;
                }
            }
        }

        public void ResetVibrance()
        {
            foreach (var screen in Screen.AllScreens)
            {
                _gpuAdapter.SetSaturationOnDisplay(this.Model.WindowsVibranceLevel, screen.DeviceName);
            }
        }

        private string GetDisplayName(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero)
            {
                return null;
            }

            return Screen.FromHandle(hwnd).DeviceName;
        }

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        private static extern IntPtr FindWindow(IntPtr zeroOnly, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        public string SettingsName { get; private set; }

        public VibranceSettings Model
        {
            get
            {
                return _model;
            }

            private set
            {
                if (_model != null)
                {
                    _model.PropertyChanged -= HandleSettingChanged;
                }

                this.Set(() => Model, ref _model, value, true);
                _model.PropertyChanged += HandleSettingChanged;
            }
        }

        private void HandleSettingChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "WindowsVibranceLevel")
            {
                windowsAlreadySet = false;
            }

            RefreshVibranceStatus(GetForegroundWindow());
        }

        public bool SettingsExists()
        {
            return File.Exists(_settingsFileFullName);
        }

        public void SaveVibranceSettings()
        {
            lock (_padlock)
            {
                using (StreamWriter streamWriter = new StreamWriter(new FileStream(_settingsFileFullName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite), Encoding.UTF8))
                {
                    streamWriter.Write(JsonConvert.SerializeObject(Model, Formatting.Indented));
                }
            }
        }

        public void LoadVibranceSettings()
        {
            lock (_padlock)
            {
                try
                {
                    using (StreamReader streamReader = new StreamReader(new FileStream(_settingsFileFullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Encoding.UTF8))
                    {
                        string content = streamReader.ReadToEnd();
                        if (string.IsNullOrEmpty(content))
                        {
                            return;
                        }

                        Model = JsonConvert.DeserializeObject<VibranceSettings>(content);
                    }
                }
                catch (Exception)
                {
                    Model = new VibranceSettings();
                    SaveVibranceSettings();
                }
            }
        }
    }
}