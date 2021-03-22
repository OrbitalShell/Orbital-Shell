using OrbitalShell.Component.CommandLine.Reader;
using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Console;
using OrbitalShell.Component.Shell;
using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace OrbitalShell.Component.Shell.Init
{
    public class ShellServiceHost : IShellServiceHost
    {
        readonly IConsole _cons;
        readonly ICommandLineProcessor _clp;
        readonly ICommandLineReader _clr;

        public static IShellServiceHost GetShellServiceHost(
            string[] args,
            Action<IHostBuilder> initAction = null)
        {
            App.InitializeServices(System.Array.Empty<string>());

            var si = new ShellServicesInitializer();
            si.InitializeServices(App.HostBuilder);

            initAction?.Invoke(App.HostBuilder);

            App.EndInitializeServices();

            var scope = App.Host.Services.CreateScope();
            si.ScopedServiceProvider = scope.ServiceProvider;

            return scope.ServiceProvider.GetRequiredService<IShellServiceHost>();
        }

        public ShellServiceHost(
            IConsole console,
            ICommandLineProcessor commandLineProcessor,
            ICommandLineReader commandLineReader
            )
        {
            _cons = console;
            _clp = commandLineProcessor;
            _clr = commandLineReader;
        }

        /// <summary>
        /// bootstrap shell for CLI context (interactive shell)
        /// </summary>
        /// <param name="args">shell arguments</param>
        /// <returns>shell exit code</returns>
        public int InitializeShellServiceHost(
            string[] args
            )
        {
            var shellBootstrap = GetShellBootstrap(args);

            // prepare console

            _cons.Out.Echo(ANSI.RIS);
            _cons.Out.ClearScreen();

            // invoke a shell initializer associated to the clp
            
            shellBootstrap.Run();

            // starts an interactive shell

            return _clr.ReadCommandLine();
        }

        public ShellBootstrap GetShellBootstrap(string[] args)
        {
            _clp.SetArgs(args);
            var shellBootstrap = new ShellBootstrap(_clp);
            return shellBootstrap;
        }

        public ICommandLineReader GetCommandLineReader()
            => _clr;  
    }
}
