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

            var commandLineProcessor = new CommandLineProcessor(
                args,
                new OrbitalShellCommandLineProcessorSettings()
            );

            var commandLineReader = new CommandLineReader(
                commandLineProcessor);

            var shellInitializer = new ShellInitializer(commandLineProcessor);

            //commandLineProcessor.Initialize();
            shellInitializer.Run(commandLineProcessor.CommandEvaluationContext);

            var returnCode = commandLineReader.ReadCommandLine();
            Environment.Exit(returnCode);
        }
    }
}

