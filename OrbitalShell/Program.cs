using DotNetConsoleAppToolkit.Component.CommandLine.CommandLineReader;
using DotNetConsoleAppToolkit.Component.CommandLine.Processor;
using System;
using static DotNetConsoleAppToolkit.DotNetConsole;

namespace OrbitalShell
{
    class Program
    {
        static void Main(string[] args)
        {
            Out.ClearScreen();
            var commandLineProcessor = new CommandLineProcessor(args);
            var commandLineReader = new CommandLineReader(commandLineProcessor);
            commandLineProcessor.Initialize();
            var returnCode = commandLineReader.ReadCommandLine();
            Environment.Exit(returnCode);
        }
    }
}

