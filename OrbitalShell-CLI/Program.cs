
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Shell.Init;

namespace OrbitalShell
{
    public static class Program
    {
        static int Main(string[] args)
        {
            var returnCode =
                GetShellServiceHost(args)
                    .RunShellServiceHost(args);

            App.Host.Run();

            return returnCode;
        }

        public static IShellServiceHost GetShellServiceHost(string[] args)
            => ShellServiceHost.GetShellServiceHost(
                    args,
                    (hostBuilder) =>
                        hostBuilder.ConfigureServices(
                            (_, services) =>
                                services.AddScoped
                                    <ICommandLineProcessorSettings, OrbitalShellCommandLineProcessorSettings>()
                                    )
                        );
    }
}

