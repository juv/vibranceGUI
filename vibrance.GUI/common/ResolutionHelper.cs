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
        private const int EnumCurrentSettings = -1;

        [DllImport("user32.dll")]
        public static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref Devmode devMode);


        [DllImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.I4)]
        private static extern int ChangeDisplaySettings(
            [In, Out]
            ref Devmode lpDevMode,
            [param: MarshalAs(UnmanagedType.U4)]
            uint dwflags);

        [DllImport("user32.dll")]
        private static extern DispChange ChangeDisplaySettingsEx(
            string lpszDeviceName,
            ref Devmode lpDevMode,
            IntPtr hwnd,
            ChangeDisplaySettingsFlags dwflags,
            IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern DispChange ChangeDisplaySettingsEx(
            string lpszDeviceName, 
            IntPtr lpDevMode, 
            IntPtr hwnd, 
            ChangeDisplaySettingsFlags dwflags, 
            IntPtr lParam);


        public static bool GetCurrentResolutionSettings(out Devmode mode, string lpszDeviceName)
        {
            mode = new Devmode();
            mode.dmSize = (ushort)Marshal.SizeOf(mode);
            mode.dmDriverExtra = 0;

            if (EnumDisplaySettings(lpszDeviceName, EnumCurrentSettings, ref mode) == true)
            {
                return true;
            }

            return false;
        }

        public static List<ResolutionModeWrapper> EnumerateSupportedResolutionModes()
        {
            return EnumerateSupportedResolutionModes(null);
        }

        public static List<ResolutionModeWrapper> EnumerateSupportedResolutionModes(string deviceName)
        {
            List<ResolutionModeWrapper> resolutionList = new List<ResolutionModeWrapper>();
            Devmode mode = new Devmode();
            mode.dmSize = (ushort)Marshal.SizeOf(mode);

            int index = 0;
            while (EnumDisplaySettings(deviceName, index++, ref mode) == true)
            {
                resolutionList.Add(new ResolutionModeWrapper(mode));
            }

            return resolutionList;
        }

        public static bool ChangeResolution(ResolutionModeWrapper resolutionMode)
        {
            Devmode mode = new Devmode();
            if (GetCurrentResolutionSettings(out mode, null))
            {
                mode.dmPelsWidth = resolutionMode.DmPelsWidth;
                mode.dmPelsHeight = resolutionMode.DmPelsHeight;
                mode.dmBitsPerPel = resolutionMode.DmBitsPerPel;
                mode.dmDisplayFrequency = resolutionMode.DmDisplayFrequency;
                mode.dmDisplayFixedOutput = resolutionMode.DmDisplayFixedOutput;

                DispChange returnValue = (DispChange)ChangeDisplaySettings(ref mode, 0);
                if (DispChange.DispChangeSuccessful == returnValue)
                {
                    return true;
                }
                else
                {
                    MessageBox.Show("Changing the resolution failed: " + Enum.GetName(typeof(DispChange), returnValue));
                }
            }
            return false;
        }

        public static bool ChangeResolutionEx(ResolutionModeWrapper resolutionMode, string lpszDeviceName)
        {
            Devmode mode = new Devmode();
            if (GetCurrentResolutionSettings(out mode, lpszDeviceName))
            {
                mode.dmPelsWidth = resolutionMode.DmPelsWidth;
                mode.dmPelsHeight = resolutionMode.DmPelsHeight;
                mode.dmBitsPerPel = resolutionMode.DmBitsPerPel;
                mode.dmDisplayFrequency = resolutionMode.DmDisplayFrequency;
                mode.dmDisplayFixedOutput = resolutionMode.DmDisplayFixedOutput;


                DispChange returnValue = (DispChange)ChangeDisplaySettingsEx(lpszDeviceName, ref mode, IntPtr.Zero, (ChangeDisplaySettingsFlags.CdsUpdateregistry | ChangeDisplaySettingsFlags.CdsNoreset), IntPtr.Zero);
                ChangeDisplaySettingsEx(null, IntPtr.Zero, (IntPtr)null, ChangeDisplaySettingsFlags.CdsNone, (IntPtr)null);

                if (DispChange.DispChangeSuccessful == returnValue)
                {
                    return true;
                }
                else
                {
                    MessageBox.Show("Changing the resolution failed: " + Enum.GetName(typeof(DispChange), returnValue));
                }
            }
            return false;
        }
    }

    public enum DispChange : int
    {
        DispChangeSuccessful = 0,
        DispChangeRestart = 1,
        DispChangeFailed = -1,
        DispChangeBadmode = -2,
        DispChangeNotupdated = -3,
        DispChangeBadflags = -4,
        DispChangeBadparam = -5
    };

    public enum Dmdfo : int
    {
        Default = 0,
        Stretch = 1,
        Center = 2
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct Devmode
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

        public Pointl dmPosition;

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
    public struct Pointl
    {
        [MarshalAs(UnmanagedType.I4)]
        public int x;
        [MarshalAs(UnmanagedType.I4)]
        public int y;
    }

    [Flags()]
    public enum ChangeDisplaySettingsFlags : uint
    {
        CdsNone = 0,
        CdsUpdateregistry = 0x00000001,
        CdsTest = 0x00000002,
        CdsFullscreen = 0x00000004,
        CdsGlobal = 0x00000008,
        CdsSetPrimary = 0x00000010,
        CdsVideoparameters = 0x00000020,
        CdsEnableUnsafeModes = 0x00000100,
        CdsDisableUnsafeModes = 0x00000200,
        CdsReset = 0x40000000,
        CdsResetEx = 0x20000000,
        CdsNoreset = 0x10000000
    }

}
