using OrbitalShell.Component.CommandLine.Reader;
using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Console;
using OrbitalShell.Component.Shell;
using System;

namespace OrbitalShell
{
    public class ShellStartup : IShellStartup
    {
        readonly IConsole _cons;
        readonly ICommandLineProcessor _clp;
        readonly ICommandLineReader _clr;

        public ShellStartup(
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
            string[] args
            )
        {
            var shellInitializer = GetShellInitializer(args);

            // prepare console

            _cons.Out.Echo(ANSI.RIS);
            _cons.Out.ClearScreen();

            // invoke a shell initializer associated to the clp
            
            shellInitializer.Run();

            // starts an interactive shell

            return _clr.ReadCommandLine();
        }

        public ShellInitializer GetShellInitializer(string[] args)
        {
            _clp.SetArgs(args);
            var shellInitializer = new ShellInitializer(_clp);
            return shellInitializer;
        }

        public ICommandLineReader GetCommandLineReader()
            => _clr;  
    }
}
