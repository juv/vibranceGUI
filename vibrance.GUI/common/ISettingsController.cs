using System.Collections.Generic;
using vibrance.GUI.NVIDIA;

namespace vibrance.GUI.common
{
    internal interface ISettingsController
    {
        bool SetVibranceSettings(string windowsLevel, string affectPrimaryMonitorOnly, string neverSwitchResolution, string neverChangeColorSettings, List<ApplicationSetting> applicationSettings, 
            string brightnessWindowsLevel, string contrastWindowsLevel, string gammaWindowsLevel);
        bool SetVibranceSetting(string szKeyName, string value);
        void ReadVibranceSettings(GraphicsAdapter graphicsAdapter, out int vibranceWindowsLevel, out bool affectPrimaryMonitorOnly, out bool neverSwitchResolution,
            out bool neverChangeColorSettings, out List<ApplicationSetting> applicationSettings, out int brightnessWindowsLevel, out int contrastWindowsLevel, out int gammaWindowsLevel);
    }
}