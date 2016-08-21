using System;

namespace vibrance.GUI.AMD.vendor.adl64
{
    public class Delegates
    {
        public delegate IntPtr AdlMainMemoryAlloc(int size);
        public delegate int AdlMainControlCreate(AdlMainMemoryAlloc callback, int enumConnectedAdapters);
        public delegate int AdlMainControlDestroy();
        public delegate int AdlAdapterNumberOfAdaptersGet(ref int numAdapters);
        public delegate int AdlAdapterAdapterInfoGet(IntPtr info, int inputSize);
        public delegate int AdlAdapterActiveGet(int adapterIndex, ref int status);
        public delegate int AdlDisplayDisplayInfoGet(int adapterIndex, ref int numDisplays, out IntPtr displayInfoArray, int forceDetect);
        public delegate int AdlDisplayColorSet(int a, int b, int c, int d);
        public delegate int AdlDisplayColorGet(int a, int b, int c, ref int d, ref int e, ref int f, ref int g, ref int h);
    }
}