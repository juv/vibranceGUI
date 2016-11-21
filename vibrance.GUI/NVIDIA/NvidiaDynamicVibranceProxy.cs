using System;
using System.Collections.Generic;
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

        public const string NvapiErrorInitFailed = "Falha ao iniciar o vibranceProxy! Leia readme.txt para corrigir!";
        public const string NvapiErrorSystypeUnsupported = "VibranceProxy detectado que você está executando um notebook com uma GPU NVIDIA. " +
            "NVIDIA GPU para notebook não suportam vibrance pois os drivers não contêm Digital Vibrance! " +
            "Está faltando a opção Vibrance Digital em seu painel de controla NVIDIA. VibranceGUI não pode ser executado em seu sistema.";
        public const string NvapiErrorSystypeUnknown = "Falha ao inicializar VibranceProxy! Seu tipo de GPU (Desktop / Laptop) é desconhecido!";

        private static VibranceInfo _vibranceInfo;
        private static List<ApplicationSetting> _applicationSettings;
        private static ResolutionModeWrapper _windowsResolutionSettings;
        private WinEventHook _hook;
        private static Screen _gameScreen;

        public NvidiaDynamicVibranceProxy(List<ApplicationSetting> savedApplicationSettings, ResolutionModeWrapper currentWindowsResolutionSettings)
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
                MessageBox.Show(NvidiaDynamicVibranceProxy.NvapiErrorInitFailed, "vibranceGUI Erro", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                        MessageBox.Show(NvidiaDynamicVibranceProxy.NvapiErrorSystypeUnknown, "vibranceGUI Erro", 
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
            _vibranceInfo.defaultHandle = enumerateNvidiaDisplayHandle(0);

            NvDisplayDvcInfo info = new NvDisplayDvcInfo();
            if (getDVCInfo(ref info, _vibranceInfo.defaultHandle))
            {
                if (info.currentLevel != _vibranceInfo.userVibranceSettingDefault)
                {
                    setDVCLevel(_vibranceInfo.defaultHandle, _vibranceInfo.userVibranceSettingDefault);
                }
            }

            _vibranceInfo.isInitialized = true;
        }

        private static void OnWinEventHook(object sender, WinEventHookEventArgs e)
        {
            if (_applicationSettings.Count > 0)
            {
                ApplicationSetting applicationSetting = _applicationSettings.FirstOrDefault(x => x.Name.Equals(e.ProcessName));
                if (applicationSetting != null)
                {
                    //test if a resolution change is needed
                    Screen screen = Screen.FromHandle(e.Handle);
                    if (applicationSetting.IsResolutionChangeNeeded && IsResolutionChangeNeeded(screen, applicationSetting.ResolutionSettings))
                    {
                        _gameScreen = screen;
                        PerformResolutionChange(screen, applicationSetting.ResolutionSettings);
                    }

                    //test if changing the vibrance value is needed
                    int displayHandle = GetApplicationDisplayHandle(e.Handle);
                    if (displayHandle != -1 && !equalsDVCLevel(displayHandle, applicationSetting.IngameLevel))
                    {
                        _vibranceInfo.defaultHandle = displayHandle;
                        setDVCLevel(_vibranceInfo.defaultHandle, applicationSetting.IngameLevel);
                    }
                }
                else
                {
                    IntPtr processHandle = e.Handle;
                    if (!isWindowActive(ref processHandle))
                        return;

                    //test if a resolution change is needed
                    Screen screen = Screen.FromHandle(processHandle);
                    if (_gameScreen != null && _gameScreen.Equals(screen) && IsResolutionChangeNeeded(screen, _windowsResolutionSettings))
                    {
                        PerformResolutionChange(screen, _windowsResolutionSettings);
                    }

                    //test if changing the vibrance value is needed
                    if (_vibranceInfo.affectPrimaryMonitorOnly && !equalsDVCLevel(_vibranceInfo.defaultHandle, _vibranceInfo.userVibranceSettingDefault))
                    {
                        setDVCLevel(_vibranceInfo.defaultHandle, _vibranceInfo.userVibranceSettingDefault);
                    }
                    else if (!_vibranceInfo.affectPrimaryMonitorOnly && !_vibranceInfo.displayHandles.TrueForAll(handle => equalsDVCLevel(handle, _vibranceInfo.userVibranceSettingDefault)))
                    {
                        _vibranceInfo.displayHandles.ForEach(handle => setDVCLevel(handle, _vibranceInfo.userVibranceSettingDefault));
                    }
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

        private static int GetApplicationDisplayHandle(IntPtr hWnd)
        {
            if (hWnd != IntPtr.Zero)
            {
                Screen primaryScreen = System.Windows.Forms.Screen.FromHandle(hWnd);
                if (primaryScreen != null)
                {
                    string deviceName = primaryScreen.DeviceName;
                    GCHandle handle = GCHandle.Alloc(deviceName, GCHandleType.Pinned);
                    int id = getAssociatedNvidiaDisplayHandle(deviceName, deviceName.Length);
                    handle.Free();

                    return id;
                }
            }
            return -1;
        }

        public void SetApplicationSettings(List<ApplicationSetting> refApplicationSettings)
        {
            _applicationSettings = refApplicationSettings;
        }

        public void SetShouldRun(bool shouldRun)
        {
            _vibranceInfo.shouldRun = shouldRun;
        }

        public void SetVibranceWindowsLevel(int vibranceWindowsLevel)
        {
            _vibranceInfo.userVibranceSettingDefault = vibranceWindowsLevel;
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
            if (_vibranceInfo.affectPrimaryMonitorOnly)
            {
                setDVCLevel(_vibranceInfo.defaultHandle, _vibranceInfo.userVibranceSettingDefault);
            }
            else if (!_vibranceInfo.displayHandles.TrueForAll(handle => equalsDVCLevel(handle, _vibranceInfo.userVibranceSettingDefault)))
                _vibranceInfo.displayHandles.ForEach(handle => setDVCLevel(handle, _vibranceInfo.userVibranceSettingDefault));
        }
    }
}
