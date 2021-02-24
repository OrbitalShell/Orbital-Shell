using OrbitalShell.Component.CommandLine.Batch;
using OrbitalShell.Component.CommandLine.CommandModel;
using OrbitalShell.Component.Shell.Data;
using OrbitalShell.Component.Shell;
using OrbitalShell.Component.CommandLine.Parsing;
using OrbitalShell.Component.CommandLine.Pipeline;
using OrbitalShell.Component.Shell.Variable;
using OrbitalShell.Component.Shell.Module;
using lib = OrbitalShell.Lib;
using OrbitalShell.Lib;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using static OrbitalShell.Component.CommandLine.Parsing.CommandLineParser;
using cmdlr = OrbitalShell.Component.CommandLine.Reader;
using static OrbitalShell.Component.EchoDirective.Shortcuts;
using OrbitalShell.Lib.FileSystem;
using System.Text;
using OrbitalShell.Lib.Process;
using OrbitalShell.Component.Console;
using Microsoft.Extensions.DependencyInjection;
using OrbitalShell.Lib.Sys;

namespace OrbitalShell.Component.CommandLine.Processor
{
    public class CommandLineProcessor : ICommandLineProcessor
    {
        #region attributes

        public ICommandLineProcessorSettings Settings
        {
            get { return _settings; }
            protected set { _settings = value; }
        }

        /// <summary>
        /// warning: for the moment may be null (for example, during shell init phasis)
        /// </summary>
        public CancellationTokenSource CancellationTokenSource { get; set; }

        /// <summary>
        /// preferred way to check if cancellation is requested
        /// </summary>
#pragma warning disable CS8073 // Le résultat de l'expression est toujours le même, car une valeur de ce type n'est jamais égale à 'null'
        public bool IsCancellationRequested => CancellationTokenSource != null && CancellationTokenSource.Token != null && CancellationTokenSource.Token.IsCancellationRequested;
#pragma warning restore CS8073 // Le résultat de l'expression est toujours le même, car une valeur de ce type n'est jamais égale à 'null'

        /// <summary>
        /// shell args
        /// </summary>
        public string[] Args => (string[])_args?.Clone();
                
        public bool IsInitialized { get; set; } = false;

        public ISyntaxAnalyser SyntaxAnalyzer { get; protected set; }

        public CommandsHistory CmdsHistory { get; set; }

        public ICommandsAlias CommandsAlias { get; protected set; }

        public cmdlr.CommandLineReader CommandLineReader { get; set; }

        public CommandEvaluationContext CommandEvaluationContext { get; protected set; }

        public ICommandBatchProcessor CommandBatchProcessor { get; protected set; }

        public IExternalParserExtension ExternalParserExtension { get; protected set; }

        public IModuleManager ModuleManager { get; protected set; }

        public IDotNetConsole Console { get; protected set; }

        string[] _args;

        ICommandLineProcessorSettings _settings;
        
        static int _InstanceId = 0;

        #endregion

        #region cli methods

        public const string OPT_ENV = "--env";
        public const string OPT_NAME_VALUE_SEPARATOR = ":";

        public void SetArgs(
            string[] args,
            CommandEvaluationContext context,
            List<string> appliedSettings)
        {
            _args = (string[])args?.Clone();

            // parse and apply any --env:{VarName}={VarValue} argument
            foreach (var arg in args)
            {
                if (arg.StartsWith(OPT_ENV + OPT_NAME_VALUE_SEPARATOR))
                {
                    try
                    {
                        var t = arg.Split(':');
                        var t2 = t[1].Split('=');
                        if (t.Length == 2 && t[0] == OPT_ENV && t2.Length == 2)
                        {
                            SetVariable(context, t2[0], t2[1]);
                            appliedSettings.Add(arg);
                        }
                        else
                            Error($"shell arg set error: syntax error: {arg}", true);
                    }
                    catch (Exception ex)
                    {
                        Error($"shell arg set error: {arg} (error is: {ex.Message})", true);
                    }
                }
            }
        }

        /// <summary>
        /// set a typed variable from a string value<br/>
        /// don't set the value if conversion has failed
        /// </summary>
        /// <param name="name">name including namespace</param>
        /// <param name="value">value that must be converted to var type an assigned to the var</param>
        void SetVariable(CommandEvaluationContext context, string name, string value)
        {
            var tn = VariableSyntax.SplitPath(name);
            var t = new ArraySegment<string>(tn);
            if (context.ShellEnv.Get(t, out var o) && o is DataValue val)
            {
                if (ValueTextParser.ToTypedValue(value, val.ValueType, null, out var v, out _))
                    val.SetValue(v);
            }
            else
                Error($"variable not found: {Variables.Nsp(VariableNamespace.env, context.ShellEnv.Name, name)}", true);
        }

        #endregion

        #region command engine operations

        public CommandLineProcessor(
            IServiceProviderScope scope,
            IDotNetConsole console,
            ICommandBatchProcessor cbp,
            ICommandsAlias cal,
            ISyntaxAnalyser sa,
            IModuleManager modManager,
            IExternalParserExtension parserExt,
            ICommandLineProcessorSettings settings = null
            )
        {
            _InstanceId++;
#if DBG_DI_INSTANCE
            System.Console.Out.WriteLine($"new CLP #{_InstanceId}");
#endif
            Console = console;
            ExternalParserExtension = parserExt;
            parserExt.CommandLineProcessor = this;
            SyntaxAnalyzer = sa;
            ModuleManager = modManager;
            _settings = settings ?? scope.ServiceProvider.GetRequiredService<ICommandLineProcessorSettings>();
            CommandBatchProcessor = cbp;
            CommandsAlias = cal;            
        }

        public void SetArgs(string[] args) => _args = args;

        /// <summary>
        /// clp init
        /// </summary>
        public void Init(
            string[] args,
            ICommandLineProcessorSettings settings,
            CommandEvaluationContext context = null
            )
        {
            _args = args;

            context ??= new CommandEvaluationContext(
                this,
                Console.Out,
                Console.In,
                Console.Err,
                null
            );
            CommandEvaluationContext = context;
        }

        /// <summary>
        /// clp post init
        /// </summary>
        public virtual void PostInit()
        {
        }

        public void AssertCommandLineProcessorHasACommandLineReader()
        {
            if (CommandLineReader == null) throw new Exception("a command line reader is required by the command line processor to perform this action");
        }

        public void PrintInfo(CommandEvaluationContext context)
        {
            context.Out.Echoln($"{CommandEvaluationContext.ShellEnv.Colors.Label}{Uon} {Settings.AppLongName} ({Settings.AppName}) version {Assembly.GetExecutingAssembly().GetName().Version}" + ("".PadRight(18, ' ')) + Tdoff);
            context.Out.Echoln($" {Settings.AppEditor}");
            context.Out.Echoln($" {RuntimeInformation.OSDescription} {RuntimeInformation.OSArchitecture} - {RuntimeInformation.FrameworkDescription} - {lib.RuntimeEnvironment.OSType}");

            if (context.ShellEnv.GetValue<bool>(ShellEnvironmentVar.settings_console_banner_isEnabled))
            {
                try
                {
                    // todo: use a simple banner command !
                    var banner =
                        File.ReadAllLines(
                            context.ShellEnv.GetValue<string>(ShellEnvironmentVar.settings_console_banner_path)
                        );
                    int c = context.ShellEnv.GetValue<int>(ShellEnvironmentVar.settings_console_banner_startColorIndex);
                    int n = 1;
                    var stp = context.ShellEnv.GetValue<int>(ShellEnvironmentVar.settings_console_banner_colorIndexStep);
                    foreach (var line in banner)
                    {
                        context.Out.SetForeground(c);
                        context.Out.Echoln(line);
                        c += stp;
                        n++;
                        if (n == 6)
                        {
                            //c -= 5;
                            c = context.ShellEnv.GetValue<int>(ShellEnvironmentVar.settings_console_banner_startColorIndex2);
                            stp *= -1;
                        }
                    }
                    //context.Out.Echoln();
                }
                catch (Exception ex)
                {
                    Error("banner error: " + ex.Message, true);
                }
            }
        }

        #region console trace operations

        public void Error(string message = null, bool log = false, bool lineBreak = true, string prefix = "")
        {
            var logMessage = CommandEvaluationContext.ShellEnv.Colors.Error + prefix + (message == null ? "" : $"{message}");
            Console.Out.Echo(logMessage, lineBreak);
            if (log) Console.LogError(logMessage);
        }

        #endregion

        #endregion

        #region commands operations

        /// <summary>
        /// parse and evaluate a command line<br/>
        /// 1. parse command line<br/>
        /// error or:<br/>
        /// 2. execute command<br/>
        ///     A. internal command (modules) or alias<br/>
        ///     B. underlying shell command (found in scan paths)<br/>
        ///      file: <br/>
        ///         C. file (batch)<br/>
        ///         D. non executable file<br/>
        ///     not a file:<br/>
        ///         E. unknown command<br/>
        /// </summary>
        /// <param name="context">command evaluation context</param>
        /// <param name="commandMethodInfo">method info of the invoked command</param>
        /// <param name="args">args are added to the command name to build the command line</param>
        /// <param name="outputX">begin x location of the command line expression in the console if applyable</param>
        /// <param name="postAnalysisPreExecOutput">text to be outputed before any analysis output</param>
        /// <returns>data returned by the analysis and the evaluation of an expression (analysis error or commmand returns or command error)</returns>
        public ExpressionEvaluationResult Eval(
            CommandEvaluationContext context,
            MethodInfo commandMethodInfo,
            string args,
            int outputX = 0,
            string postAnalysisPreExecOutput = null)    // TODO: an Eval options object could be nice
        {
            var comSpec = ModuleManager.ModuleCommandManager.GetCommandSpecification(commandMethodInfo);
            if (comSpec == null)
                throw new Exception($"command method info not found: dt={commandMethodInfo.DeclaringType.FullName} method name={commandMethodInfo.Name}");
            var expr = comSpec.Name + (!string.IsNullOrWhiteSpace(args) ? (" " + args) : "");
            return Eval(context, expr, outputX, postAnalysisPreExecOutput);
        }

        /// <summary>
        /// parse and evaluate a command line<br/>
        /// 1. parse command line<br/>
        /// error or:<br/>
        /// 2. execute command<br/>
        ///     A. internal command (modules) or alias<br/>
        ///     B. underlying shell command (found in scan paths)<br/>
        ///      file: <br/>
        ///         C. file (batch)<br/>
        ///         D. non executable file<br/>
        ///     not a file:<br/>
        ///         E. unknown command<br/>
        /// </summary>
        /// <param name="context">command evaluation context</param>
        /// <param name="expr">expression to be evaluated</param>
        /// <param name="outputX">begin x location of the command line expression in the console if applyable</param>
        /// <param name="postAnalysisPreExecOutput">text to be outputed before any analysis output</param>
        /// <returns>data returned by the analysis and the evaluation of an expression (analysis error or commmand returns or command error)</returns>
        public ExpressionEvaluationResult Eval(
            CommandEvaluationContext context,
            string expr,
            int outputX = 0,
            string postAnalysisPreExecOutput = null)        // TODO: an eval options object would be nice
        {
            try
            {
                var pipelineParseResults = ParseCommandLine(context, SyntaxAnalyzer, expr, ExternalParserExtension);
                bool allValid = true;
                var evalParses = new List<ExpressionEvaluationResult>();

                // check pipeline syntax analysis
                foreach (var pipelineParseResult in pipelineParseResults)
                {
                    allValid &= pipelineParseResult.ParseResult.ParseResultType == ParseResultType.Valid;
                    var evalParse = AnalysisPipelineParseResult(
                        context,
                        pipelineParseResult,
                        expr,
                        outputX,
                        pipelineParseResult.ParseResult
                    );

                    evalParses.Add(evalParse);
                }

                // eventually output the post analysis pre exec content
                if (!string.IsNullOrEmpty(postAnalysisPreExecOutput)) context.Out.Echo(postAnalysisPreExecOutput);

                if (!allValid)
                {
                    // 💥syntax error in pipeline - break exec
                    var err = evalParses.FirstOrDefault();
                    context.ShellEnv.UpdateVarLastCommandReturn(expr, null, err == null ? ReturnCode.OK : GetReturnCode(err), err?.SyntaxError);
                    return err;
                }

                // run pipeline
                var evalRes = PipelineProcessor.RunPipeline(context, pipelineParseResults.FirstOrDefault());

                // update shell env
                context.ShellEnv.UpdateVarLastCommandReturn(expr, evalRes.Result, GetReturnCode(evalRes), evalRes.EvalErrorText, evalRes.EvalError);
                return evalRes;
            }
            catch (Exception pipelineException)
            {
                // code pipeline parse or execution error
                // update shell env
                context.ShellEnv.UpdateVarLastCommandReturn(expr, ReturnCode.Error, ReturnCode.Unknown, pipelineException.Message, pipelineException);
                context.Errorln(pipelineException.Message);
                return new ExpressionEvaluationResult(expr, null, ParseResultType.NotIdentified, null, (int)ReturnCode.Error, pipelineException, pipelineException.Message);
            }
        }

        static ReturnCode GetReturnCode(ExpressionEvaluationResult expr)
        {
            var r = ReturnCode.Error;
            try
            {
                r = (ReturnCode)expr.EvalResultCode;
            }
            catch (Exception) { }
            return r;
        }

        /// <summary>
        /// search a file having a name that would be located in shell scan path and having a shell scan path ext<br/>
        /// if no path is provided, the os will search for the filename in os PathExt
        /// </summary>
        /// <param name="context">command evaluation context</param>
        /// <param name="cmdName">command name</param>
        /// <param name="filePath">matching file path</param>
        /// <returns>the first matching file according to search paths order</returns>
        public bool ExistsInPath(
            CommandEvaluationContext context,
            string cmdName,
            out string filePath)
        {
            var paths = context.ShellEnv.GetValue<List<DirectoryPath>>(ShellEnvironmentVar.path).Clone();
            paths.Insert(0, new DirectoryPath(Environment.CurrentDirectory));
            var pathExts = context.ShellEnv.GetValue<List<string>>(ShellEnvironmentVar.pathExt);
            var searchedPaths = new List<string>();
            int i = 0;
            foreach (var path in paths)
            {
                if (!searchedPaths.Contains(path.FullName))
                {
                    //context.Out.Echoln(path);
                    searchedPaths.Add(path.FullName);

                    foreach (var pathExt in pathExts)
                    {
                        var px = string.IsNullOrWhiteSpace(pathExt) ? pathExt : ((pathExt.StartsWith('.')) ? pathExt : "." + pathExt);
                        var filename = Path.Combine(path.FullName, cmdName + px);
                        //context.Out.Echoln(filename);
                        if (File.Exists(filename))  // accorded to system case sensitive file name setting
                        {
                            //context.Out.Echoln($"(b=red,f=yellow)FOUND: {filename}(rdc)");
                            filePath = filename;
                            return true;
                        }

                    }
                }
                i++;
            }
            filePath = null;
            return false;
        }

        public static bool FindInPath(
                    CommandEvaluationContext context,
                    string cmdName,
                    out List<FilePath> filePath,
                    bool filterExtOnPathExt = false,
                    StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase)
        {
            var paths = context.ShellEnv.GetValue<List<DirectoryPath>>(ShellEnvironmentVar.path).Clone();
            paths.Insert(0, new DirectoryPath(Environment.CurrentDirectory));
            var pathExts = context.ShellEnv.GetValue<List<string>>(ShellEnvironmentVar.pathExt);
            var searchedPaths = new List<string>();
            int i = 0;
            filePath = new List<FilePath>();
            foreach (var path in paths)
            {
                if (!searchedPaths.Contains(path.FullName, stringComparison))
                {
                    searchedPaths.Add(path.FullName);

                    if (filterExtOnPathExt)
                    {
                        foreach (var pathExt in pathExts)
                        {
                            var px = string.IsNullOrWhiteSpace(pathExt) ? pathExt : ((pathExt.StartsWith('.')) ? pathExt : "." + pathExt);
                            var filename = Path.Combine(path.FullName, cmdName + px);
                            if (File.Exists(filename))  // accorded to system case sensitive file name setting
                                filePath.Add(new FilePath(filename));
                        }
                    }
                    else
                    {
                        if (path.CheckExists(context))
                        {
                            var matchingFiles = path.DirectoryInfo.GetFiles().Where(x => Path.GetFileNameWithoutExtension(x.FullName).Equals(cmdName, stringComparison)).Select(x => new FilePath(x.FullName));
                            filePath.AddRange(matchingFiles);
                        }
                    }
                }
                i++;
            }
            return filePath.Count > 0;
        }

        public const int MaxWaitTime = 2000;    // 2 sec

        /// <summary>
        /// shell exec short syntax. output not provided
        /// </summary>
        /// <param name="context"></param>
        /// <param name="com"></param>
        /// <param name="args"></param>
        /// <param name="returnCommandResult"></param>
        /// <returns></returns>
        public bool ShellExec(
           CommandEvaluationContext context,
           string com,
           string args,
           out CommandVoidResult returnCommandResult)
        {
            var returnCode = ShellExec(
                context, com, args, out var output,
                true
                );
            var success = (returnCode == (int)ReturnCode.OK);

            returnCommandResult = success ? null : new CommandVoidResult(returnCode, output);

            return success;
        }

        /// <summary>
        /// exec a file with os shell exec or orbsh shell exec
        /// </summary>
        /// <param name="context">command evaluation context</param>
        /// <param name="comPath">command filePath</param>
        /// <param name="args">command line arguments string</param>
        /// <param name="output">shell exec result if any</param>
        /// <param name="waitForExit">true if wait for exec process exits</param>
        /// <param name="isStreamsEchoEnabled">if true, exec process output stream is echoized to context out (dump command output)</param>
        /// <param name="isOutputCaptureEnabled">if true capture the exec process output and give the result in parameter 'output'</param>
        /// <param name="mergeErrorStreamIntoOutput">if true merge exec process err stream content to the process output content (if process out capture is enabled)</param>
        /// <returns>exec process return code</returns>
        public int ShellExec(
            CommandEvaluationContext context,
            string comPath,
            string args,
            string workingDirectory = null,
            bool waitForExit = true,
            bool isStreamsEchoEnabled = true,
            bool isOutputCaptureEnabled = true,
            bool mergeErrorStreamIntoOutput = true
            )
        {
            return ShellExec(
                context,
                comPath,
                args,
                workingDirectory,
                out _,
                waitForExit,
                isStreamsEchoEnabled,
                isOutputCaptureEnabled,
                mergeErrorStreamIntoOutput
                );
        }

        /// <summary>
        /// exec a file with os shell exec or orbsh shell exec [delivered]
        /// </summary>
        /// <param name="context">command evaluation context</param>
        /// <param name="comPath">command filePath</param>
        /// <param name="args">command line arguments string</param>
        /// <param name="output">shell exec result if any</param>
        /// <param name="waitForExit">true if wait for exec process exits</param>
        /// <param name="isStreamsEchoEnabled">if true, exec process output stream is echoized to context out (dump command output)</param>
        /// <param name="isOutputCaptureEnabled">if true capture the exec process output and give the result in parameter 'output'</param>
        /// <param name="mergeErrorStreamIntoOutput">if true merge exec process err stream content to the process output content (if process out capture is enabled)</param>
        /// <returns>exec process return code</returns>
        public int ShellExec(
            CommandEvaluationContext context,
            string comPath,
            string args,
            out string output,
            bool waitForExit = true,
            bool isStreamsEchoEnabled = true,
            bool isOutputCaptureEnabled = true,
            bool mergeErrorStreamIntoOutput = true
            ) => ShellExec(
                    context,
                    comPath,
                    args,
                    null,
                    out output,
                    waitForExit,
                    isStreamsEchoEnabled,
                    isOutputCaptureEnabled,
                    mergeErrorStreamIntoOutput
                );

        /// <summary>
        /// exec a file with os shell exec or orbsh shell exec
        /// </summary>
        /// <param name="context">command evaluation context</param>
        /// <param name="comPath">command filePath</param>
        /// <param name="args">command line arguments string</param>
        /// <param name="output">shell exec result if any</param>
        /// <param name="waitForExit">true if wait for exec process exits</param>
        /// <param name="isStreamsEchoEnabled">if true, exec process output stream is echoized to context out (dump command output)</param>
        /// <param name="isOutputCaptureEnabled">if true capture the exec process output and give the result in parameter 'output'</param>
        /// <param name="mergeErrorStreamIntoOutput">if true merge exec process err stream content to the process output content (if process out capture is enabled)</param>
        /// <returns>exec process return code</returns>
        public int ShellExec(
            CommandEvaluationContext context,
            string comPath,
            string args,
            string workingDirectory,
            out string output,
            bool waitForExit = true,
            bool isStreamsEchoEnabled = true,
            bool isOutputCaptureEnabled = true,
            bool mergeErrorStreamIntoOutput = true
            )
        {
            try
            {
                output = null;
                workingDirectory ??= Environment.CurrentDirectory;
                var processStartInfo = new ProcessStartInfo()
                {
                    UseShellExecute = false,
                    //StandardOutputEncoding = Encoding.UTF8,   // keep system default
                    //StandardErrorEncoding = Encoding.UTF8,    // keep system default
                    RedirectStandardError = true,
                    RedirectStandardInput = false,
                    RedirectStandardOutput = true,
                    //LoadUserProfile = true,       // windows only
                    CreateNoWindow = true,
                    FileName = comPath,
                    Arguments = args,
                    WindowStyle = ProcessWindowStyle.Normal,
                    WorkingDirectory = workingDirectory
                };
                var sb = new StringBuilder();

                // batch shell exec ?
                if (Path.GetExtension(comPath) == context.ShellEnv.GetValue<string>(ShellEnvironmentVar.settings_clp_shellExecBatchExt))
                {
                    var batchMethod = typeof(CommandLineProcessorCommands).GetMethod(nameof(CommandLineProcessorCommands.Batch));
                    var r = Eval(context, batchMethod, "\"" + FileSystemPath.UnescapePathSeparators(comPath) + " " + args + "\"", 0);
                    output = sb.ToString();
                    return r.EvalResultCode;
                }

                var pw = ProcessWrapper.ThreadRun(
                    processStartInfo,
                    null,
                    (outStr) =>
                    {
                        if (isStreamsEchoEnabled) context.Out.Echoln(outStr);
                        if (isOutputCaptureEnabled) sb.AppendLine(outStr);
                    },
                    (errStr) =>
                    {
                        if (isStreamsEchoEnabled) context.Errorln(errStr);
                        if (isOutputCaptureEnabled && mergeErrorStreamIntoOutput) sb.AppendLine(errStr);
                    }
                );

                if (context.ShellEnv.IsOptionSetted(ShellEnvironmentVar.settings_clp_enableShellExecTraceProcessStart)) context.Out.Echoln($"{context.ShellEnv.Colors.TaskInformation}process '{Path.GetFileName(comPath)}' [{pw.Process.Id}] started(rdc)");

                int retCode = 0;

                if (waitForExit)
                {
                    pw.Process.WaitForExit();
                    retCode = pw.Process.ExitCode;


                    pw.StdOutCallBackThread.Join();
                    pw.StdErrCallBackThread.Join();


                    output = sb.ToString();

                    //if (pw.StdOutCallBackThread.ThreadState != System.Threading.ThreadState.WaitSleepJoin) pw.StdOutCallBackThread.Join();
                    //if (pw.StdErrCallBackThread.ThreadState != System.Threading.ThreadState.WaitSleepJoin) pw.StdErrCallBackThread.Join();
                }

                if (context.ShellEnv.IsOptionSetted(ShellEnvironmentVar.settings_clp_enableShellExecTraceProcessEnd)) context.Out.Echoln($"{context.ShellEnv.Colors.TaskInformation}process '{Path.GetFileName(comPath)}' exited with code: {retCode}(rdc)");

                return retCode;
            }
            catch (Exception shellExecException)
            {
                throw new Exception($"ShellExec error: {shellExecException}", shellExecException);
            }
            finally
            {
            }
        }


        /// <summary>
        /// react after parse within a parse work unit result
        /// </summary>
        /// <param name="context">command eval context</param>
        /// <param name="expr">command line expr</param>
        /// <param name="outputX">begin cursor x output</param>
        /// <param name="parseResult">parse resul</param>
        /// <returns>expression evaluation context</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:remove the parameter not used", Justification = "maybe in future change - waiting impl.")]
        ExpressionEvaluationResult AnalysisPipelineParseResult(
            CommandEvaluationContext context,
            PipelineParseResult pipelineParseResult,
            string expr,
            int outputX,
            ParseResult parseResult
            )
        {
            ExpressionEvaluationResult r = null;
            var errorText = "";
            string[] t;
            int idx;
            string serr;

            switch (parseResult.ParseResultType)
            {
                case ParseResultType.Empty:

                    r = new ExpressionEvaluationResult(expr, null, parseResult.ParseResultType, null, (int)ReturnCode.OK, null);
                    break;

                case ParseResultType.NotValid:  /* command syntax not valid */

                    var perComErrs = new Dictionary<string, List<CommandSyntaxParsingResult>>();
                    foreach (var prs in parseResult.SyntaxParsingResults)
                        if (prs.CommandSyntax != null)
                        {
                            if (perComErrs.TryGetValue(prs.CommandSyntax?.CommandSpecification?.Name, out var lst))
                                lst.Add(prs);
                            else
                                perComErrs.Add(prs.CommandSyntax.CommandSpecification.Name, new List<CommandSyntaxParsingResult> { prs });
                        }

                    var errs = new List<string>();
                    var minErrPosition = int.MaxValue;
                    var errPositions = new List<int>();
                    foreach (var kvp in perComErrs)
                    {
                        var comSyntax = kvp.Value.First().CommandSyntax;
                        foreach (var prs in kvp.Value)
                        {
                            foreach (var perr in prs.ParseErrors)
                            {
                                minErrPosition = Math.Min(minErrPosition, perr.Position);
                                errPositions.Add(perr.Position);
                                if (!errs.Contains(perr.Description))
                                    errs.Add(perr.Description);
                            }
                            errorText += Br + Red + string.Join(Br + Red, errs);
                        }
                        errorText += $"{Br}{Red}for syntax: {comSyntax}{Br}";
                    }

                    errPositions.Sort();
                    errPositions = errPositions.Distinct().ToList();

                    t = new string[expr.Length + 2];
                    for (int i = 0; i < t.Length; i++) t[i] = " ";
                    foreach (var errPos in errPositions)
                    {
                        t[GetIndex(context, errPos, expr)] = Settings.ErrorPositionMarker + "";
                    }
                    serr = string.Join("", t);
                    Error(" ".PadLeft(outputX + 1) + serr, false, false);

                    Error(errorText);
                    r = new ExpressionEvaluationResult(expr, errorText, parseResult.ParseResultType, null, (int)ReturnCode.NotIdentified, null);
                    break;

                case ParseResultType.Ambiguous:

                    errorText += $"{Red}ambiguous syntaxes:{Br}";
                    foreach (var prs in parseResult.SyntaxParsingResults)
                        errorText += $"{Red}{prs.CommandSyntax}{Br}";
                    Error(errorText);
                    r = new ExpressionEvaluationResult(expr, errorText, parseResult.ParseResultType, null, (int)ReturnCode.NotIdentified, null);
                    break;

                case ParseResultType.NotIdentified:

                    t = new string[expr.Length + 2];
                    for (int j = 0; j < t.Length; j++) t[j] = " ";
                    var err = parseResult.SyntaxParsingResults.First().ParseErrors.First();
                    idx = err.Position;
                    t[idx] = Settings.ErrorPositionMarker + "";
                    errorText += Red + err.Description;
                    serr = string.Join("", t);
                    context.Errorln(" ".PadLeft(outputX) + serr);
                    context.Errorln(errorText);
                    r = new ExpressionEvaluationResult(expr, errorText, parseResult.ParseResultType, null, (int)ReturnCode.NotIdentified, null);
                    break;

                case ParseResultType.SyntaxError:

                    t = new string[expr.Length + 2];
                    for (int j = 0; j < t.Length; j++) t[j] = " ";
                    var err2 = parseResult.SyntaxParsingResults.First().ParseErrors.First();
                    idx = err2.Index;
                    t[idx] = Settings.ErrorPositionMarker + "";
                    errorText += Red + err2.Description;
                    serr = string.Join("", t);
                    context.Errorln(" ".PadLeft(outputX) + serr);
                    context.Errorln(errorText);
                    r = new ExpressionEvaluationResult(expr, errorText, parseResult.ParseResultType, null, (int)ReturnCode.NotIdentified, null);
                    break;
            }

            return r;
        }

        static object InvokeCommand(
            CommandEvaluationContext context,
            CommandSpecification commandSpecification,
            MatchingParameters matchingParameters)
        {
            var parameters = new List<object>() { context };
            var pindex = 0;
            foreach (var parameter in commandSpecification.MethodInfo.GetParameters())
            {
                if (pindex > 0)
                {
                    if (matchingParameters.TryGet(parameter.Name, out var matchingParameter))
                        parameters.Add(matchingParameter.GetValue());
                    else
                        throw new InvalidOperationException($"parameter not found: '{parameter.Name}' when invoking command: {commandSpecification}");
                }
                pindex++;
            }
            var r = commandSpecification.MethodInfo
                .Invoke(commandSpecification.MethodOwner, parameters.ToArray());
            return r;
        }

        #endregion
    }
}
