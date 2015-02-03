using System;
using System.Runtime.InteropServices;

namespace gui.app.gpucontroller.amd.adl
{
    public static class ADLImport
    {
        public const string Atiadlxy_FileName = "atiadlxy.dll";
        public const string Kernel32_FileName = "kernel32.dll";

        [DllImport(Kernel32_FileName)]
        public static extern IntPtr GetModuleHandle(string moduleName);

        [DllImport(Atiadlxy_FileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ADL_Main_Control_Create(Delegates.ADL_Main_Memory_Alloc callback, int enumConnectedAdapters);

        [DllImport(Atiadlxy_FileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ADL_Main_Control_Destroy();

        [DllImport(Atiadlxy_FileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ADL_Main_Control_IsFunctionValid(IntPtr module, string procName);

        [DllImport(Atiadlxy_FileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ADL_Main_Control_GetProcAddress(IntPtr module, string procName);

        [DllImport(Atiadlxy_FileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ADL_Adapter_NumberOfAdapters_Get(ref int numAdapters);

        [DllImport(Atiadlxy_FileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ADL_Adapter_AdapterInfo_Get(IntPtr info, int inputSize);

        [DllImport(Atiadlxy_FileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ADL_Adapter_Active_Get(int adapterIndex, ref int status);

        [DllImport(Atiadlxy_FileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ADL_Display_DisplayInfo_Get(int adapterIndex, ref int numDisplays, out IntPtr displayInfoArray, int forceDetect);

        [DllImport(Atiadlxy_FileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ADL_Display_Color_Set(int a, int b, int c, int d);

        [DllImport(Atiadlxy_FileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ADL_Display_Color_Get(int a, int b, int c, ref int d, ref int e, ref int f, ref int g, ref int h);

    }
}