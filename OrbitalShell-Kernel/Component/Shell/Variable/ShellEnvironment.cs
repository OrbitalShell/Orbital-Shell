using OrbitalShell.Component.Shell.Data;
using OrbitalShell.Component.CommandLine.Parsing;
using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Console;
using OrbitalShell.Lib.FileSystem;
using OrbitalShell.Lib.Sys;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using static OrbitalShell.Lib.Str;
using System.Collections.Generic;
using System.Data;
using OrbitalShell.Component.Shell.Module;

namespace OrbitalShell.Component.Shell.Variable
{
    /// <summary>
    /// variables of the shell
    /// </summary>
    public class ShellEnvironment : DataObject
    {
        public const string SystemPathSeparator = ";";

        public Variables Vars { get; protected set; }

        public ColorSettings Colors { get; protected set; }

        public TableFormattingOptions TableFormattingOptions => GetValue<TableFormattingOptions>(ShellEnvironmentVar.display_tableFormattingOptions);
        
        public FileSystemPathFormattingOptions FileSystemPathFormattingOptions => GetValue<FileSystemPathFormattingOptions>(ShellEnvironmentVar.display_fileSystemPathFormattingOptions);

        public readonly ModuleInitModel ModuleInitModel;

        public ShellEnvironment(string name) : base(name, false) { }

        /// <summary>
        /// creates the standard shell<br/>
        /// - add known namespaces and values names<br/>
        /// - setup shell variables
        /// </summary>
        /// <param name="context"></param>
        public void Initialize(CommandEvaluationContext context)
        {            
            Vars = context.Variables;

            // data objects

            foreach (var shellNs in Enum.GetValues(typeof(ShellEnvironmentNamespace)))
            {
                var ns = (ShellEnvironmentNamespace)shellNs;
                if (!HasObject(ns)) AddObject(ns);
            }

            // debug

            AddValue(ShellEnvironmentVar.debug_enablePipelineTrace, false);
            AddValue(ShellEnvironmentVar.debug_enableHookTrace, false);

            // env

            // global settings

            AddValue(ShellEnvironmentVar.display_fileSystemPathFormattingOptions, new FileSystemPathFormattingOptions());
            AddValue(ShellEnvironmentVar.display_tableFormattingOptions, new TableFormattingOptions());
            var o = AddValue(ShellEnvironmentVar.display_colors_colorSettings, new ColorSettings(context.CommandLineProcessor.Console));
            Colors = (ColorSettings)o.Value;

            // bash vars (extended)

            AddValue(ShellEnvironmentVar.shell, new DirectoryPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)), true);
            AddValue(ShellEnvironmentVar.SHELL__VERSION, context.CommandLineProcessor.Settings.AppVersion, true);
            AddValue(ShellEnvironmentVar.SHELL__NAME, context.CommandLineProcessor.Settings.AppName, true);
            AddValue(ShellEnvironmentVar.SHELL__LONG__NAME, context.CommandLineProcessor.Settings.AppLongName, true);
            AddValue(ShellEnvironmentVar.SHELL__EDITOR, context.CommandLineProcessor.Settings.AppEditor, true);
            AddValue(ShellEnvironmentVar.SHELL__LICENSE, context.CommandLineProcessor.Settings.AppLicense, true);
            AddValue(ShellEnvironmentVar.home, new DirectoryPath(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)), true);
            AddValue(ShellEnvironmentVar.modules, new DirectoryPath(CommandLineProcessorSettings.ModulesFolderPath), true);
            AddValue(ShellEnvironmentVar.init, new DirectoryPath(context.CommandLineProcessor.Settings.ShellAppDataPath), true);
            AddValue(ShellEnvironmentVar.userProfile, new DirectoryPath(context.CommandLineProcessor.Settings.AppDataRoamingUserFolderPath), true);
            var path = GetSystemPath();
            var pathExt = GetSystemPathExt();
            var pl = new List<DirectoryPath>();
            var plx = new List<string>();
            if (path != null)
            {
                var paths = path.Split(SystemPathSeparator).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => Environment.ExpandEnvironmentVariables(x));
                var dps = paths.Select(x => new DirectoryPath(x)).ToList();
                dps.Add(new DirectoryPath(Path.Combine(GetValue<DirectoryPath>(ShellEnvironmentVar.shell).FullName, "scripts")));
                pl.AddRange(dps);
            }
            if (pathExt != null)
            {
                plx.AddRange(pathExt.Split(SystemPathSeparator));
            }
            AddValue(ShellEnvironmentVar.path, pl);
            AddValue(ShellEnvironmentVar.pathExt, plx);
            AddValue(ShellEnvironmentVar.pathExtInit, "");

            // shell settings (defaults)

            AddValue(ShellEnvironmentVar.settings_module_providerUrls, new List<string> { "https://raw.githubusercontent.com/OrbitalShell/Orbital-Shell/master/module-index-repository/module-list" });
            AddValue(ShellEnvironmentVar.settings_console_prompt, ANSI.RSTXTA + "> ");        // prompt   
            AddValue(ShellEnvironmentVar.settings_console_initialWindowWidth, -1);
            AddValue(ShellEnvironmentVar.settings_console_initialWindowHeight, -1);
            AddValue(ShellEnvironmentVar.settings_console_enableCompatibilityMode, false);
            AddValue(ShellEnvironmentVar.settings_console_enableAvoidEndOfLineFilledWithBackgroundColor, true);
            AddValue(ShellEnvironmentVar.settings_clr_comPreAnalysisOutput, ANSI.CRLF);
            AddValue(ShellEnvironmentVar.settings_clr_comPostExecOutModifiedOutput, ANSI.CRLF);
            AddValue(ShellEnvironmentVar.settings_clr_comPostExecOutput, "");
            AddValue(ShellEnvironmentVar.settings_clp_enableShellExecTraceProcessStart, false);
            AddValue(ShellEnvironmentVar.settings_clp_enableShellExecTraceProcessEnd, false);
            AddValue(ShellEnvironmentVar.settings_clp_shellExecBatchExt, ".sh");
            AddValue(ShellEnvironmentVar.settings_console_banner_path,
                Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                        context.CommandLineProcessor.Settings.KernelCommandsRootNamespace,
                        nameof(CommandLineProcessor),
                        "banner-4.txt"));
            AddValue(ShellEnvironmentVar.settings_console_banner_isEnabled, true);
            AddValue(ShellEnvironmentVar.settings_console_banner_startColorIndex, 202);// 167);
            AddValue(ShellEnvironmentVar.settings_console_banner_colorIndexStep, 1);// 167);

            InitializeSpecialVars(context);

            InitializeArgsVars(context.CommandLineProcessor.Args);

            // @TODO: override settings from a config file .json (do also for CommandLineProcessorSettings)
        }

        #region path util

        static string GetSystemPath()
        {
            string s;
            object r = null;
            var t = Environment.GetEnvironmentVariables();
            if (t.Contains(s = "Path")) r = t[s];
            if (t.Contains(s = "path")) r = t[s];
            if (t.Contains(s = "PATH")) r = t[s];
            return (string)r;
        }

        static string GetSystemPathExt()
        {
            string s;
            object r = null;
            var t = Environment.GetEnvironmentVariables();
            if (t.Contains(s = "PathExt")) r = t[s];
            if (t.Contains(s = "pathExt")) r = t[s];
            if (t.Contains(s = "path_ext")) r = t[s];
            if (t.Contains(s = "Path_Ext")) r = t[s];
            if (t.Contains(s = "PATH_EXT")) r = t[s];
            if (t.Contains(s = "PATHEXT")) r = t[s];
            return (string)r;
        }

        #endregion

        /// <summary>
        /// init shell env special vars
        /// </summary>
        void InitializeSpecialVars(CommandEvaluationContext context)
        {
            var o = AddValue(ShellEnvironmentVar.sp__lastCommandReturnCode, ReturnCode.NotIdentified);
            AddValue(ShellEnvironmentVar.lastComReturnCode, o.Value);

            o = AddValue(ShellEnvironmentVar.sp__lastCommandResult, new object());
            AddValue(ShellEnvironmentVar.lastComResult, o.Value);
            o.SetValue(null);

            o = AddValue(ShellEnvironmentVar.sp__lastCommand, "");
            AddValue(ShellEnvironmentVar.lastCom, o.Value);

            o = AddValue(ShellEnvironmentVar.sp__lastCommandException, new Exception());
            AddValue(ShellEnvironmentVar.lastComException, o.Value);
            o.SetValue(null);

            o = AddValue(ShellEnvironmentVar.sp__lastCommandErrorText, new StringWrapper());
            AddValue(ShellEnvironmentVar.lastComErrorText, o.Value);

            o = AddValue(ShellEnvironmentVar.sp__activeShellPID, Environment.ProcessId, true);
            AddValue(ShellEnvironmentVar.activeShellPID, o.Value, true);

            o = AddValue(ShellEnvironmentVar.sp__activeShellThreadID, System.Threading.Thread.CurrentThread.ManagedThreadId, true);
            AddValue(ShellEnvironmentVar.activeShellThreadID, o.Value, true);

            o = AddValue(ShellEnvironmentVar.sp__lastTaskThreadID, -1);
            AddValue(ShellEnvironmentVar.lastTaskThreadID, o.Value);

            o = AddValue(ShellEnvironmentVar.sp__shellOpts, string.Join(" ", context.CommandLineProcessor.Args), true);
            AddValue(ShellEnvironmentVar.shellOpts, o.Value, true);

            o = AddValue(ShellEnvironmentVar.sp__activeShellContextID, context.ID);
            AddValue(ShellEnvironmentVar.activeShellContextID, o.Value);
        }

        /// <summary>
        /// set shell vars for last command return info
        /// </summary>
        /// <param name="com">com text</param>
        /// <param name="result">command result</param>
        /// <param name="returnCode">code returned</param>
        /// <param name="errorText">eventually (default blank) text of error returned</param>
        /// <param name="exception">eventually (default null) com exception</param>
        public void UpdateVarLastCommandReturn(
            string com,
            object result,
            ReturnCode returnCode,
            string errorText = "",
            Exception exception = null)
        {
            SetValue(ShellEnvironmentVar.sp__lastCommandReturnCode, returnCode);
            SetValue(ShellEnvironmentVar.lastComReturnCode, returnCode);
            var err = (exception != null) ? exception.Message : errorText;
            SetValue(ShellEnvironmentVar.sp__lastCommandErrorText, new StringWrapper(err, Colors.Error + ""));
            SetValue(ShellEnvironmentVar.lastComErrorText, new StringWrapper(err, Colors.Error + ""));
            SetValue(ShellEnvironmentVar.sp__lastCommandException, exception);
            SetValue(ShellEnvironmentVar.lastComException, exception);
            SetValue(ShellEnvironmentVar.sp__lastCommandResult, result);
            SetValue(ShellEnvironmentVar.lastComResult, result);
            SetValue(ShellEnvironmentVar.sp__lastCommand, com);
            SetValue(ShellEnvironmentVar.lastCom, com);
        }

        /// <summary>
        /// update context info
        /// </summary>
        /// <param name="contextID">context id</param>
        public void UpdateVarContext(int contextID)
        {
            SetValue(ShellEnvironmentVar.sp__activeShellContextID, contextID);
            SetValue(ShellEnvironmentVar.activeShellContextID, contextID);
        }

        /// <summary>
        /// initialize args env vars
        /// </summary>
        /// <param name="args">args</param>
        void InitializeArgsVars(string[] args)
        {
            var t_a = string.Join(" ", args);
            var t_sa = string.Join(",", args.Select(x => AssureIsQuoted(x)));
            var o = AddValue(ShellEnvironmentVar.sp__ArgList, t_a);
            AddValue(ShellEnvironmentVar.argList, o.Value);
            o = AddValue(ShellEnvironmentVar.sp__ArgSepList, t_sa);
            AddValue(ShellEnvironmentVar.argSepList, o.Value);
            o = AddValue(ShellEnvironmentVar.sp__ArgsCount, args.Length);
            AddValue(ShellEnvironmentVar.argsCount, o.Value);
            int i = 0;
            foreach (var a in args)
            {
                switch (i)
                {
                    case 0:
                        o = AddValue(ShellEnvironmentVar.sp__arg0, args[i]);
                        AddValue(ShellEnvironmentVar.arg0, o.Value);
                        break;
                    case 1:
                        o = AddValue(ShellEnvironmentVar.sp__arg1, args[i]);
                        AddValue(ShellEnvironmentVar.arg1, o.Value);
                        break;
                    case 2:
                        o = AddValue(ShellEnvironmentVar.sp__arg2, args[i]);
                        AddValue(ShellEnvironmentVar.arg2, o.Value);
                        break;
                    case 3:
                        o = AddValue(ShellEnvironmentVar.sp__arg3, args[i]);
                        AddValue(ShellEnvironmentVar.arg3, o.Value);
                        break;
                    case 4:
                        o = AddValue(ShellEnvironmentVar.sp__arg4, args[i]);
                        AddValue(ShellEnvironmentVar.arg4, o.Value);
                        break;
                    case 5:
                        o = AddValue(ShellEnvironmentVar.sp__arg5, args[i]);
                        AddValue(ShellEnvironmentVar.arg5, o.Value);
                        break;
                    case 6:
                        o = AddValue(ShellEnvironmentVar.sp__arg6, args[i]);
                        AddValue(ShellEnvironmentVar.arg6, o.Value);
                        break;
                    case 7:
                        o = AddValue(ShellEnvironmentVar.sp__arg7, args[i]);
                        AddValue(ShellEnvironmentVar.arg7, o.Value);
                        break;
                    case 8:
                        o = AddValue(ShellEnvironmentVar.sp__arg8, args[i]);
                        AddValue(ShellEnvironmentVar.arg8, o.Value);
                        break;
                    case 9:
                        o = AddValue(ShellEnvironmentVar.sp__arg9, args[i]);
                        AddValue(ShellEnvironmentVar.arg9, o.Value);
                        break;
                }
                i++;
            }
            // empty args
            int j = 0;
            if (j >= i) AddValue(ShellEnvironmentVar.sp__arg0, ""); j++;
            if (j >= i) AddValue(ShellEnvironmentVar.sp__arg1, ""); j++;
            if (j >= i) AddValue(ShellEnvironmentVar.sp__arg2, ""); j++;
            if (j >= i) AddValue(ShellEnvironmentVar.sp__arg3, ""); j++;
            if (j >= i) AddValue(ShellEnvironmentVar.sp__arg4, ""); j++;
            if (j >= i) AddValue(ShellEnvironmentVar.sp__arg5, ""); j++;
            if (j >= i) AddValue(ShellEnvironmentVar.sp__arg6, ""); j++;
            if (j >= i) AddValue(ShellEnvironmentVar.sp__arg7, ""); j++;
            if (j >= i) AddValue(ShellEnvironmentVar.sp__arg8, ""); j++;
            if (j >= i) AddValue(ShellEnvironmentVar.sp__arg9, ""); j++;
        }

        #region operations on variables

        DataObject AddObject(ShellEnvironmentNamespace ns)
        {
            var path = Nsp(ns);
            var name = path.Split(CommandLineSyntax.VariableNamePathSeparator).Last();
            var obj = new DataObject(name);
            Vars.Set(path, obj);
            return obj;
        }

        public bool HasObject(ShellEnvironmentNamespace ns) => Vars.GetObject(Nsp(ns), out _, false);
        public bool HasValue(ShellEnvironmentVar ns) => Vars.GetObject(Nsp(ns), out var o, false) && o is DataValue;
        public bool HasObject(string varPath, string varName) => Vars.GetObject(Nsp(varPath, varName), out _, false);
        public bool HasValue(string varPath, string varName) => Vars.GetObject(Nsp(varPath, varName), out var o, false) && o is DataValue;
        public bool HasValue(string path) => Vars.GetObject(Nsp(path), out var o, false) && o is DataValue;

        public DataValue AddValue(ShellEnvironmentVar var, object value, bool readOnly = false)
        {
            var path = Nsp(var);
            var name = path.Split(CommandLineSyntax.VariableNamePathSeparator).Last();
            var val = new DataValue(name, value, readOnly);
            Vars.Set(path, val);
            return val;
        }

        public DataValue AddValue(ShellEnvironmentNamespace var, string subPath, object value, bool readOnly = false)
        {
            var path = Nsp(var, new string[] { subPath });
            var name = path.Split(CommandLineSyntax.VariableNamePathSeparator).Last();
            var val = new DataValue(name, value, readOnly);
            Vars.Set(path, val);
            return val;
        }

        public DataValue AddValue(ShellEnvironmentNamespace var, string subPath, string varName, object value, bool readOnly = false)
        {
            var path = Nsp(var, new string[] { subPath, varName });
            var name = path.Split(CommandLineSyntax.VariableNamePathSeparator).Last();
            var val = new DataValue(name, value, readOnly);
            Vars.Set(path, val);
            return val;
        }

        public DataValue AddNew(string varPath, string varName, object value, bool readOnly = false)
        {
            var path = Nsp(varPath, varName);
            var name = path.Split(CommandLineSyntax.VariableNamePathSeparator).Last();
            var val = new DataValue(name, value, readOnly);
            Vars.Set(path, val);
            return val;
        }

        public DataValue AddValue(string var, object value, bool readOnly = false)
        {
            var path = Nsp(var);
            var name = path.Split(CommandLineSyntax.VariableNamePathSeparator).Last();
            var val = new DataValue(name, value, readOnly);
            Vars.Set(path, val);
            return val;
        }

        public DataValue SetValue(ShellEnvironmentVar var, object value)
        {
            if (!HasValue(var))
                return AddValue(var, value);
            var o = GetDataValue(var);
            o.SetValue(value);
            return o;
        }

        public DataValue SetValue(string varPath, string varName, object value)
        {
            var path = Nsp(varPath, varName);
            if (!HasValue(path))
                return AddValue(path, value);
            var o = GetDataValue(path);
            o.SetValue(value);
            return o;
        }


        #region getters

        public bool Get(ShellEnvironmentVar var, out object value, bool throwException = true)
        {
            var path = Nsp(var);
            return Vars.Get(path, out value, throwException);
        }

        public DataValue GetDataValue(ShellEnvironmentVar var, bool throwException = true)
        {
            var path = Nsp(var);
            return Vars.GetValue(path, throwException);
        }
        public DataValue GetDataValue(string path, bool throwException = true)
        {
            return Vars.GetValue(path, throwException);
        }
        public T GetValue<T>(ShellEnvironmentVar var, bool throwException = true) => (T)GetDataValue(var, throwException).Value;
        public T GetValue<T>(string varPath, string varName, bool throwException = true) => (T)GetDataValue(Nsp(varPath, varName), throwException).Value;

        public bool IsOptionSetted(ShellEnvironmentVar @var) => GetValue<bool>(@var, false);
        public bool IsOptionSetted(string Namespace, string varName) => Vars.GetValue<bool>(Nsp(Namespace, varName));

        #endregion

        static string Nsp(string @namespace, string key) => ToAbsNsp(@namespace + CommandLineSyntax.VariableNamePathSeparator + key);
        static string Nsp(params string[] key) => ToAbsNsp(string.Join(CommandLineSyntax.VariableNamePathSeparator, key));
        static string Nsp(ShellEnvironmentNamespace @namespace, params string[] key) => ToAbsNsp(ToNsp(@namespace) + (key.Length == 0 ? "" : (CommandLineSyntax.VariableNamePathSeparator + "") + string.Join(CommandLineSyntax.VariableNamePathSeparator, key)));
        static string Nsp(ShellEnvironmentVar @var, params string[] key) =>

        ToAbsNsp(ToNsp(@var) + (key.Length == 0 ? "" : (CommandLineSyntax.VariableNamePathSeparator + "") + string.Join(CommandLineSyntax.VariableNamePathSeparator, key)));

        static string ToNsp(ShellEnvironmentVar @var) => ToNsp(ToStr(@var));

        /// <summary>
        /// 💥constraint: postfix (here __) same in ShellEnvironmentVar
        /// </summary>
        public const string SPECIAL_VAR_DECL_PREFIX = "sp__";
        public const string SPECIAL_VAR_IMPL_PREFIX = "sp_";

        static string ToStr(ShellEnvironmentVar var)
        {
            var v = var + "";
            var isSpec = IsSpecialVar(v);
            if (isSpec)
            {
                var nst = v.Replace(SPECIAL_VAR_DECL_PREFIX, "").Replace(SPECIAL_VAR_IMPL_PREFIX, "").Split("_").ToList();
                if (nst.Count > 0) nst.RemoveAt(nst.Count - 1);
                var nspfx = string.Join("_", nst);
                if (!string.IsNullOrEmpty(nspfx)) nspfx += "_";
                return $"{SPECIAL_VAR_DECL_PREFIX}{nspfx}{(char)var}";
            }
            return v;
        }

        static bool IsSpecialVar(string s) => s.StartsWith(SPECIAL_VAR_IMPL_PREFIX) || s.StartsWith(SPECIAL_VAR_DECL_PREFIX);

        static string ToNsp(ShellEnvironmentNamespace @namespace) => ToNsp(@namespace + "");
        static string ToNsp(string shellVar)
        {
            var s = (shellVar + "").Replace("__", "¤");
            if (s.Length != 1 || s != "_")
                s = s.Replace("_", CommandLineSyntax.VariableNamePathSeparator + "");
            return s.Replace("¤", "_");
        }

        static string ToAbsNsp(string @namespace)
        {
            var isSpecialVar = IsSpecialVar(@namespace);
            return Variables.Nsp(
                ("" + (isSpecialVar ? VariableNamespace._ : VariableNamespace.env)),
                isSpecialVar ? @namespace[SPECIAL_VAR_IMPL_PREFIX.Length..] : @namespace
                );
        }

        #endregion
    }
}
