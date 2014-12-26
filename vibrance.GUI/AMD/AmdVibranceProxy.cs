using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using ATI.ADL;

namespace vibrance.GUI
{
    public class AmdVibranceProxy : IVibranceProxy
    {
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
        
        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        static extern int GetWindowTextLength([In] IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        static extern int GetWindowTextA([In] IntPtr hWnd, [In, Out] StringBuilder lpString, [In] int nMaxCount);

        public const int AMD_MAX_LEVEL = 63;
        public const int AMD_DEFAULT_LEVEL = 0;
        public const int AMD_MIN_REFRESH_RATE = 200;
        public const int AMD_DEFAULT_REFRESH_RATE = 5000;

        public const string NVAPI_ERROR_INIT_FAILED = "VibranceProxy failed to initialize! Read readme.txt for fix!";
        public const string NVAPI_ERROR_GET_MONITOR_HANDLE = "Couldn't determine the monitor handle CSGO is running in. :( Using main output!";
        public const string NVAPI_WARNING_MULTIPLE_MONITORS = "You are using multiple monitors. Start CSGO, Tab out and click ok after. Do this once then keep vibranceGUI open (means you can start and quit CSGO several times).";
        public const string NVAPI_GLOBAL_OFFENSIVE_WINDOW_NAME = "Counter-Strike: Global Offensive";

        public VIBRANCE_INFO vibranceInfo;

        public AmdVibranceProxy(bool isSilenced = false)
        {
            try
            {
                vibranceInfo = new VIBRANCE_INFO();
                bool ret = initializeLibrary();

                //alle display handles auslesen, damit vibrance auf allen displays angepasst wird
                enumerateDisplayHandles();
               
                //vielleicht unnötig bei AMD?
                //int[] gpuHandles = new int[NVAPI_MAX_PHYSICAL_GPUS];
                //int[] outputIds = new int[NVAPI_MAX_PHYSICAL_GPUS];
                //vibranceInfo.activeOutput = getActiveOutputs();

                vibranceInfo.szGpuName = getGpuName();

                //standard display handle (hauptmonitor in windows)
                vibranceInfo.defaultHandle = enumerateAmdDisplayHandle(0);


                //Wenn aktuelles Vibrance nicht den User-Settings entspricht ändern
                //info musst du abändern, für irgendwas AMD-mäßige$
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
                MessageBox.Show(AmdVibranceProxy.NVAPI_ERROR_INIT_FAILED);
            }

        }

        private int getCsgoDisplayHandle()
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
                    int id = getAssociatedAmdDisplayHandle(deviceName, deviceName.Length);
                    handle.Free();

                    return id;
                }
            }

            return -1;
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

        public void setKeepActive(bool keepActive)
        {
            this.vibranceInfo.keepActive = keepActive;
        }

        public void setSleepInterval(int interval)
        {
            this.vibranceInfo.sleepInterval = interval;
        }
        
        //Keine Anpassung nötig, wenn andere Methoden ordentlich angepasst sind
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

                            if (sb != null && sb.ToString().Equals(AmdVibranceProxy.NVAPI_GLOBAL_OFFENSIVE_WINDOW_NAME))
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
                        if (isChanged && !vibranceInfo.keepActive)
                        {
                            vibranceInfo.displayHandles.ForEach(handle => setDVCLevel(handle, vibranceInfo.userVibranceSettingDefault));
                            isChanged = false;
                        }
                    }
                }
                else
                {
                    if (isChanged || !vibranceInfo.displayHandles.TrueForAll(handle => equalsDVCLevel(handle, vibranceInfo.userVibranceSettingDefault)))
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
            if (!vibranceInfo.displayHandles.TrueForAll(handle => equalsDVCLevel(handle, vibranceInfo.userVibranceSettingDefault)))
                vibranceInfo.displayHandles.ForEach(handle => setDVCLevel(handle, vibranceInfo.userVibranceSettingDefault));
        }

        private void enumerateDisplayHandles()
        {
            for (int i = 0, displayHandle = 0; displayHandle != -1; i++)
            {
                if (vibranceInfo.displayHandles == null)
                    vibranceInfo.displayHandles = new List<int>();

                displayHandle = enumerateAmdDisplayHandle(i);
                if (displayHandle != -1)
                    vibranceInfo.displayHandles.Add(displayHandle);
            }
        }

        //TODO: AMD Anpassung. index (0 bis displayCount-1) -> Display Handle 
        private int enumerateAmdDisplayHandle(int index)
        {
            return 0;
        }


        private bool initializeLibrary()
        {
            int ADLRet = -1;
            try
            {
                if (null != ADL.ADL_Main_Control_Create)
                {
                    // Second parameter is 1: Get only the present adapters
                    ADLRet = ADL.ADL_Main_Control_Create(ADL.ADL_Main_Memory_Alloc, 1);
                    if (ADL.ADL_SUCCESS == ADLRet)
                    {
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Error initializing AMD ADL: " + ADLRet.ToString());
            }
            return false;
        }

        private bool unloadLibrary()
        {
            if (null != ADL.ADL_Main_Control_Destroy)
            {
                ADL.ADL_Main_Control_Destroy();
                return true;
            }
            return false;
        }

        //TODO: AMD Anpassung
        private bool equalsDVCLevel(int handle, int level)
        {
            return false;
        }

        //TODO: AMD Anpassung ADL.ADL_Display_Color_Set
        private bool setDVCLevel(int handle, int level)
        {
            return false;
        }

        //TODO: AMD Anpassung. Vielleicht unnötig bei AMD
        private int getActiveOutputs(out int[] gpuHandles, out int[] outputIds)
        {
            gpuHandles = null;
            outputIds = null;
            return 0;

            //return *outputIds[0];
        }

        //TODO: AMD Anpassung. DeviceName (z.B. \\.\DISPLAY1) -> Display Handle 
        private int getAssociatedAmdDisplayHandle(string deviceName, int length)
        {
            return 0;
        }

        //TODO: AMD Anpassung 
        private string getGpuName()
        {
            return null;
        }

        //TODO: AMD Anpassung. NV_DISPLAY_DVC_INFO ersetzen
        private bool getDVCInfo(ref NV_DISPLAY_DVC_INFO info, int defaultHandle)
        {
            return false;
        }
    }
}
