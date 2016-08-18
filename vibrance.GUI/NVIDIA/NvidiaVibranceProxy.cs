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
        static extern bool getDVCInfo(ref NV_DISPLAY_DVC_INFO info, int defaultHandle);

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


        public const int NVAPI_MAX_PHYSICAL_GPUS = 64;
        public const int NVAPI_MAX_LEVEL = 63;
        public const int NVAPI_DEFAULT_LEVEL = 0;
        public const int NVAPI_MIN_REFRESH_RATE = 200;
        public const int NVAPI_DEFAULT_REFRESH_RATE = 5000;

        public const string NVAPI_ERROR_INIT_FAILED = "VibranceProxy failed to initialize! Read readme.txt for fix!";
        public const string NVAPI_ERROR_GET_MONITOR_HANDLE = "Couldn't determine the monitor handle CSGO is running in. :( Using main output!";
        public const string NVAPI_WARNING_MULTIPLE_MONITORS = "You are using multiple monitors. Start CSGO, Tab out and click ok after. Do this once then keep vibranceGUI open (means you can start and quit CSGO several times).";
        public const string NVAPI_GLOBAL_OFFENSIVE_WINDOW_NAME = "Counter-Strike: Global Offensive";

        public VIBRANCE_INFO vibranceInfo;

        public NvidiaVibranceProxy(bool isSilenced = false)
        {
            try
            {
                vibranceInfo = new VIBRANCE_INFO();
                bool ret = initializeLibrary();

                int[] gpuHandles = new int[NVAPI_MAX_PHYSICAL_GPUS];
                int[] outputIds = new int[NVAPI_MAX_PHYSICAL_GPUS];
                enumeratePhsyicalGPUs(gpuHandles);

                enumerateDisplayHandles();
               
                vibranceInfo.activeOutput = getActiveOutputs(gpuHandles, outputIds);
                StringBuilder buffer = new StringBuilder(64);
                char[] sz = new char[64];
                getGpuName(gpuHandles, buffer);
                vibranceInfo.szGpuName = buffer.ToString();
                vibranceInfo.defaultHandle = enumerateNvidiaDisplayHandle(0);

                NV_DISPLAY_DVC_INFO info = new NV_DISPLAY_DVC_INFO();
                if (getDVCInfo(ref info, vibranceInfo.defaultHandle))
                {
                    if (info.currentLevel != vibranceInfo.userVibranceSettingDefault)
                    {
                        setDVCLevel(vibranceInfo.defaultHandle, vibranceInfo.userVibranceSettingDefault);
                    }
                }

                vibranceInfo.isInitialized = true;
            }
            catch (Exception)
            {
                MessageBox.Show(NvidiaVibranceProxy.NVAPI_ERROR_INIT_FAILED);
            }

        }

        public int getCsgoDisplayHandle()
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

        public void setApplicationSettings(ref List<ApplicationSetting> refApplicationSettings)
        {
            throw new NotImplementedException();
        }

        public void setShouldRun(bool shouldRun)
        {
            this.vibranceInfo.shouldRun = shouldRun;
        }

        public void setVibranceWindowsLevel(int vibranceWindowsLevel)
        {
            this.vibranceInfo.userVibranceSettingDefault= vibranceWindowsLevel;
        }

        public void setVibranceIngameLevel(int vibranceIngameLevel)
        {
            this.vibranceInfo.userVibranceSettingActive = vibranceIngameLevel;
        }

        public void setSleepInterval(int interval)
        {
            this.vibranceInfo.sleepInterval = interval;
        }

        public void setAffectPrimaryMonitorOnly(bool affectPrimaryMonitorOnly)
        {
            this.vibranceInfo.affectPrimaryMonitorOnly = affectPrimaryMonitorOnly;
        }
        
        public void handleDVC()
        {
            bool isChanged = false;
            while (vibranceInfo.shouldRun)
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

                            if (sb != null && sb.ToString().Equals(NvidiaVibranceProxy.NVAPI_GLOBAL_OFFENSIVE_WINDOW_NAME))
                            {
                                if (Screen.AllScreens.Length > 1)
                                {
                                    int csgoHandle = getCsgoDisplayHandle();
                                    if (csgoHandle != -1)
                                    {
                                        vibranceInfo.defaultHandle = csgoHandle;
                                    }
                                }

                                if (!equalsDVCLevel(vibranceInfo.defaultHandle, vibranceInfo.userVibranceSettingActive))
                                {
                                    setDVCLevel(vibranceInfo.defaultHandle, vibranceInfo.userVibranceSettingActive);
                                    isChanged = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (isChanged /*&& !vibranceInfo.keepActive*/)
                        {
                            if (vibranceInfo.affectPrimaryMonitorOnly)
                            {
                                setDVCLevel(vibranceInfo.defaultHandle, vibranceInfo.userVibranceSettingDefault);
                            }
                            else
                            {
                                vibranceInfo.displayHandles.ForEach(handle => setDVCLevel(handle, vibranceInfo.userVibranceSettingDefault));
                            }
                            isChanged = false;
                        }
                    }
                }
                else
                {
                    if (vibranceInfo.affectPrimaryMonitorOnly && !equalsDVCLevel(vibranceInfo.defaultHandle, vibranceInfo.userVibranceSettingDefault))
                    {
                        setDVCLevel(vibranceInfo.defaultHandle, vibranceInfo.userVibranceSettingDefault);
                        isChanged = false;
                    }
                    else if(!vibranceInfo.affectPrimaryMonitorOnly && !vibranceInfo.displayHandles.TrueForAll(handle => equalsDVCLevel(handle, vibranceInfo.userVibranceSettingDefault)))
                    {
                        vibranceInfo.displayHandles.ForEach(handle => setDVCLevel(handle, vibranceInfo.userVibranceSettingDefault));
                        isChanged = false;
                    }
                }
                System.Threading.Thread.Sleep(vibranceInfo.sleepInterval);
            }
            handleDVCExit();
        }


        public bool unloadLibraryEx()
        {
            return unloadLibrary();
        }

        public void handleDVCExit()
        {
            if (vibranceInfo.affectPrimaryMonitorOnly)
            {
                setDVCLevel(vibranceInfo.defaultHandle, vibranceInfo.userVibranceSettingDefault);
            }
            else if (!vibranceInfo.displayHandles.TrueForAll(handle => equalsDVCLevel(handle, vibranceInfo.userVibranceSettingDefault)))
                vibranceInfo.displayHandles.ForEach(handle => setDVCLevel(handle, vibranceInfo.userVibranceSettingDefault));
        }

        private void enumerateDisplayHandles()
        {
            for (int i = 0, displayHandle = 0; displayHandle != -1; i++)
            {
                if (vibranceInfo.displayHandles == null)
                    vibranceInfo.displayHandles = new List<int>();

                displayHandle = enumerateNvidiaDisplayHandle(i);
                if (displayHandle != -1)
                    vibranceInfo.displayHandles.Add(displayHandle);
            }
        }

        public VIBRANCE_INFO getVibranceInfo()
        {
            return vibranceInfo;
        }

        public bool setDVCLevel_extern(int defaultHandle, int level)
        {
            if(vibranceInfo.isInitialized)
                return setDVCLevel(defaultHandle, level);
            return false;
        }

        public bool equalsDVCLevel_extern(int defaultHandle, int level)
        {
            if (vibranceInfo.isInitialized)
                return equalsDVCLevel(defaultHandle, level);
            return false;
        }
    }
}
