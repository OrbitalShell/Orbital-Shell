using OrbitalShell.Component.CommandLine.Reader;
using System;
using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Console;
using OrbitalShell.Component.Shell;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace OrbitalShell
{
    class Program
    {
        static Task Main(string[] args)
        {
            App.Bootstrap();
            using IHost host = App.Host;
            var cons = host.Services.GetRequiredService<IDotNetConsole>();

            cons.Out.Echo(ANSI.RIS);
            cons.Out.ClearScreen();

            // 1. build a clp

            var commandLineProcessor = new CommandLineProcessor(
                args,
                new OrbitalShellCommandLineProcessorSettings()
            );

            // 2. build a clr associated to the clp

            var commandLineReader = new CommandLineReader(
                commandLineProcessor);

            // 3. invoke a shell initializer to bootsrap an intractive shell env within the clp

            var shellInitializer = new ShellInitializer(commandLineProcessor);
            shellInitializer.Run(commandLineProcessor.CommandEvaluationContext);

            var returnCode = commandLineReader.ReadCommandLine();
            
            //Environment.Exit(returnCode);

            return host.RunAsync();
        }
    }
}

