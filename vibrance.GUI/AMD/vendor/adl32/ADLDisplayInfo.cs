using System.Runtime.InteropServices;

namespace gui.app.gpucontroller.amd.adl64
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLDisplayInfo
    {
        internal ADLDisplayID DisplayID;
        internal int DisplayControllerIndex;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADL.ADL_MAX_PATH)]
        internal string DisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADL.ADL_MAX_PATH)]
        internal string DisplayManufacturerName;
        internal int DisplayType;
        internal int DisplayOutputType;
        internal int DisplayConnector;
        internal int DisplayInfoMask;
        internal int DisplayInfoValue;
    }
}