namespace vibrance.GUI.common
{
    internal interface IRegistryController
    {
        bool registerProgram(string appName, string pathToExe);
        bool unregisterProgram(string appName);
        bool isProgramRegistered(string appName);
    }
}