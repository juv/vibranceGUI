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
            _vibranceInfo.isWindowsLevelKnown = true;
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
            // Same restore this proxy now runs from both its OnWinEventHook restore branch and
            // here, in place of the old unconditional SetSaturationOnAllDisplays(...) both used to
            // call regardless of affectPrimaryMonitorOnly (issue #60/#36 on AMD: the flag was never
            // consulted here, so a second monitor's level was stomped on every exit even with the
            // checkbox on).
            RestoreWindowsVibranceLevel();
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
                        IsResolutionChangeNeeded(screen, applicationSetting.ResolutionSettings) &&
                        _windowsResolutionSettings.ContainsKey(screen.DeviceName) &&
                        _windowsResolutionSettings[screen.DeviceName].Item2.Contains(applicationSetting.ResolutionSettings))
                    {
                        _gameScreen = screen;
                        PerformResolutionChange(screen, applicationSetting.ResolutionSettings);
                    }

                    // The unconditional SetSaturationOnAllDisplays(userVibranceSettingDefault) that
                    // used to run here, before applying the game's level, is gone (issue #60/#36 on
                    // AMD, apply side): it reset every attached display's saturation to the Windows
                    // level - ignoring affectPrimaryMonitorOnly entirely - immediately before one of
                    // the two branches below overwrote some or all of that same reset with the
                    // game's level. Every display it touched was either about to be overwritten by
                    // this same event (a visible flash, no lasting effect) or, with the flag on, a
                    // non-game display it had no business touching at all - removing it changes no
                    // display's final state.
                    if (_vibranceInfo.affectPrimaryMonitorOnly)
                    {
                        _amdAdapter.SetSaturationOnDisplay(applicationSetting.IngameLevel, screen.DeviceName);
                        // Only the game's own screen was written - that is the only display owing
                        // a restore.
                        VibranceRestoreHelper.RecordGameLevelApplied(screen.DeviceName);
                    }
                    else
                    {
                        _amdAdapter.SetSaturationOnAllDisplays(applicationSetting.IngameLevel);
                        // This branch really did write every attached display, not just the game's
                        // own - unlike NVIDIA's equivalent, IAmdAdapter has no per-display read-back
                        // to confirm any of them individually, so every currently attached display
                        // is recorded as owing a restore.
                        foreach (Screen attachedScreen in Screen.AllScreens)
                        {
                            VibranceRestoreHelper.RecordGameLevelApplied(attachedScreen.DeviceName);
                        }
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
                        IsResolutionChangeNeeded(screen, _windowsResolutionSettings[screen.DeviceName].Item1))
                    {
                        PerformResolutionChange(screen, _windowsResolutionSettings[screen.DeviceName].Item1);
                    }

                    //apply Windows saturation
                    RestoreWindowsVibranceLevel();
                }
            }
        }

        // Same restore run from both the OnWinEventHook restore branch above and HandleDvcExit, in
        // place of the old unconditional SetSaturationOnAllDisplays(...) both used to call
        // regardless of affectPrimaryMonitorOnly (issue #60/#36 on AMD: the flag was never
        // consulted here, so a second monitor's level was stomped on every restore and every exit
        // even with the checkbox on).
        private void RestoreWindowsVibranceLevel()
        {
            // A no-op before SetVibranceWindowsLevel has actually run once - see VibranceInfo's
            // isWindowsLevelKnown comment and NvidiaDynamicVibranceProxy's matching guard.
            if (!_vibranceInfo.isWindowsLevelKnown)
            {
                return;
            }

            if (!_vibranceInfo.affectPrimaryMonitorOnly)
            {
                _amdAdapter.SetSaturationOnAllDisplays(_vibranceInfo.userVibranceSettingDefault);
                VibranceRestoreHelper.ClearAllGameLevelRecords();
                return;
            }

            // IAmdAdapter has no read-back to confirm a write landed, unlike NVIDIA's
            // equalsDVCLevel/setDVCLevel pair - so, unlike NvidiaDynamicVibranceProxy's
            // RestoreOneDisplay, every target is written unconditionally and cleared
            // unconditionally, unable to tell "already correct" from "just fixed" or to retry a
            // failure that has no way to be observed here.
            List<string> targets = VibranceRestoreHelper.ComposeRestoreTargets(true, VibranceRestoreHelper.GetPrimaryDeviceName());
            foreach (string deviceName in targets)
            {
                _amdAdapter.SetSaturationOnDisplay(_vibranceInfo.userVibranceSettingDefault, deviceName);
                VibranceRestoreHelper.ClearGameLevelRecord(deviceName);
            }
        }

        private static bool IsResolutionChangeNeeded(Screen screen, ResolutionModeWrapper resolutionSettings)
        {
            Devmode mode;
            if (resolutionSettings != null && ResolutionHelper.GetCurrentResolutionSettings(out mode, screen.DeviceName) && !resolutionSettings.Equals(mode))
            {
                return true;
            }
            return false;
        }

        private static void PerformResolutionChange(Screen screen, ResolutionModeWrapper resolutionSettings)
        {
            ResolutionHelper.ChangeResolutionEx(resolutionSettings, screen.DeviceName);
        }
    }
}
