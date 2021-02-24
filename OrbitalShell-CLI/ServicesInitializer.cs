using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Lib.Sys;

namespace OrbitalShell
{
    public class ServicesInitializer
    {
        public IServiceProvider ScopedServiceProvider { get; set; }

        public void InitializeServices(IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices(
                (_, services) => services
                    .AddScoped
                        <ICommandLineProcessorSettings, OrbitalShellCommandLineProcessorSettings>()
                    .AddScoped
                        <ICommandLineProcessor, CommandLineProcessor>()
                    .AddScoped
                        <IServiceProviderScope, ServiceProviderScope>(
                            serviceProvider => 
                                new ServiceProviderScope(ScopedServiceProvider)
                        )
                    );
        }
    }
}
