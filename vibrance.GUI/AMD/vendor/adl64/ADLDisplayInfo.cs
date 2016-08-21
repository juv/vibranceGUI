using System.Runtime.InteropServices;

namespace vibrance.GUI.AMD.vendor.adl64
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct AdlDisplayInfo
    {
        internal AdlDisplayId DisplayID;
        internal int DisplayControllerIndex;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)Adl.AdlMaxPath)]
        internal string DisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)Adl.AdlMaxPath)]
        internal string DisplayManufacturerName;
        internal int DisplayType;
        internal int DisplayOutputType;
        internal int DisplayConnector;
        internal int DisplayInfoMask;
        internal int DisplayInfoValue;
    }
}