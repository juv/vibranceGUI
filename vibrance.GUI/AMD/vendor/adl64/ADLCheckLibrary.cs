using System;

namespace vibrance.GUI.AMD.vendor.adl64
{
    public class AdlCheckLibrary
    {
        private IntPtr _adlLibrary = System.IntPtr.Zero;
        private static AdlCheckLibrary _adlCheckLibrary = new AdlCheckLibrary();

        private AdlCheckLibrary()
        {
            try
            {
                if (1 == AdlImport.ADL_Main_Control_IsFunctionValid(IntPtr.Zero, "ADL_Main_Control_Create"))
                {
                    _adlLibrary = AdlImport.GetModuleHandle(AdlImport.AtiadlFileName);
                }
            }
            catch (DllNotFoundException) { }
            catch (EntryPointNotFoundException) { }
            catch (Exception) { }
        }

        ~AdlCheckLibrary()
        {
            if (System.IntPtr.Zero != _adlCheckLibrary._adlLibrary)
            {
                AdlImport.ADL_Main_Control_Destroy();
            }
        }

        public static bool IsFunctionValid(string functionName)
        {
            bool result = false;
            if (System.IntPtr.Zero != _adlCheckLibrary._adlLibrary)
            {
                if (1 == AdlImport.ADL_Main_Control_IsFunctionValid(_adlCheckLibrary._adlLibrary, functionName))
                {
                    result = true;
                }
            }
            return result;
        }

        public static IntPtr GetProcAddress(string functionName)
        {
            IntPtr result = System.IntPtr.Zero;
            if (System.IntPtr.Zero != _adlCheckLibrary._adlLibrary)
            {
                result = AdlImport.ADL_Main_Control_GetProcAddress(_adlCheckLibrary._adlLibrary, functionName);
            }
            return result;
        }
    }
}