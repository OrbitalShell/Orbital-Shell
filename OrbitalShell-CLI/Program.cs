
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
                ShellServiceHost.GetShellServiceHost(
                    args,
                    (hostBuilder) => 
                        hostBuilder.ConfigureServices(
                            (_,services) => 
                                services.AddScoped
                                    <ICommandLineProcessorSettings, OrbitalShellCommandLineProcessorSettings>()
                                    )
                        )                    
                .InitializeShellServiceHost(args);

            App.Host.Run();

            return returnCode;
        }
    }
}

