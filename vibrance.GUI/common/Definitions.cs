using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace vibrance.GUI.common
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VibranceInfo
    {
        public bool isInitialized;
        public int activeOutput;
        // False until SetVibranceWindowsLevel has actually been called - true from construction
        // through the window between the proxy subscribing OnWinEventHook and VibranceGUI.cs's
        // backgroundWorker_DoWork reaching SetVibranceWindowsLevel (it waits on
        // "while (!this.IsHandleCreated) Thread.Sleep(500);" first). Both vendors' restore paths
        // treat a foreground event landing in that window as a no-op instead of writing
        // userVibranceSettingDefault's still-zero struct default.
        public bool isWindowsLevelKnown;
        public int userVibranceSettingDefault;
        public int userVibranceSettingActive;
        public String szGpuName;
        public bool shouldRun;
        public int sleepInterval;
        public List<int> displayHandles;
        public bool affectPrimaryMonitorOnly;
        public bool neverChangeResolution;
    }
}
