using OrbitalShell.Component.CommandLine.Reader;
using System;
using static OrbitalShell.DotNetConsole;
using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Console;

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
                new OrbitalShellCommandLineProcessorSettings());
            var commandLineReader = new CommandLineReader(
                commandLineProcessor);
            commandLineProcessor.Initialize();
            var returnCode = commandLineReader.ReadCommandLine();
            Environment.Exit(returnCode);
        }
    }
}

