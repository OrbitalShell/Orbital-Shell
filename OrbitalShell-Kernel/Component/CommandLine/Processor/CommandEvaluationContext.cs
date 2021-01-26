using OrbitalShell.Component.CommandLine.Variable;
using OrbitalShell.Console;
using System.IO;
using System;

namespace OrbitalShell.Component.CommandLine.Processor
{
    public class CommandEvaluationContext
    {
        static int _instanceCounter = 1000;
        static object _instanceLock = new object();

        /// <summary>
        /// context id
        /// </summary>
        public int ID = -1;

        /// <summary>
        /// the clp it is attached to (and dedicated to : it is the owner creator of the contexts)
        /// </summary>
        public readonly CommandLineProcessor CommandLineProcessor;

        /// <summary>
        /// context output stream
        /// </summary>
        public readonly ConsoleTextWriterWrapper Out;

        /// <summary>
        /// context error stream
        /// </summary>
        public readonly TextWriterWrapper Err;

        /// <summary>
        /// context input steam
        /// </summary>
        public readonly TextReader In;

        /// <summary>
        /// input data to begin the context with
        /// </summary>
        public readonly object InputData;

        /// <summary>
        /// eventual parent context
        /// </summary>
        public readonly CommandEvaluationContext ParentContext;

        /// <summary>
        /// context variables
        /// </summary>
        public readonly Variables Variables;

        /// <summary>
        /// context shell environment
        /// </summary>
        public ShellEnvironment ShellEnv { get; protected set; }

        /// <summary>
        /// context echo map
        /// </summary>
        public EchoPrimitiveMap EchoMap { get; protected set; }

        /// <summary>
        /// new command evaluation context
        /// </summary>
        /// <param name="commandLineProcessor">the clp it is attached to</param>
        /// <param name="out">output stream</param>
        /// <param name="in">input stream</param>
        /// <param name="err">error stream</param>
        /// <param name="inputData">input data to begin the context with</param>
        /// <param name="parentContext">parent context (optionnal, default null)</param>
        public CommandEvaluationContext(
            CommandLineProcessor commandLineProcessor,
            ConsoleTextWriterWrapper @out,
            TextReader @in,
            TextWriterWrapper err,
            object inputData,
            CommandEvaluationContext parentContext = null)
        {
            lock (_instanceLock)
            {
                ID = _instanceCounter;
                _instanceCounter++;
            }

            CommandLineProcessor = commandLineProcessor;
            Out = @out;
            In = @in;
            Err = err;
            InputData = inputData;

            SetupShellEnvVar();
            Variables = new Variables((VariableNamespace.env, ShellEnv));
            ShellEnv.Initialize(this);

            EchoMap = new EchoPrimitiveMap();
            ParentContext = parentContext;

            @out.ColorSettings = ShellEnv.Colors;
        }

        void SetupShellEnvVar()
        {
            ShellEnv = new ShellEnvironment(VariableNamespace.env + "");
        }

        #region output shortcuts

        public void Errorln(string s) => Out.Error(s, true);

        public void Error(string s, bool lineBreak = false) => Out.Error(s, lineBreak);

        public void Warningln(string s) => Out.Warning(s, true);

        public void Warning(string s, bool lineBreak = false) => Out.Warning(s, lineBreak);

        #endregion
    }
}
