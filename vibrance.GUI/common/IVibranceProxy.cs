using System.Collections.Generic;
using vibrance.GUI.NVIDIA;

namespace vibrance.GUI.common
{
    public interface IVibranceProxy
    {
        void setApplicationSettings(ref List<ApplicationSetting> refApplicationSettings);
        void setShouldRun(bool shouldRun);
        void setVibranceWindowsLevel(int vibranceWindowsLevel);
        void setVibranceIngameLevel(int vibranceIngameLevel);
        void setSleepInterval(int interval);
        void handleDVC();
        bool unloadLibraryEx();
        void handleDVCExit();
        void setAffectPrimaryMonitorOnly(bool affectPrimaryMonitorOnly);
        VIBRANCE_INFO getVibranceInfo();
    }
}