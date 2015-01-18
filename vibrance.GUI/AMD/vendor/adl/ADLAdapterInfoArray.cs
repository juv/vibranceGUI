using System.Runtime.InteropServices;

namespace gui.app.gpucontroller.amd.adl
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLAdapterInfoArray
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)ADL.ADL_MAX_ADAPTERS)]
        internal ADLAdapterInfo[] ADLAdapterInfo;
    }
}