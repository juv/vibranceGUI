using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using vibrance.GUI.AMD.vendor;
using vibrance.GUI.common;
using vibrance.GUI.NVIDIA;

namespace vibrance.GUI.AMD
{
    public class AmdDynamicVibranceProxy : IVibranceProxy
    {
        private readonly IAmdAdapter _amdAdapter;
        private List<ApplicationSetting> _applicationSettings;
        private readonly Dictionary<string, Tuple<ResolutionModeWrapper, List<ResolutionModeWrapper>>> _windowsResolutionSettings;
        private VibranceInfo _vibranceInfo;
        private WinEventHook _hook;
        private static Screen _gameScreen;

        public AmdDynamicVibranceProxy(IAmdAdapter amdAdapter, List<ApplicationSetting> applicationSettings, Dictionary<string, Tuple<ResolutionModeWrapper, List<ResolutionModeWrapper>>> windowsResolutionSettings)
        {
            _amdAdapter = amdAdapter;
            _applicationSettings = applicationSettings;
            _windowsResolutionSettings = windowsResolutionSettings;

            try
            {
                _vibranceInfo = new VibranceInfo();
                if (amdAdapter.IsAvailable())
                {
                    _vibranceInfo.isInitialized = true;
                    amdAdapter.Init();
                }

                if (_vibranceInfo.isInitialized)
                {
                    _hook = WinEventHook.GetInstance();
                    _hook.WinEventHookHandler += OnWinEventHook;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                DialogResult result = MessageBox.Show(NvidiaDynamicVibranceProxy.NvapiErrorInitFailed, "vibranceGUI Error",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                if (result == DialogResult.OK)
                {
                    Process.Start(NvidiaDynamicVibranceProxy.GuideLink);
                }
            }
        }

        public void SetApplicationSettings(List<ApplicationSetting> refApplicationSettings)
        {
            _applicationSettings = refApplicationSettings;
        }

        public void SetShouldRun(bool shouldRun)
        {
            _vibranceInfo.shouldRun = shouldRun;
        }

        public void SetNeverSwitchResolution(bool neverChangeResolution)
        {
            _vibranceInfo.neverChangeResolution = neverChangeResolution;
        }

        public void SetVibranceWindowsLevel(int vibranceWindowsLevel)
        {
            _vibranceInfo.userVibranceSettingDefault = vibranceWindowsLevel;
        }

        public void SetVibranceIngameLevel(int vibranceIngameLevel)
        {
            _vibranceInfo.userVibranceSettingActive = vibranceIngameLevel;
        }

        public bool UnloadLibraryEx()
        {
            _hook.RemoveWinEventHook();
            return true;
        }

        public void HandleDvcExit()
        {
            _amdAdapter.SetSaturationOnAllDisplays(_vibranceInfo.userVibranceSettingDefault);
        }

        public void SetAffectPrimaryMonitorOnly(bool affectPrimaryMonitorOnly)
        {
            _vibranceInfo.affectPrimaryMonitorOnly = affectPrimaryMonitorOnly;
        }

        public VibranceInfo GetVibranceInfo()
        {
            return _vibranceInfo;
        }

        public GraphicsAdapter GraphicsAdapter { get; } = GraphicsAdapter.Amd;

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        private void OnWinEventHook(object sender, WinEventHookEventArgs e)
        {
            if (_applicationSettings.Count > 0)
            {
                ApplicationSetting applicationSetting = _applicationSettings.FirstOrDefault(x => string.Equals(x.Name, e.ProcessName, StringComparison.OrdinalIgnoreCase));
                if (applicationSetting != null)
                {
                    //test if a resolution change is needed
                    Screen screen = Screen.FromHandle(e.Handle);
                    if (_vibranceInfo.neverChangeResolution == false &&
                        applicationSetting.IsResolutionChangeNeeded &&
                        ResolutionHelper.IsResolutionChangeNeeded(screen.DeviceName, applicationSetting.ResolutionSettings) &&
                        _windowsResolutionSettings.ContainsKey(screen.DeviceName) &&
                        _windowsResolutionSettings[screen.DeviceName].Item2.Contains(applicationSetting.ResolutionSettings))
                    {
                        _gameScreen = screen;
                        ResolutionHelper.ResolutionChangeResult result = ResolutionHelper.ChangeResolutionEx(
                            applicationSetting.ResolutionSettings, screen.DeviceName, false);
                        // AppliedUnverified means CDS_UPDATEREGISTRY itself reported success but the
                        // post-apply readback did not confirm it - see the matching comment in
                        // NvidiaDynamicVibranceProxy's OnWinEventHook.
                        _vibranceInfo.isResolutionChangeApplied =
                            result == ResolutionHelper.ResolutionChangeResult.Applied ||
                            result == ResolutionHelper.ResolutionChangeResult.AppliedUnverified;
                    }

                    _amdAdapter.SetSaturationOnAllDisplays(_vibranceInfo.userVibranceSettingDefault);
                    if (_vibranceInfo.affectPrimaryMonitorOnly)
                    {
                        _amdAdapter.SetSaturationOnDisplay(applicationSetting.IngameLevel, screen.DeviceName);
                    }
                    else
                    {
                        _amdAdapter.SetSaturationOnAllDisplays(applicationSetting.IngameLevel);
                    }
                }
                else
                {
                    IntPtr processHandle = e.Handle;
                    if (GetForegroundWindow() != processHandle)
                        return;

                    //test if a resolution change is needed
                    Screen screen = Screen.FromHandle(processHandle);
                    if (_vibranceInfo.neverChangeResolution == false &&
                        _gameScreen != null && _gameScreen.Equals(screen) &&
                        _windowsResolutionSettings.ContainsKey(screen.DeviceName) &&
                        ResolutionHelper.IsResolutionChangeNeeded(screen.DeviceName, _windowsResolutionSettings[screen.DeviceName].Item1))
                    {
                        ResolutionHelper.ResolutionChangeResult result = ResolutionHelper.ChangeResolutionEx(
                            _windowsResolutionSettings[screen.DeviceName].Item1, screen.DeviceName, true);
                        // A failed (or unverified) revert must leave the flag true so the next
                        // foreground event retries it; Suppressed (the give-up state) deliberately
                        // still clears it - see the matching comment in NvidiaDynamicVibranceProxy's
                        // OnWinEventHook.
                        if (result != ResolutionHelper.ResolutionChangeResult.Failed &&
                            result != ResolutionHelper.ResolutionChangeResult.AppliedUnverified)
                            _vibranceInfo.isResolutionChangeApplied = false;
                    }

                    _amdAdapter.SetSaturationOnAllDisplays(_vibranceInfo.userVibranceSettingDefault);
                }
            }
        }
    }
}