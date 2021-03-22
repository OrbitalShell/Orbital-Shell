namespace OrbitalShell.Component.Shell.Init
{
    public interface IShellServiceHost
    {
        int InitializeShellServiceHost(string[] args);

        ShellBootstrap GetShellBootstrap(string[] args);
    }
}
