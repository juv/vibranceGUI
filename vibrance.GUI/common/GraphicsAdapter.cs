using System;
using System.Runtime.InteropServices;
using vibrance.GUI.AMD.vendor;
using vibrance.GUI.AMD.vendor.adl32;
using vibrance.GUI.NVIDIA;

namespace vibrance.GUI.common
{
    public enum GraphicsAdapter
    {
        Unknown = 0,
        Nvidia = 1,
        Amd = 2,
    }

    public class GraphicsAdapterHelper
    {

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LoadLibrary(string dllToLoad);

        private const string NvidiaDllName = "nvapi.dll";
        private static readonly string _amdDllName = Environment.Is64BitOperatingSystem 
            ? AdlImport.AtiadlFileName 
            : AMD.vendor.adl64.AdlImport.AtiadlFileName;


        public static GraphicsAdapter GetAdapter()
        {
            if (IsAdapterAvailable(_amdDllName))
            {
                IAmdAdapter amdAdapter = Environment.Is64BitOperatingSystem ? (IAmdAdapter)new AmdAdapter64() :new AmdAdapter32();
                if (amdAdapter.IsAvailable())
                {
                    return GraphicsAdapter.Amd;
                }
            }
            if (IsAdapterAvailable(NvidiaDllName))
            {
                return GraphicsAdapter.Nvidia;
            }
            return GraphicsAdapter.Unknown;
        }

        private static bool IsAdapterAvailable(string dllName)
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
