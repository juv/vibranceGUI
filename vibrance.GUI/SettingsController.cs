using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using vibrance.GUI.NVIDIA;

namespace vibrance.GUI
{

    class SettingsController : ISettingsController
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern uint GetPrivateProfileString(
           string lpAppName,
           string lpKeyName,
           string lpDefault,
           StringBuilder lpReturnedString,
           uint nSize,
           string lpFileName);


        [DllImport("kernel32.dll", EntryPoint = "WritePrivateProfileString")]
        private static extern bool WritePrivateProfileString(string lpAppName,
          string lpKeyName, string lpString, string lpFileName);

        const string szSectionName = "Settings";
        const string szKeyNameInactive = "inactiveValue";
        const string szKeyNameActive = "activeValue";
        const string szKeyNameKeepActive = "keepActive";
        const string szKeyNameRefreshRate = "refreshRate";
        const string szKeyNameAffectPrimaryMonitorOnly = "affectPrimaryMonitorOnly";

        private string fileName = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString() + "\\vibranceGUI\\vibranceGUI.ini";

        public bool setVibranceSettings(string ingameLevel, string windowsLevel, string keepActive, string refreshRate, string affectPrimaryMonitorOnly)
        {
            if (!prepareFile())
            {
                return false;
            }

            WritePrivateProfileString(szSectionName, szKeyNameActive, ingameLevel, fileName);
            WritePrivateProfileString(szSectionName, szKeyNameInactive, windowsLevel, fileName);
            WritePrivateProfileString(szSectionName, szKeyNameKeepActive, keepActive, fileName);
            WritePrivateProfileString(szSectionName, szKeyNameRefreshRate, refreshRate, fileName);
            WritePrivateProfileString(szSectionName, szKeyNameAffectPrimaryMonitorOnly, affectPrimaryMonitorOnly, fileName);

            return (Marshal.GetLastWin32Error() == 0);
        }

        public bool setVibranceSetting(string szKeyName, string value)
        {
            if (!prepareFile())
            {
                return false;
            }

            WritePrivateProfileString(szSectionName, szKeyName, value.ToString(), fileName);

            return (Marshal.GetLastWin32Error() == 0);
        }

        private bool prepareFile()
        {
            if (!isFileExisting(fileName))
            {
                StreamWriter sw = new StreamWriter(fileName);
                sw.Close();
                if (!isFileExisting(fileName))
                {
                    return false;
                }
            }

            return true;
        }

        public void readVibranceSettings(GraphicsAdapter graphicsAdapter, out int vibranceIngameLevel, out int vibranceWindowsLevel, out bool keepActive, out int refreshRate, out bool affectPrimaryMonitorOnly)
        {
            int defaultLevel = 0; 
            int maxLevel = 0;
            int defaultRefreshRate = 0;
            int minRefreshRate = 0;
            if (graphicsAdapter == GraphicsAdapter.NVIDIA)
            {
                defaultLevel = NvidiaVibranceProxy.NVAPI_DEFAULT_LEVEL;
                maxLevel = NvidiaVibranceProxy.NVAPI_MAX_LEVEL;
                defaultRefreshRate = NvidiaVibranceProxy.NVAPI_DEFAULT_REFRESH_RATE;
                minRefreshRate = NvidiaVibranceProxy.NVAPI_MIN_REFRESH_RATE;
            }


            if (!isFileExisting(fileName))
            {
                vibranceIngameLevel = defaultLevel;
                vibranceWindowsLevel = defaultLevel;
                refreshRate = defaultRefreshRate;
                keepActive = false;
                affectPrimaryMonitorOnly = false;
                return;
            }

            string szDefault = "";

            StringBuilder szValueActive = new StringBuilder(1024);
            
            GetPrivateProfileString(szSectionName,
                szKeyNameActive,
                szDefault,
                szValueActive,
                Convert.ToUInt32(szValueActive.Capacity),
                fileName);

            StringBuilder szValueInactive = new StringBuilder(1024);
            GetPrivateProfileString(szSectionName,
                szKeyNameInactive,
                szDefault,
                szValueInactive,
                Convert.ToUInt32(szValueInactive.Capacity),
                fileName);

            StringBuilder szValueRefreshRate = new StringBuilder(1024);
            GetPrivateProfileString(szSectionName,
                szKeyNameRefreshRate,
                szDefault,
                szValueRefreshRate,
                Convert.ToUInt32(szValueRefreshRate.Capacity),
                fileName);


            StringBuilder szValueKeepActive = new StringBuilder(1024);
            GetPrivateProfileString(szSectionName,
                szKeyNameKeepActive,
                szDefault,
                szValueKeepActive,
                Convert.ToUInt32(szValueKeepActive.Capacity),
                fileName);

            StringBuilder szValueAffectPrimaryMonitorOnly = new StringBuilder(1024);
            GetPrivateProfileString(szSectionName,
                szKeyNameAffectPrimaryMonitorOnly,
                szDefault,
                szValueAffectPrimaryMonitorOnly,
                Convert.ToUInt32(szValueAffectPrimaryMonitorOnly.Capacity),
                fileName);

            try
            {
                vibranceWindowsLevel = int.Parse(szValueInactive.ToString());
                vibranceIngameLevel = int.Parse(szValueActive.ToString());
                refreshRate = int.Parse(szValueRefreshRate.ToString());
                keepActive = bool.Parse(szValueKeepActive.ToString());
                affectPrimaryMonitorOnly = bool.Parse(szValueAffectPrimaryMonitorOnly.ToString());
            }
            catch (Exception)
            {
                vibranceIngameLevel = defaultLevel;
                vibranceWindowsLevel = defaultLevel;
                refreshRate = defaultRefreshRate;
                keepActive = false;
                affectPrimaryMonitorOnly = false;
                return;
            }

            if (vibranceWindowsLevel < defaultLevel || vibranceWindowsLevel > maxLevel)
                vibranceWindowsLevel = defaultLevel;
            if (vibranceIngameLevel < defaultLevel || vibranceIngameLevel > maxLevel)
                vibranceIngameLevel = maxLevel;
            if (refreshRate < minRefreshRate)
                refreshRate = defaultRefreshRate;
        }

        private bool isFileExisting(string szFilename)
        {
            return File.Exists(szFilename);
        }
    }
}
