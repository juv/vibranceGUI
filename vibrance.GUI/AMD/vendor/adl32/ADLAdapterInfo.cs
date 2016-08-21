using System.Runtime.InteropServices;

namespace vibrance.GUI.AMD.vendor.adl32
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct AdlAdapterInfo
    {
        int Size;
        internal int AdapterIndex;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)Adl.AdlMaxPath)]
        internal string UDID;
        internal int BusNumber;
        internal int DriverNumber;
        internal int FunctionNumber;
        internal int VendorID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)Adl.AdlMaxPath)]
        internal string AdapterName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)Adl.AdlMaxPath)]
        internal string DisplayName;
        internal int Present;
        internal int Exist;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)Adl.AdlMaxPath)]
        internal string DriverPath;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)Adl.AdlMaxPath)]
        internal string DriverPathExt;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)Adl.AdlMaxPath)]
        internal string PNPString;
        internal int OSDisplayIndex;
    }
}