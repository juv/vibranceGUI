namespace vibrance.GUI.common
{
    internal interface IRegistryController
    {
        bool RegisterProgram(string appName, string pathToExe);
        bool UnregisterProgram(string appName);
        bool IsProgramRegistered(string appName);
    }
}