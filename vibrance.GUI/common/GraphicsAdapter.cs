using System;
using System.IO;
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
        Ambiguous = 3
    }

    public class GraphicsAdapterHelper
    {

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LoadLibrary(string dllToLoad);

        private const string _nvidiaDllName = "nvapi.dll";
        private static readonly string _amdDllName = Environment.Is64BitOperatingSystem 
            ? AMD.vendor.adl64.AdlImport.AtiadlFileName
            : AMD.vendor.adl32.AdlImport.AtiadlFileName;


        public static GraphicsAdapter GetAdapter()
        {
            string windowsFolder = Environment.GetFolderPath(Environment.SpecialFolder.SystemX86);
            if (File.Exists(Path.Combine(windowsFolder, _amdDllName)) && 
                File.Exists(Path.Combine(windowsFolder, _nvidiaDllName)))
            {
                return GraphicsAdapter.Ambiguous;
            }
            if (IsAdapterAvailable(_amdDllName))
            {
                IAmdAdapter amdAdapter = Environment.Is64BitOperatingSystem ? (IAmdAdapter)new AmdAdapter64() :new AmdAdapter32();
                if (amdAdapter.IsAvailable())
                {
                    return GraphicsAdapter.Amd;
                }
            }
            if (IsAdapterAvailable(_nvidiaDllName))
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
