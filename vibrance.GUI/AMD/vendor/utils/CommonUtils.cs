using System;
using System.IO;
using System.Reflection;

namespace vibrance.GUI.AMD.vendor.utils
{
    public static class CommonUtils
    {
        public static string GetVibrance_GUI_AppDataPath()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "vibranceGUI");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        public static string LoadUnmanagedLibraryFromResource(Assembly assembly,
            string libraryResourceName,
            string libraryName)
        {
            string tempDllPath;
            using (Stream s = assembly.GetManifestResourceStream(libraryResourceName))
            {
                byte[] data = new BinaryReader(s).ReadBytes((int)s.Length);

                tempDllPath = Path.Combine(GetVibrance_GUI_AppDataPath(), libraryName);
                File.WriteAllBytes(tempDllPath, data);

            }

            NativeMethods.LoadLibrary(libraryName);
            return tempDllPath;
        }
    }
}