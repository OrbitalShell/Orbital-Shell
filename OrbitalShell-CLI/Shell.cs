using OrbitalShell.Component.CommandLine.Reader;
using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Console;
using OrbitalShell.Component.Shell;
using Microsoft.Extensions.DependencyInjection;

namespace OrbitalShell
{
    public class Shell
    {
        public static int Startup(string[] args)
        {
            var cons = App.ServiceProvider.GetRequiredService<IDotNetConsole>();

            // 2. build a clp

            var clp = App.ServiceProvider.GetRequiredService<ICommandLineProcessor>();

            clp.SetArgs(args);

            // 3. build a clr associated to the clp

            var clr = new CommandLineReader(clp);

            // 4 . prepare console

            cons.Out.Echo(ANSI.RIS);
            cons.Out.ClearScreen();

            // 5. invoke a shell initializer associated to the clp

            var shellInitializer = new ShellInitializer(clp);
            shellInitializer.Run(clp.CommandEvaluationContext);

            // 6. starts an interactive shell

            return clr.ReadCommandLine();
        }
    }
}
