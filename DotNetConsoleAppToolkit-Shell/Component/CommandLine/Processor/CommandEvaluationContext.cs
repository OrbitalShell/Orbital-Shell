using DotNetConsoleAppToolkit.Component.CommandLine.Variable;
using DotNetConsoleAppToolkit.Console;
using System.IO;

namespace DotNetConsoleAppToolkit.Component.CommandLine.Processor
{
    public class CommandEvaluationContext
    {
        public readonly CommandLineProcessor CommandLineProcessor;
        public readonly ConsoleTextWriterWrapper Out;
        public readonly TextWriterWrapper Err;
        public readonly TextReader In;
        public readonly object InputData;
        public readonly Variables Variables;
        public readonly CommandEvaluationContext ParentContext;
        public ShellEnvironment ShellEnv { get; protected set; }

        public CommandEvaluationContext(            
            CommandLineProcessor commandLineProcessor, 
            ConsoleTextWriterWrapper @out, 
            TextReader @in, 
            TextWriterWrapper err, 
            object inputData,
            CommandEvaluationContext parentContext = null)
        {
            CommandLineProcessor = commandLineProcessor;
            Out = @out;
            In = @in;
            Err = err;
            InputData = inputData;
            Variables = new Variables();            
            ParentContext = parentContext;
            SetupShellEnvVar();
        }

        void SetupShellEnvVar()
        {
            var envn = 
                CommandLineProcessor
                .Settings
                .ShellEnvironmentVariableName;
            ShellEnv = new ShellEnvironment(envn);
            ShellEnv.Initialize(this);
        }

        public void Errorln(string s) => Error(s, true);

        public void Error(string s, bool lineBreak = false)
        {
            lock (Out.Lock)
            {
                Out.RedirecToErr = true;
                Out.Echo($"{ColorSettings.Error}{s}{ColorSettings.Default}", lineBreak);
                Out.RedirecToErr = false;
            }
        }
    }
}
