using System;
using System.Runtime.InteropServices;

namespace vibrance.GUI.AMD.vendor.adl32
{
    public static class AdlImport
    {
        public const string AtiadlFileName = "atiadlxx.dll";

        public const string Kernel32FileName = "kernel32.dll";

        [DllImport(Kernel32FileName)]
        public static extern IntPtr GetModuleHandle(string moduleName);

        [DllImport(AtiadlFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ADL_Main_Control_Create(Delegates.AdlMainMemoryAlloc callback, int enumConnectedAdapters);

        [DllImport(AtiadlFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ADL_Main_Control_Destroy();

        [DllImport(AtiadlFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ADL_Main_Control_IsFunctionValid(IntPtr module, string procName);

        [DllImport(AtiadlFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ADL_Main_Control_GetProcAddress(IntPtr module, string procName);

        [DllImport(AtiadlFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ADL_Adapter_NumberOfAdapters_Get(ref int numAdapters);

        [DllImport(AtiadlFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ADL_Adapter_AdapterInfo_Get(IntPtr info, int inputSize);

        [DllImport(AtiadlFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ADL_Adapter_Active_Get(int adapterIndex, ref int status);

        [DllImport(AtiadlFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ADL_Display_DisplayInfo_Get(int adapterIndex, ref int numDisplays, out IntPtr displayInfoArray, int forceDetect);

        [DllImport(AtiadlFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ADL_Display_Color_Set(int a, int b, int c, int d);

        [DllImport(AtiadlFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ADL_Display_Color_Get(int a, int b, int c, ref int d, ref int e, ref int f, ref int g, ref int h);

    }
}