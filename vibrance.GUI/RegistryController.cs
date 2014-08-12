using Microsoft.Win32;
using System;

namespace vibrance.GUI
{
    class RegistryController
    {
        private const string runKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
        private const string vcKey = "SOFTWARE\\Microsoft\\DevDiv\\VC\\Servicing\\11.0\\RuntimeMinimum";

        private RegistryKey startupKey;

        public RegistryController()
        {

        }

        public bool registerProgram(string appName, string pathToExe)
        {
            try
            {
                if (!isProgramRegistered(appName))
                {
                    startupKey = Registry.CurrentUser.OpenSubKey(runKey);
                    if (startupKey.GetValue(appName) == null)
                    {
                        startupKey.Close();
                        startupKey = Registry.CurrentUser.OpenSubKey(runKey, true);
                        startupKey.SetValue(appName, pathToExe);
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                startupKey.Close();
            }
            return true;
        }

        public bool unregisterProgram(string appName)
        {
            try
            {
                startupKey = Registry.CurrentUser.OpenSubKey(runKey, true);
                startupKey.DeleteValue(appName, true);
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                startupKey.Close();
            }
            return true;
        }

        public bool isProgramRegistered(string appName)
        {
            try
            {
                startupKey = Registry.CurrentUser.OpenSubKey(runKey, true);
                if (startupKey.GetValue(appName) != null)
                    return true;
            }
            catch (Exception)
            {
                return false;
            }
            return false;
        }
    }
}
