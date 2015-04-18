using System;

namespace gui.app.gpucontroller.amd.adl32
{
    public class ADLCheckLibrary
    {
        private IntPtr ADLLibrary = System.IntPtr.Zero;
        private static ADLCheckLibrary ADLCheckLibrary_ = new ADLCheckLibrary();

        private ADLCheckLibrary()
        {
            try
            {
                if (1 == ADLImport.ADL_Main_Control_IsFunctionValid(IntPtr.Zero, "ADL_Main_Control_Create"))
                {
                    ADLLibrary = ADLImport.GetModuleHandle(ADLImport.Atiadl_FileName);
                }
            }
            catch (DllNotFoundException) { }
            catch (EntryPointNotFoundException) { }
            catch (Exception) { }
        }

        ~ADLCheckLibrary()
        {
            if (System.IntPtr.Zero != ADLCheckLibrary_.ADLLibrary)
            {
                ADLImport.ADL_Main_Control_Destroy();
            }
        }

        public static bool IsFunctionValid(string functionName)
        {
            bool result = false;
            if (System.IntPtr.Zero != ADLCheckLibrary_.ADLLibrary)
            {
                if (1 == ADLImport.ADL_Main_Control_IsFunctionValid(ADLCheckLibrary_.ADLLibrary, functionName))
                {
                    result = true;
                }
            }
            return result;
        }

        public static IntPtr GetProcAddress(string functionName)
        {
            IntPtr result = System.IntPtr.Zero;
            if (System.IntPtr.Zero != ADLCheckLibrary_.ADLLibrary)
            {
                result = ADLImport.ADL_Main_Control_GetProcAddress(ADLCheckLibrary_.ADLLibrary, functionName);
            }
            return result;
        }
    }
}