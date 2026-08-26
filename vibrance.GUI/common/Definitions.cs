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
        // Tracks whether ResolutionHelper.ChangeResolutionEx last reported the game's resolution as
        // applied (Applied or AppliedUnverified) for the current foreground game, so
        // VibranceGUI.RebuildWindowsResolutionSettings knows not to overwrite the captured Windows
        // mode with a live read while one of the proxies' own changes is in effect.
        public bool isResolutionChangeApplied;
    }
}
