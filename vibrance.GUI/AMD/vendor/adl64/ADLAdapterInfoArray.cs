using System.Runtime.InteropServices;

namespace gui.app.gpucontroller.amd.adl32
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLAdapterInfoArray
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)ADL.ADL_MAX_ADAPTERS)]
        internal ADLAdapterInfo[] ADLAdapterInfo;
    }
}