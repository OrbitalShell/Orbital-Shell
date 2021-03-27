using OrbitalShell.Component.Shell.Variable;
using OrbitalShell.Component.Console;
using System.IO;
using System.Runtime.CompilerServices;

namespace OrbitalShell.Component.CommandLine.Processor
{
    public class CommandEvaluationContext
    {
        public CommandEvaluationContextSettings Settings { get; protected set; } = new CommandEvaluationContextSettings();

        static int _instanceCounter = 1000;
        readonly static object _instanceLock = new object();

        public readonly Logger Logger;

        public override string ToString() => $"[com eval ctx : id={ID} Out={Out} ]";        

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
        public ConsoleTextWriterWrapper Out { get; protected set; }

        /// <summary>
        /// context error stream
        /// </summary>
        public TextWriterWrapper Err { get; protected set; }

        /// <summary>
        /// context input steam
        /// </summary>
        public TextReader In { get; protected set; }

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

            Logger = new Logger(this);
            CommandLineProcessor = commandLineProcessor;
            SetStreams(@out, @in, err);
            InputData = inputData;
           
            SetupShellEnvVar();
            Variables = new Variables((VariableNamespace.env, ShellEnv));
            ShellEnv.Initialize(this);

            EchoMap = new EchoPrimitiveMap();
            ParentContext = parentContext;

            @out.ColorSettings = ShellEnv.Colors;
        }

        public void SetStreams(
            ConsoleTextWriterWrapper @out,
            TextReader @in,
            TextWriterWrapper err
        )
        {
            Out = @out;
            In = @in;
            Err = err;
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

        /// <summary>
        /// system.diagnostics.debug
        /// </summary>
        public void Debug(
            string s,
            bool lineBreak = false,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = -1)
        => Out.Debug(s, lineBreak, callerFilePath, callerMemberName, callerLineNumber);

        #endregion
    }
}
