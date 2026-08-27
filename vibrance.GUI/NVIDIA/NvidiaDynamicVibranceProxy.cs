using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using vibrance.GUI.common;

namespace vibrance.GUI.NVIDIA
{
    // The seam between the vibrance apply/restore logic below and the actual NVIDIA driver.
    // RealNvidiaVibranceDevice (nested at the bottom of NvidiaDynamicVibranceProxy) is the only
    // production implementation; VibranceRestoreFixture supplies a fake that records every
    // (handle, level) it is asked to write, so the apply/restore logic - the part that carried
    // issues #60/#36 (a second monitor reset on every launch), #144 (vibrance surviving the game
    // closing) and #95 (vibrance surviving an alt-tab to another monitor) - can be driven through
    // real cycles, including forced failures, without a GPU.
    internal interface INvidiaVibranceDevice
    {
        // hWnd is ref, matching isWindowActive's own HWND* signature, not IntPtr-by-value: the
        // pre-existing call site ("IntPtr processHandle = e.Handle; ... isWindowActive(ref
        // processHandle); ... Screen.FromHandle(processHandle)") already relies on whatever this
        // native call may write back into the handle it was given determining which screen the
        // rest of that branch resolves to. Taking hWnd by value here would silently drop that and
        // risk changing which screen a restore reasons about - exactly the kind of behaviour this
        // fix is not supposed to touch.
        bool IsWindowActive(ref IntPtr hWnd);

        // getAssociatedNvidiaDisplayHandle. -1 when NvAPI cannot name deviceName's display; never
        // called with a null or empty deviceName.
        int TryResolveDisplayHandle(string deviceName);

        // equalsDVCLevel.
        bool IsAtLevel(int displayHandle, int level);

        // setDVCLevel.
        bool SetLevel(int displayHandle, int level);
    }

    class NvidiaDynamicVibranceProxy : IVibranceProxy
    {
        #region DllImports
        [DllImport(
            "vibranceDLL.dll",
            EntryPoint = "?initializeLibrary@vibrance@vibranceDLL@@QAE_NXZ",
            CallingConvention = CallingConvention.StdCall,
            CharSet = CharSet.Auto)]
        static extern bool initializeLibrary();

        [DllImport(
            "vibranceDLL.dll",
            EntryPoint = "?unloadLibrary@vibrance@vibranceDLL@@QAE_NXZ",
            CallingConvention = CallingConvention.StdCall,
            CharSet = CharSet.Auto)]
        static extern bool unloadLibrary();


        [DllImport(
            "vibranceDLL.dll",
            EntryPoint = "?getActiveOutputs@vibrance@vibranceDLL@@QAEHQAPAH0@Z",
            CallingConvention = CallingConvention.StdCall,
            CharSet = CharSet.Auto)]
        static extern int getActiveOutputs([In, Out] int[] gpuHandles, [In, Out] int[] outputIds);

        [DllImport(
            "vibranceDLL.dll",
            EntryPoint = "?enumeratePhsyicalGPUs@vibrance@vibranceDLL@@QAEXQAPAH@Z",
            CallingConvention = CallingConvention.StdCall,
            CharSet = CharSet.Auto)]
        static extern void enumeratePhsyicalGPUs([In, Out] int[] gpuHandles);

        [DllImport(
            "vibranceDLL.dll",
            EntryPoint = "?getGpuName@vibrance@vibranceDLL@@QAE_NQAPAHPAD@Z",
            CallingConvention = CallingConvention.StdCall,
            CharSet = CharSet.Ansi)]
        static extern bool getGpuName([In, Out] int[] gpuHandles, StringBuilder szName);

        [DllImport(
            "vibranceDLL.dll",
            EntryPoint = "?getDVCInfo@vibrance@vibranceDLL@@QAE_NPAUNV_DISPLAY_DVC_INFO@12@H@Z",
            CallingConvention = CallingConvention.StdCall,
            CharSet = CharSet.Ansi)]
        static extern bool getDVCInfo(ref NvDisplayDvcInfo info, int defaultHandle);

        [DllImport(
            "vibranceDLL.dll",
            EntryPoint = "?enumerateNvidiaDisplayHandle@vibrance@vibranceDLL@@QAEHH@Z",
            CallingConvention = CallingConvention.StdCall,
            CharSet = CharSet.Auto)]
        static extern int enumerateNvidiaDisplayHandle(int index);

        [DllImport(
            "vibranceDLL.dll",
            EntryPoint = "?setDVCLevel@vibrance@vibranceDLL@@QAE_NHH@Z",
            CallingConvention = CallingConvention.StdCall,
            CharSet = CharSet.Auto)]
        static extern bool setDVCLevel([In] int defaultHandle, [In] int level);

        [DllImport(
            "vibranceDLL.dll",
            EntryPoint = "?isWindowActive@vibrance@vibranceDLL@@QAE_NPAPAUHWND__@@@Z",
            CallingConvention = CallingConvention.StdCall,
            CharSet = CharSet.Auto)]
        static extern bool isWindowActive(ref IntPtr hwnd);

        [DllImport(
            "vibranceDLL.dll",
            EntryPoint = "?isCsgoStarted@vibrance@vibranceDLL@@QAE_NPAPAUHWND__@@@Z",
            CallingConvention = CallingConvention.StdCall,
            CharSet = CharSet.Ansi)]
        static extern bool isCsgoStarted(ref IntPtr hwnd);

        [DllImport(
            "vibranceDLL.dll",
            EntryPoint = "?equalsDVCLevel@vibrance@vibranceDLL@@QAE_NHH@Z",
            CallingConvention = CallingConvention.StdCall,
            CharSet = CharSet.Auto)]
        static extern bool equalsDVCLevel([In] int defaultHandle, [In] int level);

        [DllImport(
            "vibranceDLL.dll",
            EntryPoint = "?getGpuSystemType@vibrance@vibranceDLL@@QAEHPAH@Z",
            CallingConvention = CallingConvention.StdCall,
            CharSet = CharSet.Auto)]
        static extern NvSystemType getGpuSystemType(int gpuHandle);

        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        static extern int GetWindowTextLength([In] IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        static extern int GetWindowTextA([In] IntPtr hWnd, [In, Out] StringBuilder lpString, [In] int nMaxCount);

        [DllImport(
            "vibranceDLL.dll",
            EntryPoint = "?getAssociatedNvidiaDisplayHandle@vibrance@vibranceDLL@@QAEHPBDH@Z",
            CallingConvention = CallingConvention.StdCall,
            CharSet = CharSet.Ansi)]
        static extern int getAssociatedNvidiaDisplayHandle(string deviceName, [In] int length);
        #endregion


        public const int NvapiMaxPhysicalGpus = 64;
        public const int NvapiMaxLevel = 63;
        public const int NvapiDefaultLevel = 0;

        public const string NvapiErrorInitFailed = "VibranceProxy failed to initialize! Press Ok to open the vibranceGUI Steam Guide in your browser. " +
            "Scroll down to section \"Troubleshooting, Errors, Q&A\".";
        public const string NvapiErrorSystypeUnsupported = "VibranceProxy detected that you are running a Laptop with integrated NVIDIA card. " +
            "NVIDIA Laptops are not supported because their NVIDIA drivers do not contain Digital Vibrance! " +
            "You are missing the Digital Vibrance option in your NVIDIA Control Panel. VibranceGUI can not run on your system.";
        public const string NvapiErrorSystypeUnknown = "VibranceProxy failed to initialize! Graphics card system type (Desktop / Laptop) is unknown!";
        public const string GuideLink = "https://vibrancegui.com/vibrance/guide";

        private static VibranceInfo _vibranceInfo;
        private static List<ApplicationSetting> _applicationSettings;
        private static Dictionary<string, Tuple<ResolutionModeWrapper, List<ResolutionModeWrapper>>> _windowsResolutionSettings;
        private WinEventHook _hook;
        private static Screen _gameScreen;

        // The only production INvidiaVibranceDevice. Not readonly - ResetForTests below swaps it
        // for a fake so VibranceRestoreFixture can drive ApplyGameVibranceLevel/
        // RestoreWindowsVibranceLevel (and, through them, OnWinEventHook itself, which is private
        // static and so reachable by reflection with no instance at all) without ever calling
        // initializeLibrary() or touching a real GPU.
        private static INvidiaVibranceDevice _device = new RealNvidiaVibranceDevice();

        // Suppresses repeat log calls for a display that is failing to resolve or failing to
        // write, on either the apply or the restore path - one set per device, cleared on any
        // success, so a foreground-change storm against a broken display does not redo a
        // synchronous log write on the UI thread on every single event.
        private static readonly HashSet<string> _loggedDisplayFailures = new HashSet<string>();

        public NvidiaDynamicVibranceProxy(List<ApplicationSetting> savedApplicationSettings, Dictionary<string, Tuple<ResolutionModeWrapper, List<ResolutionModeWrapper>>> currentWindowsResolutionSettings)
        {
            try
            {
                _applicationSettings = savedApplicationSettings;
                _windowsResolutionSettings = currentWindowsResolutionSettings;
                _vibranceInfo = new VibranceInfo();
                if (initializeLibrary())
                {
                    InitializeProxy();
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
                    Process.Start(GuideLink);
                }
            }
        }

        private void InitializeProxy()
        {
            int[] gpuHandles = new int[NvapiMaxPhysicalGpus];
            int[] outputIds = new int[NvapiMaxPhysicalGpus];
            enumeratePhsyicalGPUs(gpuHandles);

            foreach (int gpuHandle in gpuHandles)
            {
                if(gpuHandle != 0)
                {
                    NvSystemType systemType = getGpuSystemType(gpuHandle);
                    if (systemType == NvSystemType.NvSystemTypeUnknown)
                    {
                        MessageBox.Show(NvidiaDynamicVibranceProxy.NvapiErrorSystypeUnknown, "vibranceGUI Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        _vibranceInfo.isInitialized = false;
                        return;
                    }
                }
            }

            EnumerateDisplayHandles();

            _vibranceInfo.activeOutput = getActiveOutputs(gpuHandles, outputIds);
            StringBuilder buffer = new StringBuilder(64);
            char[] sz = new char[64];
            getGpuName(gpuHandles, buffer);
            _vibranceInfo.szGpuName = buffer.ToString();

            // No DVC write here anymore (issue #60/#36). This used to do
            // "_vibranceInfo.defaultHandle = enumerateNvidiaDisplayHandle(0);" - an arbitrary
            // display, not necessarily the primary and not one affectPrimaryMonitorOnly was ever
            // consulted for - then write _vibranceInfo.userVibranceSettingDefault to it via
            // getDVCInfo/setDVCLevel. But SetVibranceWindowsLevel(...) has not been called yet at
            // this point in startup (it runs later, from VibranceGUI.cs's backgroundWorker_DoWork,
            // after that method's own "while (!this.IsHandleCreated) Thread.Sleep(500);" wait), so
            // userVibranceSettingDefault was still VibranceInfo's struct default of 0 - every
            // launch stamped 0 onto whatever display handle 0 happened to name, resetting a second
            // monitor's level even though it was never a game's screen and affectPrimaryMonitorOnly
            // was never asked about it.
            // A foreground event CAN still land on the hook before SetVibranceWindowsLevel runs
            // (the app's own window appearing is one) - RestoreWindowsVibranceLevel's
            // isWindowsLevelKnown guard (see VibranceInfo) makes that a no-op instead of writing
            // the still-unknown level, rather than trying to make it unreachable. The desktop scope
            // then receives the real, saved Windows level on the first non-game foreground event
            // after SetVibranceWindowsLevel has actually run.

            _vibranceInfo.isInitialized = true;
        }

        private static void OnWinEventHook(object sender, WinEventHookEventArgs e)
        {
            if (_applicationSettings.Count > 0)
            {
                ApplicationSetting applicationSetting = _applicationSettings.FirstOrDefault(x => string.Equals(x.Name, e.ProcessName, StringComparison.OrdinalIgnoreCase));
                if (applicationSetting != null)
                {
                    Screen screen = Screen.FromHandle(e.Handle);
                    int displayHandle = _device.TryResolveDisplayHandle(screen.DeviceName);
                    // 0 is a null NvDisplayHandle, same as the -1 "not found" - InitializeProxy
                    // used to treat a gpuHandle of 0 the same way. Neither one is a display this
                    // can meaningfully write to.
                    //test if changing the vibrance value is needed
                    if (displayHandle != -1 && displayHandle != 0 && !_device.IsAtLevel(displayHandle, applicationSetting.IngameLevel))
                    {
                        //test if a resolution change is needed
                        if (_vibranceInfo.neverChangeResolution == false &&
                            applicationSetting.IsResolutionChangeNeeded &&
                            IsResolutionChangeNeeded(screen, applicationSetting.ResolutionSettings) &&
                            _windowsResolutionSettings.ContainsKey(screen.DeviceName) &&
                            _windowsResolutionSettings[screen.DeviceName].Item2.Contains(applicationSetting.ResolutionSettings))
                        {
                            PerformResolutionChange(screen, applicationSetting.ResolutionSettings);
                        }
                        _gameScreen = screen;
                        if (ApplyGameVibranceLevel(_device, screen.DeviceName, applicationSetting.IngameLevel))
                        {
                            VibranceRestoreHelper.RecordGameLevelApplied(screen.DeviceName);
                        }
                    }
                }
                else
                {
                    IntPtr processHandle = e.Handle;

                    if (!_device.IsWindowActive(ref processHandle))
                        return;

                    //test if a resolution change is needed
                    Screen currentScreen = Screen.FromHandle(processHandle);
                    if (_vibranceInfo.neverChangeResolution == false &&
                        _gameScreen != null &&
                        _gameScreen.Equals(currentScreen) &&
                        _windowsResolutionSettings.ContainsKey(currentScreen.DeviceName) &&
                        IsResolutionChangeNeeded(currentScreen, _windowsResolutionSettings[currentScreen.DeviceName].Item1))
                    {
                        PerformResolutionChange(currentScreen, _windowsResolutionSettings[currentScreen.DeviceName].Item1);
                    }

                    // Deliberately does NOT scope this to currentScreen (issues #95, #144):
                    // restoring only "the screen that currently has focus" left a game's monitor
                    // saturated after an alt-tab to another monitor while the game was still open,
                    // and left it saturated forever after the game exited, if the desktop's
                    // foreground never happened to land back on that same screen first.
                    // RestoreWindowsVibranceLevel instead restores every display this application
                    // actually holds a game level on, plus the primary (see
                    // VibranceRestoreHelper.ComposeRestoreTargets), regardless of where the
                    // foreground currently is. This replaces the pre-fix gate:
                    // "if (_vibranceInfo.affectPrimaryMonitorOnly && !equalsDVCLevel(defaultHandle,
                    // userVibranceSettingDefault)) { if (_gameScreen != null &&
                    // !_gameScreen.DeviceName.Equals(currentScreen.DeviceName)) { return; }
                    // setDVCLevel(...); } else if (...) { ... }" - added in 2017 to stop a game
                    // losing its vibrance when the user clicked something on a second screen while
                    // the game was still visible. It could only express "the mouse is elsewhere",
                    // never "the game is still running", which is what #95/#144 cost.
                    RestoreWindowsVibranceLevel(_device, _vibranceInfo.affectPrimaryMonitorOnly,
                        VibranceRestoreHelper.GetPrimaryDeviceName(), _vibranceInfo.displayHandles,
                        _vibranceInfo.userVibranceSettingDefault, _vibranceInfo.isWindowsLevelKnown);
                }
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

        private void EnumerateDisplayHandles()
        {
            for (int i = 0, displayHandle = 0; displayHandle != -1; i++)
            {
                if (_vibranceInfo.displayHandles == null)
                    _vibranceInfo.displayHandles = new List<int>();

                displayHandle = enumerateNvidiaDisplayHandle(i);
                if (displayHandle != -1)
                    _vibranceInfo.displayHandles.Add(displayHandle);
            }
        }

        /// <summary>
        /// Writes ingameLevel to gameDeviceName's NVIDIA display, unless it is already there.
        /// Returns true only when the write actually landed - only then may the caller record the
        /// display as owing a restore. Returns true without writing anything when the display is
        /// already at ingameLevel. Does not throw on a resolve failure or a failed write - both are
        /// logged once and simply skipped. The underlying P/Invokes themselves are not guarded here
        /// and can still throw, exactly as they could before this seam existed.
        /// </summary>
        internal static bool ApplyGameVibranceLevel(INvidiaVibranceDevice device, string gameDeviceName, int ingameLevel)
        {
            int displayHandle = device.TryResolveDisplayHandle(gameDeviceName);
            if (displayHandle == -1 || displayHandle == 0)
            {
                // 0 is a null NvDisplayHandle. Never fall back to enumerateNvidiaDisplayHandle(0):
                // that arbitrary-display fallback is exactly what issue #60/#36 was.
                LogDisplayFailureOnce(gameDeviceName, string.Format(
                    "Could not resolve an NVIDIA display handle for screen {0}, skipping its ingame vibrance apply", gameDeviceName));
                return false;
            }

            if (device.IsAtLevel(displayHandle, ingameLevel))
            {
                ClearDisplayFailureLog(gameDeviceName);
                return true;
            }

            if (device.SetLevel(displayHandle, ingameLevel))
            {
                ClearDisplayFailureLog(gameDeviceName);
                return true;
            }

            LogDisplayFailureOnce(gameDeviceName, string.Format(
                "Failed to set the ingame vibrance level for screen {0}", gameDeviceName));
            return false;
        }

        /// <summary>
        /// Writes windowsLevel to every display owing a restore (VibranceRestoreHelper's
        /// work-list), plus the display the Windows Vibrance Level itself owns, and nothing else -
        /// deliberately not scoped to whichever screen currently has focus (see the call site in
        /// OnWinEventHook for why: issues #95 and #144). A no-op while isWindowsLevelKnown is
        /// false (see VibranceInfo) - windowsLevel is meaningless before SetVibranceWindowsLevel
        /// has actually run once, and writing it anyway would re-introduce the arbitrary-0 write
        /// InitializeProxy used to make (issue #60/#36) at one remove. allDisplayHandles is
        /// consulted only when affectPrimaryMonitorOnly is false, in which case this preserves the
        /// pre-existing all-displays behaviour exactly (skipping any -1/0 entry EnumerateDisplayHandles
        /// could still hand back) and then drops the whole work-list - that branch never wrote
        /// through it in the first place.
        ///
        /// Per display in the affectPrimaryMonitorOnly branch: unresolvable (-1 or 0) leaves the
        /// display on the work-list for a single P/Invoke (TryResolveDisplayHandle) so the next
        /// foreground event retries it. A display that resolves but fails to write costs three
        /// P/Invokes on that same retry - resolve, the IsAtLevel read-back, then SetLevel - not
        /// one; IsAtLevel only saves a call for a display that turns out to already be correct, not
        /// for one that is genuinely failing to write. A display already at windowsLevel is drained
        /// without ever calling SetLevel.
        /// </summary>
        internal static void RestoreWindowsVibranceLevel(INvidiaVibranceDevice device, bool affectPrimaryMonitorOnly,
            string primaryDeviceName, IList<int> allDisplayHandles, int windowsLevel, bool isWindowsLevelKnown)
        {
            if (!isWindowsLevelKnown)
            {
                return;
            }

            if (!affectPrimaryMonitorOnly)
            {
                if (allDisplayHandles != null && !AllDisplaysAtLevel(device, allDisplayHandles, windowsLevel))
                {
                    for (int i = 0; i < allDisplayHandles.Count; i++)
                    {
                        int handle = allDisplayHandles[i];
                        if (handle == -1 || handle == 0)
                        {
                            continue;
                        }
                        device.SetLevel(handle, windowsLevel);
                    }
                }
                VibranceRestoreHelper.ClearAllGameLevelRecords();
                return;
            }

            List<string> targets = VibranceRestoreHelper.ComposeRestoreTargets(true, primaryDeviceName);
            foreach (string deviceName in targets)
            {
                RestoreOneDisplay(device, deviceName, windowsLevel);
            }
        }

        private static bool AllDisplaysAtLevel(INvidiaVibranceDevice device, IList<int> displayHandles, int level)
        {
            for (int i = 0; i < displayHandles.Count; i++)
            {
                int handle = displayHandles[i];
                if (handle == -1 || handle == 0)
                {
                    continue;
                }
                if (!device.IsAtLevel(handle, level))
                {
                    return false;
                }
            }
            return true;
        }

        private static void RestoreOneDisplay(INvidiaVibranceDevice device, string deviceName, int windowsLevel)
        {
            int displayHandle = device.TryResolveDisplayHandle(deviceName);
            if (displayHandle == -1 || displayHandle == 0)
            {
                LogDisplayFailureOnce(deviceName, string.Format(
                    "Could not resolve an NVIDIA display handle for screen {0}, its Windows vibrance level restore will retry on the next foreground change", deviceName));
                return; // stays on the work-list - see VibranceRestoreHelper.
            }

            if (device.IsAtLevel(displayHandle, windowsLevel))
            {
                VibranceRestoreHelper.ClearGameLevelRecord(deviceName);
                ClearDisplayFailureLog(deviceName);
                return;
            }

            if (device.SetLevel(displayHandle, windowsLevel))
            {
                VibranceRestoreHelper.ClearGameLevelRecord(deviceName);
                ClearDisplayFailureLog(deviceName);
            }
            else
            {
                LogDisplayFailureOnce(deviceName, string.Format(
                    "Failed to restore the Windows vibrance level for screen {0}, it will retry on the next foreground change", deviceName));
            }
        }

        private static void LogDisplayFailureOnce(string deviceName, string message)
        {
            if (_loggedDisplayFailures.Add(deviceName))
            {
                Program.LogSafely(message);
            }
        }

        private static void ClearDisplayFailureLog(string deviceName)
        {
            _loggedDisplayFailures.Remove(deviceName);
        }

        // Exists for VibranceRestoreFixture only - swaps out every static field OnWinEventHook and
        // the apply/restore methods above depend on, so a check can run the real, private static
        // OnWinEventHook (reflection is the only seam into it; it takes no instance) or the
        // Apply/RestoreWindowsVibranceLevel overloads directly, entirely against a fake device and
        // fake settings, with no call to the constructor and so no initializeLibrary() and no real
        // GPU touched. Mirrors VibranceRestoreHelper.ResetForTests.
        internal static void ResetForTests(INvidiaVibranceDevice device, VibranceInfo vibranceInfo,
            List<ApplicationSetting> applicationSettings)
        {
            _device = device ?? new RealNvidiaVibranceDevice();
            _vibranceInfo = vibranceInfo;
            _applicationSettings = applicationSettings ?? new List<ApplicationSetting>();
            _gameScreen = null;
            _loggedDisplayFailures.Clear();
            VibranceRestoreHelper.ResetForTests();
        }

        // The production INvidiaVibranceDevice: the four native calls below against the real
        // vibranceDLL.dll, exactly as OnWinEventHook/GetApplicationDisplayHandle called them
        // directly before this seam existed.
        private class RealNvidiaVibranceDevice : INvidiaVibranceDevice
        {
            public bool IsWindowActive(ref IntPtr hWnd)
            {
                return isWindowActive(ref hWnd);
            }

            public int TryResolveDisplayHandle(string deviceName)
            {
                if (string.IsNullOrEmpty(deviceName))
                {
                    return -1;
                }
                // The marshaller (CharSet.Ansi on the DllImport above) copies deviceName into its
                // own native ANSI buffer for the duration of this one call and frees it afterward -
                // there is nothing here for a caller-side GCHandle to protect. The pin the old
                // GetApplicationDisplayHandle used (GCHandle.Alloc(deviceName,
                // GCHandleType.Pinned)) pinned a managed string that the marshaller was never going
                // to touch directly in the first place.
                return getAssociatedNvidiaDisplayHandle(deviceName, deviceName.Length);
            }

            public bool IsAtLevel(int displayHandle, int level)
            {
                return equalsDVCLevel(displayHandle, level);
            }

            public bool SetLevel(int displayHandle, int level)
            {
                return setDVCLevel(displayHandle, level);
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

        public void SetSleepInterval(int interval)
        {
            _vibranceInfo.sleepInterval = interval;
        }

        public void HandleDvc()
        {
        }

        public void SetAffectPrimaryMonitorOnly(bool affectPrimaryMonitorOnly)
        {
            _vibranceInfo.affectPrimaryMonitorOnly = affectPrimaryMonitorOnly;
        }

        public VibranceInfo GetVibranceInfo()
        {
            return _vibranceInfo;
        }

        public GraphicsAdapter GraphicsAdapter { get; } = GraphicsAdapter.Nvidia;

        public bool UnloadLibraryEx()
        {
            _hook.RemoveWinEventHook();
            return unloadLibrary();
        }

        public void HandleDvcExit()
        {
            // Same restore RestoreWindowsVibranceLevel already does on every non-game foreground
            // event (issue #144: vibrance used to survive the game closing because this used to
            // write only through the hijacked _vibranceInfo.defaultHandle - the game's own display,
            // if the apply branch had ever run - instead of every display actually holding a game
            // level).
            RestoreWindowsVibranceLevel(_device, _vibranceInfo.affectPrimaryMonitorOnly,
                VibranceRestoreHelper.GetPrimaryDeviceName(), _vibranceInfo.displayHandles,
                _vibranceInfo.userVibranceSettingDefault, _vibranceInfo.isWindowsLevelKnown);
        }
    }
}
