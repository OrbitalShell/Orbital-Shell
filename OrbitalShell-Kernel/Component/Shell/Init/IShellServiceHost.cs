using System;

using Microsoft.Extensions.Hosting;

namespace OrbitalShell.Component.Shell.Init
{
    public interface IShellServiceHost
    {
        int RunShellServiceHost(string[] args);

        IShellBootstrap GetShellBootstrap(string[] args);
    }
}
