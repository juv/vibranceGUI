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
        public int defaultHandle;
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
