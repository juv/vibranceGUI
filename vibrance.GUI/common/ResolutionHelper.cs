using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace vibrance.GUI.common
{
    class ResolutionHelper
    {
        private const int ENUM_CURRENT_SETTINGS = -1;

        [DllImport("user32.dll")]
        public static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);


        [DllImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.I4)]
        private static extern int ChangeDisplaySettings(
            [In, Out]
            ref DEVMODE lpDevMode,
            [param: MarshalAs(UnmanagedType.U4)]
            uint dwflags);

        [DllImport("user32.dll")]
        private static extern DISP_CHANGE ChangeDisplaySettingsEx(
            string lpszDeviceName,
            ref DEVMODE lpDevMode,
            IntPtr hwnd,
            ChangeDisplaySettingsFlags dwflags,
            IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern DISP_CHANGE ChangeDisplaySettingsEx(
            string lpszDeviceName, 
            IntPtr lpDevMode, 
            IntPtr hwnd, 
            ChangeDisplaySettingsFlags dwflags, 
            IntPtr lParam);


        public static bool GetCurrentResolutionSettings(out DEVMODE mode, string lpszDeviceName)
        {
            mode = new DEVMODE();
            mode.dmSize = (ushort)Marshal.SizeOf(mode);
            mode.dmDriverExtra = 0;

            if (EnumDisplaySettings(lpszDeviceName, ENUM_CURRENT_SETTINGS, ref mode) == true)
            {
                return true;
            }

            return false;
        }

        public static List<ResolutionModeWrapper> EnumerateSupportedResolutionModes()
        {
            List<ResolutionModeWrapper> resolutionList = new List<ResolutionModeWrapper>();
            DEVMODE mode = new DEVMODE();
            mode.dmSize = (ushort)Marshal.SizeOf(mode);

            int index = 0;
            while (EnumDisplaySettings(null, index++, ref mode) == true)
            {
                resolutionList.Add(new ResolutionModeWrapper(mode));
            }

            return resolutionList;
        }

        public static bool ChangeResolution(ResolutionModeWrapper resolutionMode)
        {
            DEVMODE mode = new DEVMODE();
            if (GetCurrentResolutionSettings(out mode, null))
            {
                mode.dmPelsWidth = resolutionMode.dmPelsWidth;
                mode.dmPelsHeight = resolutionMode.dmPelsHeight;
                mode.dmBitsPerPel = resolutionMode.dmBitsPerPel;
                mode.dmDisplayFrequency = resolutionMode.dmDisplayFrequency;
                mode.dmDisplayFixedOutput = resolutionMode.dmDisplayFixedOutput;

                DISP_CHANGE returnValue = (DISP_CHANGE)ChangeDisplaySettings(ref mode, 0);
                if (DISP_CHANGE.DISP_CHANGE_SUCCESSFUL == returnValue)
                {
                    return true;
                }
                else
                {
                    MessageBox.Show("Changing the resolution failed: " + Enum.GetName(typeof(DISP_CHANGE), returnValue));
                }
            }
            return false;
        }

        public static bool ChangeResolutionEx(ResolutionModeWrapper resolutionMode, string lpszDeviceName)
        {
            DEVMODE mode = new DEVMODE();
            if (GetCurrentResolutionSettings(out mode, lpszDeviceName))
            {
                mode.dmPelsWidth = resolutionMode.dmPelsWidth;
                mode.dmPelsHeight = resolutionMode.dmPelsHeight;
                mode.dmBitsPerPel = resolutionMode.dmBitsPerPel;
                mode.dmDisplayFrequency = resolutionMode.dmDisplayFrequency;
                mode.dmDisplayFixedOutput = resolutionMode.dmDisplayFixedOutput;


                DISP_CHANGE returnValue = (DISP_CHANGE)ChangeDisplaySettingsEx(lpszDeviceName, ref mode, IntPtr.Zero, (ChangeDisplaySettingsFlags.CDS_UPDATEREGISTRY | ChangeDisplaySettingsFlags.CDS_NORESET), IntPtr.Zero);
                ChangeDisplaySettingsEx(null, IntPtr.Zero, (IntPtr)null, ChangeDisplaySettingsFlags.CDS_NONE, (IntPtr)null);

                if (DISP_CHANGE.DISP_CHANGE_SUCCESSFUL == returnValue)
                {
                    return true;
                }
                else
                {
                    MessageBox.Show("Changing the resolution failed: " + Enum.GetName(typeof(DISP_CHANGE), returnValue));
                }
            }
            return false;
        }
    }

    public enum DISP_CHANGE : int
    {
        DISP_CHANGE_SUCCESSFUL = 0,
        DISP_CHANGE_RESTART = 1,
        DISP_CHANGE_FAILED = -1,
        DISP_CHANGE_BADMODE = -2,
        DISP_CHANGE_NOTUPDATED = -3,
        DISP_CHANGE_BADFLAGS = -4,
        DISP_CHANGE_BADPARAM = -5
    };

    public enum DMDFO : int
    {
        DEFAULT = 0,
        STRETCH = 1,
        CENTER = 2
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct DEVMODE
    {
        // You can define the following constant
        // but OUTSIDE the structure because you know
        // that size and layout of the structure
        // is very important
        // CCHDEVICENAME = 32 = 0x50
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmDeviceName;
        // In addition you can define the last character array
        // as following:
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        //public Char[] dmDeviceName;

        // After the 32-bytes array
        [MarshalAs(UnmanagedType.U2)]
        public UInt16 dmSpecVersion;

        [MarshalAs(UnmanagedType.U2)]
        public UInt16 dmDriverVersion;

        [MarshalAs(UnmanagedType.U2)]
        public UInt16 dmSize;

        [MarshalAs(UnmanagedType.U2)]
        public UInt16 dmDriverExtra;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmFields;

        public POINTL dmPosition;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmDisplayOrientation;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmDisplayFixedOutput;

        [MarshalAs(UnmanagedType.I2)]
        public Int16 dmColor;

        [MarshalAs(UnmanagedType.I2)]
        public Int16 dmDuplex;

        [MarshalAs(UnmanagedType.I2)]
        public Int16 dmYResolution;

        [MarshalAs(UnmanagedType.I2)]
        public Int16 dmTTOption;

        [MarshalAs(UnmanagedType.I2)]
        public Int16 dmCollate;

        // CCHDEVICENAME = 32 = 0x50
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmFormName;
        // Also can be defined as
        //[MarshalAs(UnmanagedType.ByValArray,
        //    SizeConst = 32, ArraySubType = UnmanagedType.U1)]
        //public Byte[] dmFormName;

        [MarshalAs(UnmanagedType.U2)]
        public UInt16 dmLogPixels;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmBitsPerPel;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmPelsWidth;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmPelsHeight;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmDisplayFlags;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmDisplayFrequency;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmICMMethod;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmICMIntent;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmMediaType;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmDitherType;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmReserved1;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmReserved2;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmPanningWidth;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmPanningHeight;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINTL
    {
        [MarshalAs(UnmanagedType.I4)]
        public int x;
        [MarshalAs(UnmanagedType.I4)]
        public int y;
    }

    [Flags()]
    public enum ChangeDisplaySettingsFlags : uint
    {
        CDS_NONE = 0,
        CDS_UPDATEREGISTRY = 0x00000001,
        CDS_TEST = 0x00000002,
        CDS_FULLSCREEN = 0x00000004,
        CDS_GLOBAL = 0x00000008,
        CDS_SET_PRIMARY = 0x00000010,
        CDS_VIDEOPARAMETERS = 0x00000020,
        CDS_ENABLE_UNSAFE_MODES = 0x00000100,
        CDS_DISABLE_UNSAFE_MODES = 0x00000200,
        CDS_RESET = 0x40000000,
        CDS_RESET_EX = 0x20000000,
        CDS_NORESET = 0x10000000
    }

}
