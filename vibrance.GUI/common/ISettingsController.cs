using System.Collections.Generic;
using vibrance.GUI.NVIDIA;

namespace vibrance.GUI.common
{
    internal interface ISettingsController
    {
        bool SetVibranceSettings(string windowsLevel, string affectPrimaryMonitorOnly, string neverSwitchResolution, List<ApplicationSetting> applicationSettings);
        bool SetVibranceSetting(string szKeyName, string value);
        void ReadVibranceSettings(GraphicsAdapter graphicsAdapter, out int vibranceWindowsLevel, out bool affectPrimaryMonitorOnly, out bool neverSwitchResolution, out List<ApplicationSetting> applicationSettings);
    }
}