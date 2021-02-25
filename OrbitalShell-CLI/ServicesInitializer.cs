using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using OrbitalShell.Component.CommandLine.Batch;
using OrbitalShell.Component.CommandLine.Parsing;
using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.CommandLine.Reader;
using OrbitalShell.Component.Shell;
using OrbitalShell.Component.Shell.Module;
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
                    .AddScoped
                        <ICommandsAlias,CommandsAlias>()
                    .AddScoped
                        <IModuleSet,ModuleSet>()
                    .AddScoped
                        <ICommandBatchProcessor,CommandBatchProcessor>()
                    .AddScoped
                        <ISyntaxAnalyser,SyntaxAnalyser>()
                    .AddScoped
                        <IModuleManager,ModuleManager>()
                    .AddScoped
                        <IModuleCommandManager,ModuleCommandManager>()
                    .AddScoped
                        <IHookManager,HookManager>()
                    .AddScoped
                        <IExternalParserExtension, CommandLineProcessorExternalParserExtension>()
                    .AddScoped
                        <ICommandLineReader,CommandLineReader>(
                            serviceProvider =>
                            {
                                var clr = new CommandLineReader();
                                clr.Initialize(
                                    null,
                                    serviceProvider.GetRequiredService<ICommandLineProcessor>());
                                return clr;
                            })
                    );
        }
    }
}
