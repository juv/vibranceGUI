using gui.app.gpucontroller.amd32;
using gui.app.gpucontroller.amd64;
using System;
using System.Runtime.InteropServices;
using vibrance.GUI.AMD.vendor;
using vibrance.GUI.NVIDIA;

namespace vibrance.GUI.common
{
    public enum GraphicsAdapter
    {
        UNKNOWN = 0,
        NVIDIA = 1,
        AMD = 2,
    }

    public class GraphicsAdapterHelper
    {

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LoadLibrary(string dllToLoad);

        private const string nvidiaDllName = "nvapi.dll";
        private static string amdDllName = Environment.Is64BitOperatingSystem 
            ? gui.app.gpucontroller.amd.adl64.ADLImport.Atiadl_FileName 
            : gui.app.gpucontroller.amd.adl32.ADLImport.Atiadl_FileName;


        public static GraphicsAdapter getAdapter()
        {
            if (isAdapterAvailable(amdDllName))
            {
                AmdAdapter amdAdapter = Environment.Is64BitOperatingSystem ? (AmdAdapter)new AmdAdapter64() : (AmdAdapter)new AmdAdapter32();
                if (amdAdapter.IsAvailable())
                {
                    return GraphicsAdapter.AMD;
                }
            }
            if (isAdapterAvailable(nvidiaDllName))
            {
                return GraphicsAdapter.NVIDIA;
            }
            return GraphicsAdapter.UNKNOWN;
        }

        private static bool isAdapterAvailable(string dllName)
        {
            try
            {
                IntPtr pDll = LoadLibrary(dllName);
                if(pDll != IntPtr.Zero)
                    return true;
                return false;
            }
            catch (Exception)
            {
                return false;
            }    
        }
    }
}
