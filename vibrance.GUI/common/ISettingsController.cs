using System.Collections.Generic;
using vibrance.GUI.NVIDIA;

namespace vibrance.GUI.common
{
    internal interface ISettingsController
    {
        bool setVibranceSettings(string windowsLevel, string keepActive, string affectPrimaryMonitorOnly, List<NvidiaApplicationSetting> applicationSettings);
        bool setVibranceSetting(string szKeyName, string value);
        void readVibranceSettings(GraphicsAdapter graphicsAdapter, out int vibranceWindowsLevel, out bool keepActive, out bool affectPrimaryMonitorOnly, out List<NvidiaApplicationSetting> applicationSettings);
    }
}