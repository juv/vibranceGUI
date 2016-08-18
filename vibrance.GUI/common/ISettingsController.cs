using System.Collections.Generic;
using vibrance.GUI.NVIDIA;

namespace vibrance.GUI.common
{
    internal interface ISettingsController
    {
        bool setVibranceSettings(string windowsLevel, string affectPrimaryMonitorOnly, List<ApplicationSetting> applicationSettings);
        bool setVibranceSetting(string szKeyName, string value);
        void readVibranceSettings(GraphicsAdapter graphicsAdapter, out int vibranceWindowsLevel, out bool affectPrimaryMonitorOnly, out List<ApplicationSetting> applicationSettings);
    }
}