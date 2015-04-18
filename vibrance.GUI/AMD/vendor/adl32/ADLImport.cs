using System;
using System.Runtime.InteropServices;

namespace gui.app.gpucontroller.amd.adl64
{
    public static class ADLImport
    {
        public const string Atiadl_FileName = "atiadlxy.dll";

        public const string Kernel32_FileName = "kernel32.dll";

        [DllImport(Kernel32_FileName)]
        public static extern IntPtr GetModuleHandle(string moduleName);

        [DllImport(Atiadl_FileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ADL_Main_Control_Create(Delegates.ADL_Main_Memory_Alloc callback, int enumConnectedAdapters);

        [DllImport(Atiadl_FileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ADL_Main_Control_Destroy();

        [DllImport(Atiadl_FileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ADL_Main_Control_IsFunctionValid(IntPtr module, string procName);

        [DllImport(Atiadl_FileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ADL_Main_Control_GetProcAddress(IntPtr module, string procName);

        [DllImport(Atiadl_FileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ADL_Adapter_NumberOfAdapters_Get(ref int numAdapters);

        [DllImport(Atiadl_FileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ADL_Adapter_AdapterInfo_Get(IntPtr info, int inputSize);

        [DllImport(Atiadl_FileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ADL_Adapter_Active_Get(int adapterIndex, ref int status);

        [DllImport(Atiadl_FileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ADL_Display_DisplayInfo_Get(int adapterIndex, ref int numDisplays, out IntPtr displayInfoArray, int forceDetect);

        [DllImport(Atiadl_FileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ADL_Display_Color_Set(int a, int b, int c, int d);

        [DllImport(Atiadl_FileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ADL_Display_Color_Get(int a, int b, int c, ref int d, ref int e, ref int f, ref int g, ref int h);

    }
}