using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace vibrance.GUI
{

    class SettingsController
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

        private string fileName = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString() + "\\vibranceGUI.ini";

        public bool setVibranceSettings(string ingameLevel, string windowsLevel, string keepActive, string refreshRate)
        {
            if (!prepareFile())
            {
                return false;
            }

            WritePrivateProfileString(szSectionName, "activeValue", ingameLevel, fileName);
            WritePrivateProfileString(szSectionName, "inactiveValue", windowsLevel, fileName);
            WritePrivateProfileString(szSectionName, "keepActive", keepActive, fileName);
            WritePrivateProfileString(szSectionName, "refreshRate", refreshRate, fileName);

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

        public void readVibranceSettings(out int vibranceIngameLevel, out int vibranceWindowsLevel, out bool keepActive, out int refreshRate)
        {
            if (!isFileExisting(fileName))
            {
                vibranceIngameLevel = VibranceProxy.NVAPI_DEFAULT_LEVEL;
                vibranceWindowsLevel = VibranceProxy.NVAPI_DEFAULT_LEVEL;
                refreshRate = VibranceProxy.NVAPI_DEFAULT_REFRESH_RATE;
                keepActive = false;
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

            try
            {
                vibranceWindowsLevel = int.Parse(szValueInactive.ToString());
                vibranceIngameLevel = int.Parse(szValueActive.ToString());
                refreshRate = int.Parse(szValueRefreshRate.ToString());
                keepActive = bool.Parse(szValueKeepActive.ToString());
            }
            catch (Exception)
            {
                vibranceIngameLevel = VibranceProxy.NVAPI_DEFAULT_LEVEL;
                vibranceWindowsLevel = VibranceProxy.NVAPI_DEFAULT_LEVEL;
                refreshRate = VibranceProxy.NVAPI_DEFAULT_REFRESH_RATE;
                keepActive = false;
                return;
            }

            if (vibranceWindowsLevel < VibranceProxy.NVAPI_DEFAULT_LEVEL || vibranceWindowsLevel > VibranceProxy.NVAPI_MAX_LEVEL)
                vibranceWindowsLevel = VibranceProxy.NVAPI_DEFAULT_LEVEL;
            if (vibranceIngameLevel < VibranceProxy.NVAPI_DEFAULT_LEVEL || vibranceIngameLevel > VibranceProxy.NVAPI_MAX_LEVEL)
                vibranceIngameLevel = VibranceProxy.NVAPI_MAX_LEVEL;
            if (refreshRate < VibranceProxy.NVAPI_MIN_REFRESH_RATE)
                refreshRate = VibranceProxy.NVAPI_DEFAULT_REFRESH_RATE;
        }

        private bool isFileExisting(string szFilename)
        {
            return File.Exists(szFilename);
        }
    }
}
