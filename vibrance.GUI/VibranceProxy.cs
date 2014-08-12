using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace vibrance.GUI
{

    [StructLayout(LayoutKind.Sequential)]
    public struct NV_DISPLAY_DVC_INFO
    {
        public uint version;              
        public int currentLevel;       
        public int minLevel;           
        public int maxLevel;           
        public NV_DISPLAY_DVC_INFO(uint version, int currentLevel, int minLevel, int maxLevel)
        {
            this.version = version;
            this.currentLevel = currentLevel;
            this.minLevel = minLevel;
            this.maxLevel = maxLevel;
        }
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct VIBRANCE_INFO
    {
        public bool isInitialized;
        public int activeOutput;
        public int defaultHandle;
        public int userVibranceSettingDefault;
        public int userVibranceSettingActive;
        public String szGpuName;
        public bool shouldRun;
        public bool keepActive;
        public int sleepInterval;

        public VIBRANCE_INFO(bool isInitialized, int activeOutput, int defaultHandle, int userVibranceSettingDefault, int userVibranceSettingActive, String szGpuName, bool shouldRun, bool keepActive, int sleepInterval)
        {
            this.isInitialized = isInitialized;
            this.activeOutput = activeOutput;
            this.defaultHandle = defaultHandle;
            this.userVibranceSettingActive = userVibranceSettingActive;
            this.userVibranceSettingDefault = userVibranceSettingDefault;
            this.szGpuName = szGpuName;
            this.shouldRun = shouldRun;
            this.keepActive = keepActive;
            this.sleepInterval = sleepInterval;
        }
    }


    enum _NvAPI_Status
    {
        NVAPI_OK = 0, NVAPI_ERROR = -1, NVAPI_LIBRARY_NOT_FOUND = -2, NVAPI_NO_IMPLEMENTATION = -3,
        NVAPI_API_NOT_INITIALIZED = -4, NVAPI_INVALID_ARGUMENT = -5, NVAPI_NVIDIA_DEVICE_NOT_FOUND = -6, NVAPI_END_ENUMERATION = -7,
        NVAPI_INVALID_HANDLE = -8, NVAPI_INCOMPATIBLE_STRUCT_VERSION = -9, NVAPI_HANDLE_INVALIDATED = -10, NVAPI_OPENGL_CONTEXT_NOT_CURRENT = -11,
        NVAPI_INVALID_POINTER = -14, NVAPI_NO_GL_EXPERT = -12, NVAPI_INSTRUMENTATION_DISABLED = -13, NVAPI_NO_GL_NSIGHT = -15,
        NVAPI_EXPECTED_LOGICAL_GPU_HANDLE = -100, NVAPI_EXPECTED_PHYSICAL_GPU_HANDLE = -101, NVAPI_EXPECTED_DISPLAY_HANDLE = -102, NVAPI_INVALID_COMBINATION = -103,
        NVAPI_NOT_SUPPORTED = -104, NVAPI_PORTID_NOT_FOUND = -105, NVAPI_EXPECTED_UNATTACHED_DISPLAY_HANDLE = -106, NVAPI_INVALID_PERF_LEVEL = -107,
        NVAPI_DEVICE_BUSY = -108, NVAPI_NV_PERSIST_FILE_NOT_FOUND = -109, NVAPI_PERSIST_DATA_NOT_FOUND = -110, NVAPI_EXPECTED_TV_DISPLAY = -111,
        NVAPI_EXPECTED_TV_DISPLAY_ON_DCONNECTOR = -112, NVAPI_NO_ACTIVE_SLI_TOPOLOGY = -113, NVAPI_SLI_RENDERING_MODE_NOTALLOWED = -114, NVAPI_EXPECTED_DIGITAL_FLAT_PANEL = -115,
        NVAPI_ARGUMENT_EXCEED_MAX_SIZE = -116, NVAPI_DEVICE_SWITCHING_NOT_ALLOWED = -117, NVAPI_TESTING_CLOCKS_NOT_SUPPORTED = -118, NVAPI_UNKNOWN_UNDERSCAN_CONFIG = -119,
        NVAPI_TIMEOUT_RECONFIGURING_GPU_TOPO = -120, NVAPI_DATA_NOT_FOUND = -121, NVAPI_EXPECTED_ANALOG_DISPLAY = -122, NVAPI_NO_VIDLINK = -123,
        NVAPI_REQUIRES_REBOOT = -124, NVAPI_INVALID_HYBRID_MODE = -125, NVAPI_MIXED_TARGET_TYPES = -126, NVAPI_SYSWOW64_NOT_SUPPORTED = -127,
        NVAPI_IMPLICIT_SET_GPU_TOPOLOGY_CHANGE_NOT_ALLOWED = -128, NVAPI_REQUEST_USER_TO_CLOSE_NON_MIGRATABLE_APPS = -129, NVAPI_OUT_OF_MEMORY = -130, NVAPI_WAS_STILL_DRAWING = -131,
        NVAPI_FILE_NOT_FOUND = -132, NVAPI_TOO_MANY_UNIQUE_STATE_OBJECTS = -133, NVAPI_INVALID_CALL = -134, NVAPI_D3D10_1_LIBRARY_NOT_FOUND = -135,
        NVAPI_FUNCTION_NOT_FOUND = -136, NVAPI_INVALID_USER_PRIVILEGE = -137, NVAPI_EXPECTED_NON_PRIMARY_DISPLAY_HANDLE = -138, NVAPI_EXPECTED_COMPUTE_GPU_HANDLE = -139,
        NVAPI_STEREO_NOT_INITIALIZED = -140, NVAPI_STEREO_REGISTRY_ACCESS_FAILED = -141, NVAPI_STEREO_REGISTRY_PROFILE_TYPE_NOT_SUPPORTED = -142, NVAPI_STEREO_REGISTRY_VALUE_NOT_SUPPORTED = -143,
        NVAPI_STEREO_NOT_ENABLED = -144, NVAPI_STEREO_NOT_TURNED_ON = -145, NVAPI_STEREO_INVALID_DEVICE_INTERFACE = -146, NVAPI_STEREO_PARAMETER_OUT_OF_RANGE = -147,
        NVAPI_STEREO_FRUSTUM_ADJUST_MODE_NOT_SUPPORTED = -148, NVAPI_TOPO_NOT_POSSIBLE = -149, NVAPI_MODE_CHANGE_FAILED = -150, NVAPI_D3D11_LIBRARY_NOT_FOUND = -151,
        NVAPI_INVALID_ADDRESS = -152, NVAPI_STRING_TOO_SMALL = -153, NVAPI_MATCHING_DEVICE_NOT_FOUND = -154, NVAPI_DRIVER_RUNNING = -155,
        NVAPI_DRIVER_NOTRUNNING = -156, NVAPI_ERROR_DRIVER_RELOAD_REQUIRED = -157, NVAPI_SET_NOT_ALLOWED = -158, NVAPI_ADVANCED_DISPLAY_TOPOLOGY_REQUIRED = -159,
        NVAPI_SETTING_NOT_FOUND = -160, NVAPI_SETTING_SIZE_TOO_LARGE = -161, NVAPI_TOO_MANY_SETTINGS_IN_PROFILE = -162, NVAPI_PROFILE_NOT_FOUND = -163,
        NVAPI_PROFILE_NAME_IN_USE = -164, NVAPI_PROFILE_NAME_EMPTY = -165, NVAPI_EXECUTABLE_NOT_FOUND = -166, NVAPI_EXECUTABLE_ALREADY_IN_USE = -167,
        NVAPI_DATATYPE_MISMATCH = -168, NVAPI_PROFILE_REMOVED = -169, NVAPI_UNREGISTERED_RESOURCE = -170, NVAPI_ID_OUT_OF_RANGE = -171,
        NVAPI_DISPLAYCONFIG_VALIDATION_FAILED = -172, NVAPI_DPMST_CHANGED = -173, NVAPI_INSUFFICIENT_BUFFER = -174, NVAPI_ACCESS_DENIED = -175,
        NVAPI_MOSAIC_NOT_ACTIVE = -176, NVAPI_SHARE_RESOURCE_RELOCATED = -177, NVAPI_REQUEST_USER_TO_DISABLE_DWM = -178, NVAPI_D3D_DEVICE_LOST = -179,
        NVAPI_INVALID_CONFIGURATION = -180, NVAPI_STEREO_HANDSHAKE_NOT_DONE = -181, NVAPI_EXECUTABLE_PATH_IS_AMBIGUOUS = -182, NVAPI_DEFAULT_STEREO_PROFILE_IS_NOT_DEFINED = -183,
        NVAPI_DEFAULT_STEREO_PROFILE_DOES_NOT_EXIST = -184, NVAPI_CLUSTER_ALREADY_EXISTS = -185, NVAPI_DPMST_DISPLAY_ID_EXPECTED = -186, NVAPI_INVALID_DISPLAY_ID = -187,
        NVAPI_STREAM_IS_OUT_OF_SYNC = -188, NVAPI_INCOMPATIBLE_AUDIO_DRIVER = -189, NVAPI_VALUE_ALREADY_SET = -190, NVAPI_TIMEOUT = -191,
        NVAPI_GPU_WORKSTATION_FEATURE_INCOMPLETE = -192, NVAPI_STEREO_INIT_ACTIVATION_NOT_DONE = -193, NVAPI_SYNC_NOT_ACTIVE = -194, NVAPI_SYNC_MASTER_NOT_FOUND = -195,
        NVAPI_INVALID_SYNC_TOPOLOGY = -196
    };
    
    [Flags()]
    public enum DisplayDeviceStateFlags : int
    {
        AttachedToDesktop = 0x1,
        MultiDriver = 0x2,
        PrimaryDevice = 0x4,
        MirroringDriver = 0x8,
        VGACompatible = 0x10,
        Removable = 0x20,
        ModesPruned = 0x8000000,
        Remote = 0x4000000,
        Disconnect = 0x2000000
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct DISPLAY_DEVICE
    {
        [MarshalAs(UnmanagedType.U4)]
        public int cb;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceString;
        [MarshalAs(UnmanagedType.U4)]
        public DisplayDeviceStateFlags StateFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceKey;
    }



    public class VibranceProxy
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
            EntryPoint = "?getActiveOutputs@vibrance@vibranceDLL@@QAEHQAPAH@Z",
            CallingConvention = CallingConvention.StdCall,
            CharSet = CharSet.Auto)]
        static extern int getActiveOutputs([In, Out] int[] gpuHandles);

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
            EntryPoint = "?enumerateNvidiaDisplayHandle@vibrance@vibranceDLL@@QAEHXZ",
            CallingConvention = CallingConvention.StdCall,
            CharSet = CharSet.Auto)]
        static extern int enumerateNvidiaDisplayHandle();

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

        [DllImport("user32.dll")]
        static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

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

        public VibranceProxy(bool multipleMonitors, bool isSilenced = false)
        {
            try
            {
                vibranceInfo = new VIBRANCE_INFO();
                bool ret = initializeLibrary();
                int[] gpuHandles = new int[NVAPI_MAX_PHYSICAL_GPUS];
                enumeratePhsyicalGPUs(gpuHandles);
                vibranceInfo.activeOutput = getActiveOutputs(gpuHandles);

                StringBuilder buffer = new StringBuilder(64);
                char[] sz = new char[64];
                getGpuName(gpuHandles, buffer);
                vibranceInfo.szGpuName = buffer.ToString();
                vibranceInfo.defaultHandle = enumerateNvidiaDisplayHandle();

                if (multipleMonitors)
                {
                    adjustMultipleMonitorsSetting(multipleMonitors, isSilenced);
                }

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
                MessageBox.Show(VibranceProxy.NVAPI_ERROR_INIT_FAILED);
            }

        }

        public bool adjustMultipleMonitorsSetting(bool flag, bool isSilenced = false)
        {
            if (flag)
            {
                if (Screen.AllScreens.Length > 1)
                {
                    if (!isSilenced)
                    {
                        MessageBox.Show(VibranceProxy.NVAPI_WARNING_MULTIPLE_MONITORS);
                        int csgoHandle = getCsgoDisplayHandle();
                        if (csgoHandle != -1)
                        {
                            vibranceInfo.defaultHandle = csgoHandle;
                            return true;
                        }
                        MessageBox.Show(VibranceProxy.NVAPI_ERROR_GET_MONITOR_HANDLE);
                    }
                    vibranceInfo.defaultHandle = enumerateNvidiaDisplayHandle();
                }
            }
            else
            {
                vibranceInfo.defaultHandle = enumerateNvidiaDisplayHandle();
            }
            return false;
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
                    int id = getAssociatedNvidiaDisplayHandle(deviceName, deviceName.Length);
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

                            if (sb != null && sb.ToString().Equals(VibranceProxy.NVAPI_GLOBAL_OFFENSIVE_WINDOW_NAME))
                            {
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
                            setDVCLevel(vibranceInfo.defaultHandle, vibranceInfo.userVibranceSettingDefault);
                            isChanged = false;
                        }
                    }
                }
                else
                {
                    if (isChanged || !equalsDVCLevel(vibranceInfo.defaultHandle, vibranceInfo.userVibranceSettingDefault))
                    {
                        setDVCLevel(vibranceInfo.defaultHandle, vibranceInfo.userVibranceSettingDefault);
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
            if (!equalsDVCLevel(vibranceInfo.defaultHandle, vibranceInfo.userVibranceSettingDefault))
                setDVCLevel(vibranceInfo.defaultHandle, vibranceInfo.userVibranceSettingDefault);
        }
    }
}
