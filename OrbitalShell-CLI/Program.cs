using DotNetConsoleAppToolkit.Component.CommandLine.CommandLineReader;
using proc = DotNetConsoleAppToolkit.Component.CommandLine.Processor;
using System;
using static DotNetConsoleAppToolkit.DotNetConsole;

using OrbitalShell.Component.CommandLine.Processor;

namespace OrbitalShell
{
    class Program
    {
        static void Main(string[] args)
        {
            //Out.ClearScreen();
            var commandLineProcessor = 
                new proc.CommandLineProcessor(args,new CommandLineProcessorSettings());
            var commandLineReader = new CommandLineReader(commandLineProcessor);
            commandLineProcessor.Initialize();
            var returnCode = commandLineReader.ReadCommandLine();
            Environment.Exit(returnCode);
        }
    }
}

