namespace OrbitalShell.Component.Shell
{
    public interface IShellStartup
    {
        int Startup(string[] args);

        ShellInitializer GetShellInitializer(string[] args);
    }
}
