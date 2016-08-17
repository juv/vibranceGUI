using System;

namespace vibrance.GUI.AMD.vendor.adl64
{
    public class Delegates
    {
        public delegate IntPtr ADL_Main_Memory_Alloc(int size);
        public delegate int ADL_Main_Control_Create(ADL_Main_Memory_Alloc callback, int enumConnectedAdapters);
        public delegate int ADL_Main_Control_Destroy();
        public delegate int ADL_Adapter_NumberOfAdapters_Get(ref int numAdapters);
        public delegate int ADL_Adapter_AdapterInfo_Get(IntPtr info, int inputSize);
        public delegate int ADL_Adapter_Active_Get(int adapterIndex, ref int status);
        public delegate int ADL_Display_DisplayInfo_Get(int adapterIndex, ref int numDisplays, out IntPtr displayInfoArray, int forceDetect);
        public delegate int ADL_Display_Color_Set(int a, int b, int c, int d);
        public delegate int ADL_Display_Color_Get(int a, int b, int c, ref int d, ref int e, ref int f, ref int g, ref int h);
    }
}