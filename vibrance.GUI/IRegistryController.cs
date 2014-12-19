namespace vibrance.GUI
{
    internal interface IRegistryController
    {
        bool registerProgram(string appName, string pathToExe);
        bool unregisterProgram(string appName);
        bool isProgramRegistered(string appName);
    }
}