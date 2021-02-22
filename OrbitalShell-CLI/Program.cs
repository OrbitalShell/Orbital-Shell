using OrbitalShell.Component.CommandLine.Reader;
using System;
using static OrbitalShell.DotNetConsole;
using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Console;
using OrbitalShell.Component.Shell;

namespace OrbitalShell
{
    class Program
    {
        static void Main(string[] args)
        {
            Out.Echo(ANSI.RIS);
            Out.ClearScreen();

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
            Environment.Exit(returnCode);
        }
    }
}

