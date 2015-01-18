using System;
using System.Runtime.InteropServices;

namespace gui.app.utils
{
    public static class NativeMethods
    {
        static NativeMethods()
        {
            SetDllDirectory(CommonUtils.GetVibrance_GUI_AppDataPath());
        }

        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetDllDirectory(string lpPathName);
    }
}