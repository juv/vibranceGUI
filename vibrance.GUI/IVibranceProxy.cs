namespace vibrance.GUI
{
    public interface IVibranceProxy
    {
        void setShouldRun(bool shouldRun);
        void setVibranceWindowsLevel(int vibranceWindowsLevel);
        void setVibranceIngameLevel(int vibranceIngameLevel);
        void setKeepActive(bool keepActive);
        void setSleepInterval(int interval);
        void handleDVC();
        bool unloadLibraryEx();
        void handleDVCExit();
        void setAffectPrimaryMonitorOnly(bool affectPrimaryMonitorOnly);
    }
}