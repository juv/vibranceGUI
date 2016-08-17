using System;
using System.Runtime.InteropServices;
using vibrance.GUI.AMD.vendor;
using vibrance.GUI.AMD.vendor.adl32;
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
            ? ADLImport.Atiadl_FileName 
            : AMD.vendor.adl64.ADLImport.Atiadl_FileName;


        public static GraphicsAdapter getAdapter()
        {
            if (isAdapterAvailable(amdDllName))
            {
                IAmdAdapter amdAdapter = Environment.Is64BitOperatingSystem ? (IAmdAdapter)new AmdAdapter64() :new AmdAdapter32();
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
                return LoadLibrary(dllName) != IntPtr.Zero;
            }
            catch (Exception)
            {
                return false;
            }    
        }
    }
}
