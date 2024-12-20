using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using vibrance.GUI.AMD;
using vibrance.GUI.NVIDIA;

namespace vibrance.GUI.common
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

        const string SzSectionName = "Settings";
        const string SzKeyNameInactive = "inactiveValue";
        const string SzKeyNameRefreshRate = "refreshRate";
        const string SzKeyNameAffectPrimaryMonitorOnly = "affectPrimaryMonitorOnly";
        const string SzKeyNameNeverSwitchResolution = "neverSwitchResolution";
        const string SzKeyNameNeverChangeColorSettings = "neverChangeColorSettings";
        const string SzKeyNameBrightnessWindowsLevel = "brightnessWindowsLevel";
        const string SzKeyNameContrastWindowsLevel = "contrastWindowsLevel";
        const string SzKeyNameGammaWindowsLevel = "gammaWindowsLevel";
        

        private string _fileName = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString() + "\\vibranceGUI\\vibranceGUI.ini";
        private string _fileNameApplicationSettings = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString() + "\\vibranceGUI\\applicationData.xml";


        public bool SetVibranceSettings(string windowsLevel, string affectPrimaryMonitorOnly, string neverSwitchResolution, string neverChangeColorSettings, List<ApplicationSetting> applicationSettings, 
            string brightnessWindowsLevel, string contrastWindowsLevel, string gammaWindowsLevel)
        {
            if (!PrepareFile())
            {
                return false;
            }

            WritePrivateProfileString(SzSectionName, SzKeyNameInactive, windowsLevel, _fileName);
            WritePrivateProfileString(SzSectionName, SzKeyNameAffectPrimaryMonitorOnly, affectPrimaryMonitorOnly, _fileName);
            WritePrivateProfileString(SzSectionName, SzKeyNameNeverSwitchResolution, neverSwitchResolution, _fileName);
            WritePrivateProfileString(SzSectionName, SzKeyNameNeverChangeColorSettings, neverChangeColorSettings, _fileName);
            WritePrivateProfileString(SzSectionName, SzKeyNameBrightnessWindowsLevel, brightnessWindowsLevel, _fileName);
            WritePrivateProfileString(SzSectionName, SzKeyNameContrastWindowsLevel, contrastWindowsLevel, _fileName);
            WritePrivateProfileString(SzSectionName, SzKeyNameGammaWindowsLevel, gammaWindowsLevel, _fileName);

            try
            {
                var writer = System.Xml.XmlWriter.Create(_fileNameApplicationSettings);
                if (writer.WriteState != WriteState.Start)
                    return false;
                XmlSerializer serializer = new XmlSerializer(typeof(List<ApplicationSetting>));
                serializer.Serialize(writer, applicationSettings);
                writer.Flush();
                writer.Close();
            }
            catch (Exception)
            {
                return false;
            }

            return (Marshal.GetLastWin32Error() == 0);
        }

        public bool SetVibranceSetting(string szKeyName, string value)
        {
            if (!PrepareFile())
            {
                return false;
            }

            WritePrivateProfileString(SzSectionName, szKeyName, value.ToString(), _fileName);

            return (Marshal.GetLastWin32Error() == 0);
        }

        private bool PrepareFile()
        {
            if (!IsFileExisting(_fileName))
            {
                StreamWriter sw = new StreamWriter(_fileName);
                sw.Close();
                if (!IsFileExisting(_fileName))
                {
                    return false;
                }
            }

            return true;
        }

        public void ReadVibranceSettings(GraphicsAdapter graphicsAdapter, out int vibranceWindowsLevel, out bool affectPrimaryMonitorOnly, out bool neverSwitchResolution, 
            out bool neverChangeColorSettings, out List<ApplicationSetting> applicationSettings, out int brightnessWindowsLevel, out int contrastWindowsLevel, out int gammaWindowsLevel)
        {
            int defaultLevel = 0; 
            int maxLevel = 0;
            if (graphicsAdapter == GraphicsAdapter.Nvidia)
            {
                defaultLevel = NvidiaDynamicVibranceProxy.NvapiDefaultLevel;
                maxLevel = NvidiaDynamicVibranceProxy.NvapiMaxLevel;
            }
            if (graphicsAdapter == GraphicsAdapter.Amd)
            {
                defaultLevel = AmdDynamicVibranceProxy.AmdDefaultLevel;
                maxLevel = AmdDynamicVibranceProxy.AmdMaxLevel;
            }

            if (!IsFileExisting(_fileName) || !IsFileExisting(_fileNameApplicationSettings))
            {
                vibranceWindowsLevel = defaultLevel;
                affectPrimaryMonitorOnly = true;
                applicationSettings = new List<ApplicationSetting>();
                neverSwitchResolution = true;
                neverChangeColorSettings = true;
                brightnessWindowsLevel = 50;
                contrastWindowsLevel = 50;
                gammaWindowsLevel = 100;
                return;
            }

            string szDefault = "";

            StringBuilder szValueInactive = new StringBuilder(1024);
            GetPrivateProfileString(SzSectionName,
                SzKeyNameInactive,
                szDefault,
                szValueInactive,
                Convert.ToUInt32(szValueInactive.Capacity),
                _fileName);

            StringBuilder szValueRefreshRate = new StringBuilder(1024);
            GetPrivateProfileString(SzSectionName,
                SzKeyNameRefreshRate,
                szDefault,
                szValueRefreshRate,
                Convert.ToUInt32(szValueRefreshRate.Capacity),
                _fileName);

            StringBuilder szValueAffectPrimaryMonitorOnly = new StringBuilder(1024);
            GetPrivateProfileString(SzSectionName,
                SzKeyNameAffectPrimaryMonitorOnly,
                "true",
                szValueAffectPrimaryMonitorOnly,
                Convert.ToUInt32(szValueAffectPrimaryMonitorOnly.Capacity),
                _fileName);

            StringBuilder szValueNeverSwitchResolution = new StringBuilder(1024);
            GetPrivateProfileString(SzSectionName,
                SzKeyNameNeverSwitchResolution,
                "true",
                szValueNeverSwitchResolution,
                Convert.ToUInt32(szValueNeverSwitchResolution.Capacity),
                _fileName);

            StringBuilder szValueNeverChangeColorSettings = new StringBuilder(1024);
            GetPrivateProfileString(SzSectionName,
                SzKeyNameNeverChangeColorSettings,
                "true",
                szValueNeverChangeColorSettings,
                Convert.ToUInt32(szValueNeverChangeColorSettings.Capacity),
                _fileName);

            StringBuilder szValueBrightnessWindowsLevel = new StringBuilder(1024);
            GetPrivateProfileString(SzSectionName,
                SzKeyNameBrightnessWindowsLevel,
                "50",
                szValueBrightnessWindowsLevel,
                Convert.ToUInt32(szValueBrightnessWindowsLevel.Capacity),
                _fileName);

            StringBuilder szValueContrastWindowsLevel = new StringBuilder(1024);
            GetPrivateProfileString(SzSectionName,
                SzKeyNameContrastWindowsLevel,
                "50",
                szValueContrastWindowsLevel,
                Convert.ToUInt32(szValueContrastWindowsLevel.Capacity),
                _fileName);

            StringBuilder szValueGammaWindowsLevel = new StringBuilder(1024);
            GetPrivateProfileString(SzSectionName,
                SzKeyNameGammaWindowsLevel,
                "100",
                szValueGammaWindowsLevel,
                Convert.ToUInt32(szValueGammaWindowsLevel.Capacity),
                _fileName);

            try
            {
                vibranceWindowsLevel = int.Parse(szValueInactive.ToString());
                affectPrimaryMonitorOnly = bool.Parse(szValueAffectPrimaryMonitorOnly.ToString());
                neverSwitchResolution = bool.Parse(szValueNeverSwitchResolution.ToString());
                neverChangeColorSettings = bool.Parse(szValueNeverChangeColorSettings.ToString());
                brightnessWindowsLevel = int.Parse(szValueBrightnessWindowsLevel.ToString());
                contrastWindowsLevel = int.Parse(szValueContrastWindowsLevel.ToString());
                gammaWindowsLevel = int.Parse(szValueGammaWindowsLevel.ToString());
            }
            catch (Exception)
            {
                vibranceWindowsLevel = defaultLevel;
                affectPrimaryMonitorOnly = false;
                applicationSettings = new List<ApplicationSetting>();
                neverSwitchResolution = true;
                neverChangeColorSettings = true;
                brightnessWindowsLevel = 50;
                contrastWindowsLevel = 50;
                gammaWindowsLevel = 100;
                return;
            }

            if (vibranceWindowsLevel < defaultLevel || vibranceWindowsLevel > maxLevel)
                vibranceWindowsLevel = defaultLevel;

            try
            {
                var reader = System.Xml.XmlReader.Create(_fileNameApplicationSettings);
                XmlSerializer serializer = new XmlSerializer(typeof(List<ApplicationSetting>));
                applicationSettings = (List<ApplicationSetting>)serializer.Deserialize(reader);
                reader.Close();
            }
            catch (Exception)
            {
                applicationSettings = new List<ApplicationSetting>();
            }
        }

        private bool IsFileExisting(string szFilename)
        {
            return File.Exists(szFilename);
        }
    }
}
