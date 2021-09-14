using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using OrbitalShell.Component.CommandLine.Batch;
using OrbitalShell.Component.CommandLine.Parsing.Command;
using OrbitalShell.Component.CommandLine.Parsing.Parser;
using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.CommandLine.Reader;
using OrbitalShell.Component.Shell.Hook;
using OrbitalShell.Component.Shell.Module;
using OrbitalShell.Lib.Sys;

namespace OrbitalShell.Component.Shell.Init
{
    public class ShellServicesInitializer
    {
        public IServiceProvider ScopedServiceProvider { get; set; }

        public IServiceScope ServiceScope { get; set; }

        public IHostBuilder InitializeServices(IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices(
                (_, services) => services
                    .AddScoped
                        <IShellArgsOptionBuilder, ShellArgsOptionBuilder>()
                    .AddScoped
                        <IShellBootstrap, ShellBootstrap>()
                    .AddScoped
                        <ICommandLineProcessor, CommandLineProcessor>()
                    .AddScoped
                        <IServiceProviderScope, ServiceProviderScope>(
                            serviceProvider =>
                                new ServiceProviderScope(ScopedServiceProvider)
                            )
                    .AddScoped
                        <ICommandsAlias, CommandsAlias>()
                    .AddScoped
                        <IModuleSet, ModuleSet>()
                    .AddScoped
                        <ICommandBatchProcessor, CommandBatchProcessor>()
                    .AddScoped
                        <ICommandSyntaxAnalyzer, CommandSyntaxAnalyser>()
                    .AddScoped
                        <IModuleManager, ModuleManager>()
                    .AddScoped
                        <IModuleCommandManager, ModuleCommandManager>()
                    .AddScoped
                        <IHookManager, HookManager>()
                    .AddScoped
                        <IExternalParserExtension, CommandLineProcessorExternalParserExtension>()
                    .AddScoped
                        <ICommandLineReader, CommandLineReader>(
                            serviceProvider =>
                            {
                                var clr = new CommandLineReader();
                                clr.Initialize(
                                    null,
                                    serviceProvider.GetRequiredService<ICommandLineProcessor>());
                                return clr;
                            })
                    .AddScoped
                        <IShellServiceHost, ShellServiceHost>()
                    );
            return hostBuilder;
        }
    }
}
