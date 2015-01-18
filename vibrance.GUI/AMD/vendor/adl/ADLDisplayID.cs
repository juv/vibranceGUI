using System.Runtime.InteropServices;

namespace gui.app.gpucontroller.amd.adl
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLDisplayID
    {
        internal int DisplayLogicalIndex;
        internal int DisplayPhysicalIndex;
        internal int DisplayLogicalAdapterIndex;
        internal int DisplayPhysicalAdapterIndex;
    }
}