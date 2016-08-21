using System.Runtime.InteropServices;

namespace vibrance.GUI.AMD.vendor.adl32
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct AdlAdapterInfoArray
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)Adl.AdlMaxAdapters)]
        internal AdlAdapterInfo[] ADLAdapterInfo;
    }
}