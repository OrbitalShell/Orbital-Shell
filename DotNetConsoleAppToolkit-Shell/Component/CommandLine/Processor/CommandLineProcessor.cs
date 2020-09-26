//#define enable_test_commands
#define BannerEnabled

using DotNetConsoleAppToolkit.Component.CommandLine.CommandBatch;
using DotNetConsoleAppToolkit.Component.CommandLine.CommandModel;
using DotNetConsoleAppToolkit.Component.CommandLine.Data;
using DotNetConsoleAppToolkit.Component.CommandLine.Parsing;
using DotNetConsoleAppToolkit.Component.CommandLine.Pipeline;
using DotNetConsoleAppToolkit.Component.CommandLine.Variable;
using DotNetConsoleAppToolkit.Console;
using DotNetConsoleAppToolkit.Lib;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using static DotNetConsoleAppToolkit.Component.CommandLine.Parsing.CommandLineParser;
using static DotNetConsoleAppToolkit.DotNetConsole;
using cmdlr = DotNetConsoleAppToolkit.Component.CommandLine.CommandLineReader;
using cons = System.Console;

namespace DotNetConsoleAppToolkit.Component.CommandLine.Processor
{
    public class CommandLineProcessor
    {
        static object _logFileLock = new object();

        #region attributes

        public CommandLineProcessorSettings Settings { get; protected set; }

        public CancellationTokenSource CancellationTokenSource;

        string[] _args;
        bool _isInitialized = false;

        readonly Dictionary<string, List<CommandSpecification>> _commands = new Dictionary<string, List<CommandSpecification>>();

        public ReadOnlyDictionary<string, List<CommandSpecification>> Commands => new ReadOnlyDictionary<string, List<CommandSpecification>>(_commands);

        public List<CommandSpecification> AllCommands {
            get {
                var coms = new List<CommandSpecification>();
                foreach (var kvp in _commands)
                    foreach (var com in kvp.Value)
                        coms.Add(com);
                coms.Sort(new Comparison<CommandSpecification>((x, y) => x.Name.CompareTo(y.Name)));
                return coms;
            }
        }

        readonly SyntaxAnalyser _syntaxAnalyzer = new SyntaxAnalyser();

        readonly Dictionary<string, CommandsModule> _modules = new Dictionary<string, CommandsModule>();

        public IReadOnlyDictionary<string, CommandsModule> Modules => new ReadOnlyDictionary<string, CommandsModule>(_modules);

        public IEnumerable<string> CommandDeclaringShortTypesNames => AllCommands.Select(x => x.DeclaringTypeShortName).Distinct();
        public IEnumerable<string> CommandDeclaringTypesNames => AllCommands.Select(x => x.DeclaringTypeFullName).Distinct();

        public CommandsHistory CmdsHistory { get; protected set; }

        public CommandsAlias CommandsAlias { get; protected set; }

        public cmdlr.CommandLineReader CommandLineReader { get; set; }

        public CommandEvaluationContext CommandEvaluationContext { get; protected set; }

        public CommandBatchProcessor CommandBatchProcessor { get; protected set; }

        #endregion

        #region cli methods

        public string Arg(int n)
        {
            if (_args == null) return null;
            if (_args.Length <= n) return null;
            return _args[n];
        }

        public bool HasArgs => _args != null && _args.Length > 0;

        void SetArgs(string[] args)
        {
            _args = (string[])args?.Clone();
        }

        #endregion

        #region command engine operations

        public CommandLineProcessor(
            string[] args,
            CommandLineProcessorSettings settings = null,
            CommandEvaluationContext commandEvaluationContext = null
            )
        {
            settings ??= new CommandLineProcessorSettings();
            CommandBatchProcessor = new CommandBatchProcessor();
            ShellInit(args, settings, commandEvaluationContext);
        }

        void SetupShellEnvVar()
        {
            var envn = CommandEvaluationContext
                .CommandLineProcessor
                .Settings
                .ShellEnvironmentVariableName;
            var env = new ShellEnvironment(envn);
            env.Initialize(CommandEvaluationContext);
            CommandEvaluationContext.Variables.Set(VariableNameSpace.Env,envn,env);
        }

        void ShellInit(
            string[] args,
            CommandLineProcessorSettings settings, 
            CommandEvaluationContext commandEvaluationContext = null)
        {
            Settings = settings;
            SetArgs(args);

            cons.ForegroundColor = DefaultForeground;
            cons.BackgroundColor = DefaultBackground;

            commandEvaluationContext = commandEvaluationContext ??
                new CommandEvaluationContext(
                    this,
                    Out,
                    cons.In,
                    Err,
                    null
                );
            CommandEvaluationContext = commandEvaluationContext;

            SetupShellEnvVar();

            if (settings.PrintInfo) PrintInfo(CommandEvaluationContext);

            // assume the application folder ($env.APPDATA/OrbitalShell) exists and is initialized

            var lbr = false;

            // creates user app data folders
            if (!Directory.Exists(Settings.AppDataFolderPath))
            {
                Settings.LogAppendAllLinesErrorIsEnabled = false;
                Info(ColorSettings.Log + $"creating user shell folder: '{Settings.AppDataFolderPath}' ... ",false);
                try
                {
                    Directory.CreateDirectory(Settings.AppDataFolderPath);
                    Success();
                } catch (Exception createAppDataFolderPathException)
                {
                    Fail(createAppDataFolderPathException);
                }
                lbr = true;
            }

            // initialize log file
            if (!File.Exists(Settings.LogFilePath))
            {
                Info(ColorSettings.Log + $"creating log file: '{Settings.LogFilePath}' ... ",false);
                try
                {
                    var logError = Log($"file created on {System.DateTime.Now}");
                    if (logError==null)
                        Success();
                    else
                        throw logError;
                }
                catch (Exception createLogFileException)
                {
                    Settings.LogAppendAllLinesErrorIsEnabled = false;
                    Fail(createLogFileException);
                }
                lbr = true;
            }

            // initialize user profile
            if (!File.Exists(Settings.UserProfileFilePath))
            {
                Info(ColorSettings.Log + $"creating user profile file: '{Settings.UserProfileFilePath}' ... ",false);
                try
                {
                    var defaultProfileFilePath = Path.Combine(Settings.DefaultsFolderPath, Settings.UserProfileFileName);
                    File.Copy(defaultProfileFilePath, Settings.UserProfileFilePath);
                    Success();
                }
                catch (Exception createUserProfileFileException)
                {
                    Fail(createUserProfileFileException);
                }
                lbr = true;
            }

            // create/restore commands history
            CmdsHistory = new CommandsHistory();
            var createNewHistoryFile = !File.Exists(Settings.HistoryFilePath);
            if (createNewHistoryFile)                
                Info(ColorSettings.Log + $"creating user commands history file: '{Settings.HistoryFilePath}' ... ", false);
            try
            {
                if (createNewHistoryFile)
#pragma warning disable CS0642 // Possibilité d'instruction vide erronée
                    using (var fs = File.Create(Settings.HistoryFilePath)) ;
#pragma warning restore CS0642 // Possibilité d'instruction vide erronée
                CmdsHistory.Init(Settings.AppDataFolderPath, Settings.HistoryFileName);
                if (createNewHistoryFile) Success();
            }
            catch (Exception createUserProfileFileException)
            {
                Fail(createUserProfileFileException);
            }
            lbr |= createNewHistoryFile;

            // create/restore user aliases
            CommandsAlias = new CommandsAlias();
            var createNewCommandsAliasFile = !File.Exists(Settings.CommandsAliasFilePath);
            if (createNewCommandsAliasFile)
                Info(ColorSettings.Log + $"creating user commands aliases file: '{Settings.CommandsAliasFilePath}' ... ", false);
            try
            {
                if (createNewCommandsAliasFile)
                {
                    var defaultAliasFilePath = Path.Combine(Settings.DefaultsFolderPath, Settings.CommandsAliasFileName);
                    File.Copy(defaultAliasFilePath, Settings.CommandsAliasFilePath);
                }
                if (createNewCommandsAliasFile) Success();                    
            }
            catch (Exception createUserProfileFileException)
            {
                Fail(createUserProfileFileException);
            }
            lbr |= createNewHistoryFile;

            // end inits
            if (lbr) Out.Echoln();

            // load kernel commands
            RegisterCommandsAssembly(CommandEvaluationContext,Assembly.GetExecutingAssembly());
#if enable_test_commands
            RegisterCommandsClass<TestCommands>();
#endif

        }

        public void Initialize()
        {
            if (_isInitialized) return;
            // run user profile
            CommandBatchProcessor.RunBatch(CommandEvaluationContext, Settings.UserProfileFilePath);
            // run user aliases
            CommandsAlias.Init(CommandEvaluationContext, Settings.AppDataFolderPath, Settings.CommandsAliasFileName);
            _isInitialized = true;
        }

        void Success(string message = null)
        {
            var logMessage = ColorSettings.Success + "Success" + (message==null?"":$" : {message}");
            Out.Echoln(logMessage);
            Log(logMessage);
        }

        void Info(string message,bool lineBreak=true) {
            var logMessage = ColorSettings.Log + message;
            Out.Echo(logMessage,lineBreak);
            Log(logMessage);
        }

        void Fail(string message=null, bool lineBreak = true)
        {
            var logMessage = ColorSettings.Error + "Fail" + (message == null ? "" : $" : {message}");
            Out.Echo(logMessage, lineBreak);
            Log(logMessage);
        }

        void Fail(Exception exception)
        {
            var logMessage = ColorSettings.Error + "Fail : " + exception?.Message ;
            Out.Echoln(logMessage);
            LogError(logMessage);
        }

        public void AssertCommandLineProcessorHasACommandLineReader()
        {
            if (CommandLineReader == null) throw new Exception("a command line reader is required by the command line processor to perform this action");
        }

        public void PrintInfo(CommandEvaluationContext context)
        {
            context.Out.Echoln($"{ColorSettings.Label}{Uon} {Settings.AppLongName} ({Settings.AppName}) version {Assembly.GetExecutingAssembly().GetName().Version}" + ("".PadRight(18,' ')) + Tdoff);
            context.Out.Echoln($" {Settings.AppEditor}");
            context.Out.Echoln($" {RuntimeInformation.OSDescription} {RuntimeInformation.OSArchitecture} - {RuntimeInformation.FrameworkDescription}");

#if BannerEnabled
            var banner =
                File.ReadAllLines(
                Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "Shell",
                    "Commands",
                    "CommandLineProcessor",
                    "banner.txt"));
            int c = 202;
            foreach ( var line in banner )
            {
                context.Out.SetForeground(c);
                context.Out.Echoln(line);
                c++;
            }
#endif
            context.Out.Echoln();
        }

        #region shell log operations
        
        public Exception Log(string text)
        {
            return LogInternal(text);
        }

        public Exception LogError(string text)
        {
            return LogInternal(text, ColorSettings.Error+"ERR");
        }

        Exception LogInternal(string text,string logPrefix="INF")
        {
            var str = $"{logPrefix} [{Process.GetCurrentProcess().ProcessName}:{Process.GetCurrentProcess().Id},{Thread.CurrentThread.Name}:{Thread.CurrentThread.ManagedThreadId}] {System.DateTime.Now}.{System.DateTime.Now.Millisecond} | {text}";
            lock ( _logFileLock )
            {
                try
                {
                    File.AppendAllLines(Settings.LogFilePath, new List<string> { str });
                    return null;
                } catch (Exception logAppendAllLinesException)
                {
                    if (Settings.LogAppendAllLinesErrorIsEnabled)
                        Errorln(logAppendAllLinesException.Message);
                    return logAppendAllLinesException;
                }
            }
        }

        #endregion

        #region commands registration

        public (int typesCount,int commandsCount) 
            UnregisterCommandsAssembly(
            CommandEvaluationContext context,
            string assemblyName)
        {
            var module = _modules.Values.Where(x => x.Name == assemblyName).FirstOrDefault();
            if (module!=null)
            {
                foreach (var com in AllCommands)
                    if (com.MethodInfo.DeclaringType.Assembly == module.Assembly)
                        RemoveCommand(com);
                return (module.TypesCount, module.CommandsCount);
            }
            else
            {
                Errorln($"commands module '{assemblyName}' not registered");
                return (0, 0);
            }
        }

        bool RemoveCommand(CommandSpecification comSpec)
        {
            if (_commands.TryGetValue(comSpec.Name, out var cmdLst))
            {
                var r = cmdLst.Remove(comSpec);
                if (r)
                    _syntaxAnalyzer.Remove(comSpec);
                if (cmdLst.Count == 0)
                    _commands.Remove(comSpec.Name);
                return r;
            }
            return false;
        }

        public (int typesCount, int commandsCount) RegisterCommandsAssembly(
            CommandEvaluationContext context,
            string assemblyPath)
        {
            var assembly = Assembly.LoadFrom(assemblyPath);
            return RegisterCommandsAssembly(context,assembly);
        }

        public (int typesCount,int commandsCount) RegisterCommandsAssembly(
            CommandEvaluationContext context,
            Assembly assembly)
        {
            if (_modules.ContainsKey(assembly.ManifestModule.Name))
            {
                Errorln($"commands module already registered: '{assembly.FullName}'");
                return (0,0);
            }
            var typesCount = 0;
            var comTotCount = 0;
            foreach ( var type in assembly.GetTypes())
            {
                var comsAttr = type.GetCustomAttribute<CommandsAttribute>();

                var comCount = 0;
                if (comsAttr != null && type.GetInterface(typeof(ICommandsDeclaringType).FullName)!=null )
                    comCount = RegisterCommandsClass(context,type,false);                
                if (comCount > 0)
                    typesCount++;
                comTotCount += comCount;
            }
            if (typesCount > 0)
            {
                var descAttr = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>();
                var description = (descAttr != null) ? descAttr.Description : "";
                _modules.Add(Path.GetFileNameWithoutExtension(assembly.ManifestModule.Name), new CommandsModule(Path.GetFileNameWithoutExtension(assembly.Location), description, assembly, typesCount, comTotCount));
            }
            return (typesCount,comTotCount);    
        }

        public void RegisterCommandsClass<T>(CommandEvaluationContext context) => RegisterCommandsClass(context,typeof(T),true);

        public int RegisterCommandsClass(CommandEvaluationContext context, Type type) => RegisterCommandsClass(context,type, true);

        int RegisterCommandsClass(CommandEvaluationContext context, Type type,bool registerAsModule)
        {
            if (type.GetInterface(typeof(ICommandsDeclaringType).FullName)==null)
                throw new Exception($"the type '{type.FullName}' must implements interface '{typeof(ICommandsDeclaringType).FullName}' to be registered as a command class");
            var comsCount = 0;
            object instance = Activator.CreateInstance(type,new object[] { });            
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (registerAsModule && _modules.ContainsKey(type.FullName))
            {
                Errorln($"a module with same name than commands type '{type.FullName}' is already registered");
                return 0;
            }
            foreach ( var method in methods )
            {
                var cmd = method.GetCustomAttribute<CommandAttribute>();
                if (cmd!=null)
                {
                    if (!method.ReturnType.HasInterface(typeof(ICommandResult)))
                    {
                        Errorln($"class={type.FullName} method={method.Name} wrong return type. should be of type '{typeof(ICommandResult).FullName}', but is of type: {method.ReturnType.FullName}");
                    }
                    else
                    {
                        var paramspecs = new List<CommandParameterSpecification>();
                        bool syntaxError = false;
                        var pindex = 0;
                        foreach (var parameter in method.GetParameters())
                        {
                            if (pindex == 0)
                            {
                                // manadatory: param 0 is CommandEvaluationContext
                                if (parameter.ParameterType != typeof(CommandEvaluationContext))
                                {
                                    Errorln($"class={type.FullName} method={method.Name} parameter 0 ('{parameter.Name}') should be of type '{typeof(CommandEvaluationContext).FullName}', but is of type: {parameter.ParameterType.FullName}");
                                    syntaxError = true;
                                    break;
                                }
                            }
                            else
                            {
                                CommandParameterSpecification pspec = null;
                                var paramAttr = parameter.GetCustomAttribute<ParameterAttribute>();
                                object defval = null;
                                if (!parameter.HasDefaultValue && parameter.ParameterType.IsValueType)
                                    defval = Activator.CreateInstance(parameter.ParameterType);

                                if (paramAttr != null)
                                {
                                    // TODO: validate command specification (eg. indexs validity)
                                    pspec = new CommandParameterSpecification(
                                        parameter.Name,
                                        paramAttr.Description,
                                        paramAttr.IsOptional,
                                        paramAttr.Index,
                                        null,
                                        true,
                                        parameter.HasDefaultValue,
                                        (parameter.HasDefaultValue) ? parameter.DefaultValue : defval,
                                        parameter);
                                }
                                var optAttr = parameter.GetCustomAttribute<OptionAttribute>();
                                if (optAttr != null)
                                {
                                    var reqParamAttr = parameter.GetCustomAttribute<OptionRequireParameterAttribute>();
                                    try
                                    {
                                        pspec = new CommandParameterSpecification(
                                            parameter.Name,
                                            optAttr.Description,
                                            optAttr.IsOptional,
                                            -1,
                                            optAttr.OptionName ?? parameter.Name,
                                            optAttr.HasValue,
                                            parameter.HasDefaultValue,
                                            (parameter.HasDefaultValue) ? parameter.DefaultValue : defval,
                                            parameter,
                                            reqParamAttr?.RequiredParameterName);
                                    }
                                    catch (Exception ex)
                                    {
                                        Errorln(ex.Message);
                                    }
                                }
                                if (pspec == null)
                                {
                                    syntaxError = true;
                                    Errorln($"invalid parameter: class={type.FullName} method={method.Name} name={parameter.Name}");
                                }
                                else
                                    paramspecs.Add(pspec);
                            }
                            pindex++;
                        }

                        if (!syntaxError)
                        {
                            var cmdNameAttr = method.GetCustomAttribute<CommandNameAttribute>();

                            var cmdName = (cmdNameAttr != null && cmdNameAttr.Name != null) ? cmdNameAttr.Name
                                : (cmd.Name ?? method.Name.ToLower());

                            var cmdspec = new CommandSpecification(
                                cmdName,
                                cmd.Description,
                                cmd.LongDescription,
                                cmd.Documentation,
                                method,
                                instance,
                                paramspecs);

                            bool registered = true;
                            if (_commands.TryGetValue(cmdspec.Name, out var cmdlst))
                            {
                                if (cmdlst.Select(x => x.MethodInfo.DeclaringType == type).Any())
                                {
                                    Errorln($"command already registered: '{cmdspec.Name}' in type '{cmdspec.DeclaringTypeFullName}'");
                                    registered = false;
                                }
                                else
                                    cmdlst.Add(cmdspec);
                            }
                            else
                                _commands.Add(cmdspec.Name, new List<CommandSpecification> { cmdspec });

                            if (registered)
                            {
                                _syntaxAnalyzer.Add(cmdspec);
                                comsCount++;
                            }
                        }
                    }
                }
            }
            if (registerAsModule)
            {
                if (comsCount == 0)
                    Errorln($"no commands found in type '{type.FullName}'");
                else
                {
                    var descAttr = type.GetCustomAttribute<CommandsAttribute>();
                    var description = descAttr != null ? descAttr.Description : "";
                    _modules.Add(type.FullName, new CommandsModule(CommandsModule.DeclaringTypeShortName(type), description, type.Assembly, 1, comsCount, type));
                }
            }
            return comsCount;
        }

        #endregion

        #endregion

        #region commands operations

        /// <summary>
        /// 1. parse command line
        /// 2. execute command or error
        ///     A. internal command (modules)
        ///     B. underlying shell command
        ///     C. unknown command
        /// </summary>
        /// <param name="expr">expression to be evaluated</param>
        /// <returns>return code</returns>
        public ExpressionEvaluationResult Eval(
            CommandEvaluationContext context,
            string expr,
            int outputX)
        {
            var pipelineParseResults = Parse(context, _syntaxAnalyzer, expr);
            bool allValid = true;
            var evalParses = new List<ExpressionEvaluationResult>();
            foreach ( var pipelineParseResult in pipelineParseResults )
            {
                allValid &= pipelineParseResult.ParseResult.ParseResultType == ParseResultType.Valid;
                var evalRes = EvalParse(context, expr, outputX, pipelineParseResult.ParseResult);
                evalParses.Add(evalRes);
            }
            if (!allValid) return evalParses.FirstOrDefault();
            return PipelineProcessor.RunPipeline(context, pipelineParseResults.FirstOrDefault());
        }

        ExpressionEvaluationResult EvalParse(
            CommandEvaluationContext context,
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
                /*
                    case ParseResultType.Valid:
                    var syntaxParsingResult = parseResult.SyntaxParsingResults.First();
                    try
                    {
                        var outputData = InvokeCommand(CommandEvaluationContext, syntaxParsingResult.CommandSyntax.CommandSpecification, syntaxParsingResult.MatchingParameters);
                        
                        r = new ExpressionEvaluationResult(null, ParseResultType.Valid, outputData, (int)ReturnCode.OK, null);
                    } catch (Exception commandInvokeError)
                    {
                        var commandError = commandInvokeError.InnerException ?? commandInvokeError;
                        Errorln(commandError.Message);
                        return new ExpressionEvaluationResult(null, parseResult.ParseResultType, null, (int)ReturnCode.Error, commandError);
                    }
                    break;
                */

                case ParseResultType.Empty:
                    r = new ExpressionEvaluationResult(null, parseResult.ParseResultType, null, (int)ReturnCode.OK, null);
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
                            errorText += Br + Red + string.Join(Br+Red, errs);
                        }
                        errorText += $"{Br}{Red}for syntax: {comSyntax}{Br}";
                    }

                    errPositions.Sort();
                    errPositions = errPositions.Distinct().ToList();

                    t = new string[expr.Length + 2];
                    for (int i = 0; i < t.Length; i++) t[i] = " ";
                    foreach (var errPos in errPositions)
                    {
                        t[GetIndex(context, errPos, expr)] = Settings.ErrorPositionMarker+"";
                    }
                    serr = string.Join("", t);
                    Error(" ".PadLeft(outputX + 1) + serr);

                    Error(errorText);
                    r = new ExpressionEvaluationResult(errorText, parseResult.ParseResultType, null, (int)ReturnCode.NotDefined, null);
                    break;

                case ParseResultType.Ambiguous:
                    errorText += $"{Red}ambiguous syntaxes:{Br}";
                    foreach (var prs in parseResult.SyntaxParsingResults)
                        errorText += $"{Red}{prs.CommandSyntax}{Br}";
                    Error(errorText);
                    r = new ExpressionEvaluationResult(errorText, parseResult.ParseResultType, null, (int)ReturnCode.NotDefined, null);
                    break;

                case ParseResultType.NotIdentified:
                    t = new string[expr.Length + 2];
                    for (int j = 0; j < t.Length; j++) t[j] = " ";
                    var err = parseResult.SyntaxParsingResults.First().ParseErrors.First();
                    idx = err.Position;
                    t[idx] = Settings.ErrorPositionMarker+"";
                    errorText += Red + err.Description;
                    serr = string.Join("", t);
                    Errorln(" ".PadLeft(outputX) + serr);
                    Errorln(errorText);
                    r = new ExpressionEvaluationResult(errorText, parseResult.ParseResultType, null, (int)ReturnCode.NotDefined, null);
                    break;

                case ParseResultType.SyntaxError:
                    t = new string[expr.Length + 2];
                    for (int j = 0; j < t.Length; j++) t[j] = " ";
                    var err2 = parseResult.SyntaxParsingResults.First().ParseErrors.First();
                    idx = err2.Index;
                    t[idx] = Settings.ErrorPositionMarker+"";
                    errorText += Red + err2.Description;
                    serr = string.Join("", t);
                    Errorln(" ".PadLeft(outputX) + serr);
                    Errorln(errorText);
                    r = new ExpressionEvaluationResult(errorText, parseResult.ParseResultType, null, (int)ReturnCode.NotDefined, null);
                    break;
            }

            return r;
        }

        object InvokeCommand(
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
