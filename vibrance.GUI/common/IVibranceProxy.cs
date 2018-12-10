using System.Collections.Generic;
using vibrance.GUI.NVIDIA;

namespace vibrance.GUI.common
{
    public interface IVibranceProxy
    {
        void SetApplicationSettings(List<ApplicationSetting> refApplicationSettings);
        void SetShouldRun(bool shouldRun);
        void SetVibranceWindowsLevel(int vibranceWindowsLevel);
        void SetVibranceIngameLevel(int vibranceIngameLevel);
        bool UnloadLibraryEx();
        void HandleDvcExit();
        void SetAffectPrimaryMonitorOnly(bool affectPrimaryMonitorOnly);
        VibranceInfo GetVibranceInfo();
        GraphicsAdapter GraphicsAdapter { get; }
        void SetNeverSwitchResolution(bool neverSwitchResolution);
    }
}