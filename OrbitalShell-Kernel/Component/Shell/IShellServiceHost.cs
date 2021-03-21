namespace OrbitalShell.Component.Shell
{
    public interface IShellServiceHost
    {
        int InitializeShellServiceHost(string[] args);

        ShellBootstrap GetShellBootstrap(string[] args);
    }
}
