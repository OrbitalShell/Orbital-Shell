using OrbitalShell.Component.CommandLine.Reader;
using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Console;
using OrbitalShell.Component.Shell;
using System;

namespace OrbitalShell
{
    public class ShellStartup : IShellStartup
    {
        IConsole _cons;
        ICommandLineProcessor _clp;
        ICommandLineReader _clr;

        public ShellStartup(
            IServiceProvider serviceProvider,
            IConsole console,
            ICommandLineProcessor commandLineProcessor,
            ICommandLineReader commandLineReader
            )
        {
            _cons = console;
            _clp = commandLineProcessor;
            _clr = commandLineReader;
        }

        public int Startup(
            string[] args)
        {
            _clp.SetArgs(args);

            // prepare console

            _cons.Out.Echo(ANSI.RIS);
            _cons.Out.ClearScreen();

            // invoke a shell initializer associated to the clp

            var shellInitializer = new ShellInitializer(_clp);
            shellInitializer.Run(_clp.CommandEvaluationContext);

            // starts an interactive shell

            return _clr.ReadCommandLine();
        }
    }
}
