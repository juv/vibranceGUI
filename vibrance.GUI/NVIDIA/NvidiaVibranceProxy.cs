using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using vibrance.GUI.common;

namespace vibrance.GUI.NVIDIA
{
    public class NvidiaVibranceProxy : IVibranceProxy
    {
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
            EntryPoint = "?isCsgoActive@vibrance@vibranceDLL@@QAE_NPAPAUHWND__@@@Z",
            CallingConvention = CallingConvention.StdCall,
            CharSet = CharSet.Auto)]
        static extern bool isCsgoActive(ref IntPtr hwnd);

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


        public const int NvapiMaxPhysicalGpus = 64;
        public const int NvapiMaxLevel = 63;
        public const int NvapiDefaultLevel = 0;
        public const int NvapiMinRefreshRate = 200;
        public const int NvapiDefaultRefreshRate = 5000;

        public const string NvapiErrorInitFailed = "VibranceProxy failed to initialize! Read readme.txt for fix!";
        public const string NvapiErrorGetMonitorHandle = "Couldn't determine the monitor handle CSGO is running in. :( Using main output!";
        public const string NvapiWarningMultipleMonitors = "You are using multiple monitors. Start CSGO, Tab out and click ok after. Do this once then keep vibranceGUI open (means you can start and quit CSGO several times).";
        public const string NvapiGlobalOffensiveWindowName = "Counter-Strike: Global Offensive";

        public VibranceInfo VibranceInfo;

        public NvidiaVibranceProxy(bool isSilenced = false)
        {
            try
            {
                VibranceInfo = new VibranceInfo();
                bool ret = initializeLibrary();

                int[] gpuHandles = new int[NvapiMaxPhysicalGpus];
                int[] outputIds = new int[NvapiMaxPhysicalGpus];
                enumeratePhsyicalGPUs(gpuHandles);

                EnumerateDisplayHandles();
               
                VibranceInfo.activeOutput = getActiveOutputs(gpuHandles, outputIds);
                StringBuilder buffer = new StringBuilder(64);
                char[] sz = new char[64];
                getGpuName(gpuHandles, buffer);
                VibranceInfo.szGpuName = buffer.ToString();
                VibranceInfo.defaultHandle = enumerateNvidiaDisplayHandle(0);

                NvDisplayDvcInfo info = new NvDisplayDvcInfo();
                if (getDVCInfo(ref info, VibranceInfo.defaultHandle))
                {
                    if (info.currentLevel != VibranceInfo.userVibranceSettingDefault)
                    {
                        setDVCLevel(VibranceInfo.defaultHandle, VibranceInfo.userVibranceSettingDefault);
                    }
                }

                VibranceInfo.isInitialized = true;
            }
            catch (Exception)
            {
                MessageBox.Show(NvidiaVibranceProxy.NvapiErrorInitFailed);
            }

        }

        public int GetCsgoDisplayHandle()
        {
            Screen primaryScreen = null;
            IntPtr hwnd = IntPtr.Zero;
            if (isCsgoStarted(ref hwnd) && hwnd != IntPtr.Zero)
            {
                primaryScreen = System.Windows.Forms.Screen.FromHandle(hwnd);
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
            throw new NotImplementedException();
        }

        public void SetShouldRun(bool shouldRun)
        {
            this.VibranceInfo.shouldRun = shouldRun;
        }

        public void SetVibranceWindowsLevel(int vibranceWindowsLevel)
        {
            this.VibranceInfo.userVibranceSettingDefault= vibranceWindowsLevel;
        }

        public void SetVibranceIngameLevel(int vibranceIngameLevel)
        {
            this.VibranceInfo.userVibranceSettingActive = vibranceIngameLevel;
        }

        public void SetSleepInterval(int interval)
        {
            this.VibranceInfo.sleepInterval = interval;
        }

        public void SetAffectPrimaryMonitorOnly(bool affectPrimaryMonitorOnly)
        {
            this.VibranceInfo.affectPrimaryMonitorOnly = affectPrimaryMonitorOnly;
        }
        
        public void HandleDvc()
        {
            bool isChanged = false;
            while (VibranceInfo.shouldRun)
            {
                IntPtr hwnd = IntPtr.Zero;
                if (isCsgoStarted(ref hwnd) && hwnd != IntPtr.Zero)
                {
                    if (isCsgoActive(ref hwnd))
                    {
                        int nLen = GetWindowTextLength(hwnd);
                        if (nLen > 0)
                        {
                            int length = GetWindowTextLength(hwnd);
                            StringBuilder sb = new StringBuilder(length + 1);
                            GetWindowTextA(hwnd, sb, sb.Capacity);

                            if (sb != null && sb.ToString().Equals(NvidiaVibranceProxy.NvapiGlobalOffensiveWindowName))
                            {
                                if (Screen.AllScreens.Length > 1)
                                {
                                    int csgoHandle = GetCsgoDisplayHandle();
                                    if (csgoHandle != -1)
                                    {
                                        VibranceInfo.defaultHandle = csgoHandle;
                                    }
                                }

                                if (!equalsDVCLevel(VibranceInfo.defaultHandle, VibranceInfo.userVibranceSettingActive))
                                {
                                    setDVCLevel(VibranceInfo.defaultHandle, VibranceInfo.userVibranceSettingActive);
                                    isChanged = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (isChanged /*&& !vibranceInfo.keepActive*/)
                        {
                            if (VibranceInfo.affectPrimaryMonitorOnly)
                            {
                                setDVCLevel(VibranceInfo.defaultHandle, VibranceInfo.userVibranceSettingDefault);
                            }
                            else
                            {
                                VibranceInfo.displayHandles.ForEach(handle => setDVCLevel(handle, VibranceInfo.userVibranceSettingDefault));
                            }
                            isChanged = false;
                        }
                    }
                }
                else
                {
                    if (VibranceInfo.affectPrimaryMonitorOnly && !equalsDVCLevel(VibranceInfo.defaultHandle, VibranceInfo.userVibranceSettingDefault))
                    {
                        setDVCLevel(VibranceInfo.defaultHandle, VibranceInfo.userVibranceSettingDefault);
                        isChanged = false;
                    }
                    else if(!VibranceInfo.affectPrimaryMonitorOnly && !VibranceInfo.displayHandles.TrueForAll(handle => equalsDVCLevel(handle, VibranceInfo.userVibranceSettingDefault)))
                    {
                        VibranceInfo.displayHandles.ForEach(handle => setDVCLevel(handle, VibranceInfo.userVibranceSettingDefault));
                        isChanged = false;
                    }
                }
                System.Threading.Thread.Sleep(VibranceInfo.sleepInterval);
            }
            HandleDvcExit();
        }


        public bool UnloadLibraryEx()
        {
            return unloadLibrary();
        }

        public void HandleDvcExit()
        {
            if (VibranceInfo.affectPrimaryMonitorOnly)
            {
                setDVCLevel(VibranceInfo.defaultHandle, VibranceInfo.userVibranceSettingDefault);
            }
            else if (!VibranceInfo.displayHandles.TrueForAll(handle => equalsDVCLevel(handle, VibranceInfo.userVibranceSettingDefault)))
                VibranceInfo.displayHandles.ForEach(handle => setDVCLevel(handle, VibranceInfo.userVibranceSettingDefault));
        }

        private void EnumerateDisplayHandles()
        {
            for (int i = 0, displayHandle = 0; displayHandle != -1; i++)
            {
                if (VibranceInfo.displayHandles == null)
                    VibranceInfo.displayHandles = new List<int>();

                displayHandle = enumerateNvidiaDisplayHandle(i);
                if (displayHandle != -1)
                    VibranceInfo.displayHandles.Add(displayHandle);
            }
        }

        public VibranceInfo GetVibranceInfo()
        {
            return VibranceInfo;
        }

        public GraphicsAdapter GraphicsAdapter { get; } = GraphicsAdapter.Nvidia;

        public bool setDVCLevel_extern(int defaultHandle, int level)
        {
            if(VibranceInfo.isInitialized)
                return setDVCLevel(defaultHandle, level);
            return false;
        }

        public bool equalsDVCLevel_extern(int defaultHandle, int level)
        {
            if (VibranceInfo.isInitialized)
                return equalsDVCLevel(defaultHandle, level);
            return false;
        }
    }
}
