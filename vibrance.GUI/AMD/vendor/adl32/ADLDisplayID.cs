using System.Runtime.InteropServices;

namespace vibrance.GUI.AMD.vendor.adl32
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct AdlDisplayId
    {
        internal int DisplayLogicalIndex;
        internal int DisplayPhysicalIndex;
        internal int DisplayLogicalAdapterIndex;
        internal int DisplayPhysicalAdapterIndex;
    }
}