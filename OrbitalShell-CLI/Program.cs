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
        static int Main(string[] args)
        {
            // 1. initialize services

            App.InitializeServices();
            App.EndInitializeServices();
            var cons = App.Host.Services.GetRequiredService<IDotNetConsole>();

            // 2. build a clp

            var commandLineProcessor = new CommandLineProcessor(
                args,
                cons,
                new OrbitalShellCommandLineProcessorSettings()
            );

            // 3. build a clr associated to the clp

            var commandLineReader = new CommandLineReader(
                commandLineProcessor);

            // 4 . prepare console
            
            cons.Out.Echo(ANSI.RIS);
            cons.Out.ClearScreen();

            // 5. invoke a shell initializer to bootsrap an intractive shell env within the clp

            var shellInitializer = new ShellInitializer(commandLineProcessor);
            shellInitializer.Run(commandLineProcessor.CommandEvaluationContext);

            var returnCode = commandLineReader.ReadCommandLine();

            // 6. end

            App.Host.RunAsync().Wait();
            return returnCode;
        }
    }
}

