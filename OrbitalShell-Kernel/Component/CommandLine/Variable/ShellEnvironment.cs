using DotNetConsoleAppToolkit.Component.CommandLine.Data;
using DotNetConsoleAppToolkit.Component.CommandLine.Parsing;
using DotNetConsoleAppToolkit.Component.CommandLine.Processor;
using DotNetConsoleAppToolkit.Console;
using DotNetConsoleAppToolkit.Lib.FileSystem;
using DotNetConsoleAppToolkit.Lib.Sys;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using static DotNetConsoleAppToolkit.Lib.Str;

namespace DotNetConsoleAppToolkit.Component.CommandLine.Variable
{
    /// <summary>
    /// variables of the shell
    /// </summary>
    public class ShellEnvironment : DataObject
    {
        public Variables Vars { get; protected set; }
        public ColorSettings Colors { get; protected set; }

        public TableFormattingOptions TableFormattingOptions => GetValue<TableFormattingOptions>(ShellEnvironmentVar.display_tableFormattingOptions);
        public FileSystemPathFormattingOptions FileSystemPathFormattingOptions => GetValue<FileSystemPathFormattingOptions>(ShellEnvironmentVar.display_fileSystemPathFormattingOptions);

        public ShellEnvironment(string name) : base(name, false) { }

        /// <summary>
        /// creates the standard shell env with known namespaces and values names
        /// </summary>
        /// <param name="context"></param>
        public void Initialize(CommandEvaluationContext context)
        {
            Vars = context.Variables;

            // data objects
            foreach ( var shellNs in Enum.GetValues(typeof(ShellEnvironmentNamespace)) )
            {
                var ns = (ShellEnvironmentNamespace)shellNs;
                if (!HasObject(ns)) AddObject(ns);
            }
            
            // debug

            AddValue(ShellEnvironmentVar.debug_pipeline,false);

            // env

            // global settings

            AddValue(ShellEnvironmentVar.display_fileSystemPathFormattingOptions, new FileSystemPathFormattingOptions());
            AddValue(ShellEnvironmentVar.display_tableFormattingOptions, new TableFormattingOptions());
            var o = AddValue(ShellEnvironmentVar.display_colors_colorSettings, new ColorSettings());
            Colors = (ColorSettings)o.Value;

            // bash vars (extended)

            AddValue(ShellEnvironmentVar.shell, new DirectoryPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)), true);
            AddValue(ShellEnvironmentVar.SHELL__VERSION, context.CommandLineProcessor.Settings.AppVersion , true );
            AddValue(ShellEnvironmentVar.SHELL__NAME, context.CommandLineProcessor.Settings.AppName , true);
            AddValue(ShellEnvironmentVar.SHELL__LONG__NAME, context.CommandLineProcessor.Settings.AppLongName, true );
            AddValue(ShellEnvironmentVar.SHELL__EDITOR, context.CommandLineProcessor.Settings.AppEditor, true );
            AddValue(ShellEnvironmentVar.SHELL__LICENSE, context.CommandLineProcessor.Settings.AppLicense, true );
            AddValue(ShellEnvironmentVar.home, new DirectoryPath(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)) , true);
            AddValue(ShellEnvironmentVar.userProfile, new DirectoryPath(context.CommandLineProcessor.Settings.AppDataFolderPath),true );
            
            // shell settings (defaults)

            AddValue(ShellEnvironmentVar.settings_prompt, ANSI.RSTXTA+"> ");        // prompt   
            AddValue(ShellEnvironmentVar.settings_consoleInitialWindowWidth,-1);
            AddValue(ShellEnvironmentVar.settings_consoleInitialWindowHeight,-1);
            AddValue(ShellEnvironmentVar.settings_enableConsoleCompatibilityMode,false);
            AddValue(ShellEnvironmentVar.settings_enableAvoidEndOfLineFilledWithBackgroundColor,true);

            InitializeSpecialVars(context);

            InitializeArgsVars(context.CommandLineProcessor.Args);

            // @TODO: override settings from a config file .json (do also for CommandLineProcessorSettings)
        }

        /// <summary>
        /// init shell env special vars
        /// </summary>
        void InitializeSpecialVars(CommandEvaluationContext context) {
            var o = AddValue(ShellEnvironmentVar.sp__lastCommandReturnCode, ReturnCode.NotIdentified );
            AddValue(ShellEnvironmentVar.lastComReturnCode,o.Value );

            o = AddValue(ShellEnvironmentVar.sp__lastCommandErrorText, new StringWrapper() );
            AddValue(ShellEnvironmentVar.lastComErrorText,o.Value );
            
            o = AddValue(ShellEnvironmentVar.sp__activeShellPID, System.Diagnostics.Process.GetCurrentProcess().Id , true);            
            AddValue(ShellEnvironmentVar.activeShellPID,o.Value, true);

            o = AddValue(ShellEnvironmentVar.sp__activeShellThreadID, System.Threading.Thread.CurrentThread.ManagedThreadId , true);
            AddValue(ShellEnvironmentVar.activeShellThreadID,o.Value, true);

            o = AddValue(ShellEnvironmentVar.sp__lastTaskThreadID, -1 );            
            AddValue(ShellEnvironmentVar.lastTaskThreadID,o.Value );

            o = AddValue(ShellEnvironmentVar.sp__shellOpts, string.Join(" ",context.CommandLineProcessor.Args) , true);        
            AddValue(ShellEnvironmentVar.shellOpts,o.Value, true);

            o = AddValue(ShellEnvironmentVar.sp__activeShellContextID, context.ID );        
            AddValue(ShellEnvironmentVar.activeShellContextID,o.Value );
        }

        /// <summary>
        /// set shell vars for last command return info
        /// </summary>
        /// <param name="returnCode">code returned</param>
        /// <param name="errorText">eventually (default blank) text of error returned</param>
        public void UpdateVarLastCommandReturn(ReturnCode returnCode,string errorText="",Exception ex=null) {
            SetValue(ShellEnvironmentVar.sp__lastCommandReturnCode,returnCode);
            SetValue(ShellEnvironmentVar.lastComReturnCode,returnCode);
            var err = (ex!=null)?ex.Message:errorText;
            SetValue(ShellEnvironmentVar.sp__lastCommandErrorText,new StringWrapper(err,Colors.Error+""));
            SetValue(ShellEnvironmentVar.lastComErrorText,new StringWrapper(err,Colors.Error+""));
        }

        public void UpdateVarContext(int contextID) {
            SetValue(ShellEnvironmentVar.sp__activeShellContextID,contextID);
            SetValue(ShellEnvironmentVar.activeShellContextID,contextID);
        }

        /// <summary>
        /// initialize args env vars
        /// </summary>
        /// <param name="args">args</param>
        void InitializeArgsVars(string[] args) {
            var t_a = string.Join(" ",args);
            var t_sa = string.Join(",",args.Select(x => AssureIsQuoted(x)));
            var o = AddValue(ShellEnvironmentVar.sp__ArgList, t_a );
            AddValue(ShellEnvironmentVar.argList,o.Value);
            o = AddValue(ShellEnvironmentVar.sp__ArgSepList, t_sa );
            AddValue(ShellEnvironmentVar.argSepList,o.Value);
            o = AddValue(ShellEnvironmentVar.sp__ArgsCount, args.Length );
            AddValue(ShellEnvironmentVar.argsCount,o.Value);
            int i = 0;
            foreach ( var a in args ) {
                switch (i) {
                    case 0:
                        o = AddValue(ShellEnvironmentVar.sp__arg0, args[i] );
                        AddValue(ShellEnvironmentVar.arg0, o.Value);
                        break;
                    case 1:
                        o = AddValue(ShellEnvironmentVar.sp__arg1, args[i]);
                        AddValue(ShellEnvironmentVar.arg1, o.Value);
                        break;
                    case 2:
                        o = AddValue(ShellEnvironmentVar.sp__arg2, args[i] );
                        AddValue(ShellEnvironmentVar.arg2, o.Value);                    
                        break;
                    case 3:
                        o = AddValue(ShellEnvironmentVar.sp__arg3, args[i] );
                        AddValue(ShellEnvironmentVar.arg3, o.Value);        
                        break;                
                    case 4:
                        o = AddValue(ShellEnvironmentVar.sp__arg4, args[i] );
                        AddValue(ShellEnvironmentVar.arg4, o.Value);
                        break;
                    case 5:
                        o = AddValue(ShellEnvironmentVar.sp__arg5, args[i] );
                        AddValue(ShellEnvironmentVar.arg5, o.Value);
                        break;
                    case 6:
                        o = AddValue(ShellEnvironmentVar.sp__arg6, args[i] );
                        AddValue(ShellEnvironmentVar.arg6, o.Value);
                        break;
                    case 7:
                        o = AddValue(ShellEnvironmentVar.sp__arg7, args[i] );
                        AddValue(ShellEnvironmentVar.arg7, o.Value);
                        break;      
                    case 8:
                        o = AddValue(ShellEnvironmentVar.sp__arg8, args[i] );
                        AddValue(ShellEnvironmentVar.arg8, o.Value);
                        break;  
                    case 9:
                        o = AddValue(ShellEnvironmentVar.sp__arg9, args[i] );
                        AddValue(ShellEnvironmentVar.arg9, o.Value);
                        break;                                                                                                                                                                                            
                }
                i++;
            }
            // empty args
            int j = 0;
            if (j>=i) AddValue(ShellEnvironmentVar.sp__arg0, "" ); j++;       
            if (j>=i) AddValue(ShellEnvironmentVar.sp__arg1, "" ); j++;              
            if (j>=i) AddValue(ShellEnvironmentVar.sp__arg2, "" ); j++;             
            if (j>=i) AddValue(ShellEnvironmentVar.sp__arg3, "" ); j++;            
            if (j>=i) AddValue(ShellEnvironmentVar.sp__arg4, "" ); j++;       
            if (j>=i) AddValue(ShellEnvironmentVar.sp__arg5, "" ); j++;           
            if (j>=i) AddValue(ShellEnvironmentVar.sp__arg6, "" ); j++;          
            if (j>=i) AddValue(ShellEnvironmentVar.sp__arg7, "" ); j++;            
            if (j>=i) AddValue(ShellEnvironmentVar.sp__arg8, "" ); j++;            
            if (j>=i) AddValue(ShellEnvironmentVar.sp__arg9, "" ); j++;           
        }

        DataObject AddObject(ShellEnvironmentNamespace ns)
        {
            var path = Nsp(ns);
            var name = path.Split(CommandLineSyntax.VariableNamePathSeparator).Last();
            var obj = new DataObject(name);
            Vars.Set(path, obj);
            return obj;
        }

        public bool HasObject(ShellEnvironmentNamespace ns) => Vars.GetObject(Nsp(ns),out _,false);
        public bool HasValue(ShellEnvironmentVar ns) => Vars.GetObject(Nsp(ns),out var o,false) && o is DataValue;

        public DataValue AddValue(ShellEnvironmentVar var,object value,bool readOnly=false)
        {
            var path = Nsp(var);
            var name = path.Split(CommandLineSyntax.VariableNamePathSeparator).Last();
            var val = new DataValue(name, value, readOnly );
            Vars.Set(path, val);
            return val;
        }

        public DataValue AddValue(string var,object value,bool readOnly=false)
        {
            var path = Nsp(var);
            var name = path.Split(CommandLineSyntax.VariableNamePathSeparator).Last();
            var val = new DataValue(name, value, readOnly );
            Vars.Set(path, val);
            return val;
        }

        public DataValue SetValue(ShellEnvironmentVar var,object value) {
            var path = Nsp(var);
            var name = path.Split(CommandLineSyntax.VariableNamePathSeparator).Last();
            if (!HasValue(var))
                return AddValue(var,value);
           var o = GetDataValue(var);
           o.SetValue(value);
           return o;
        }

        #region getters

        public bool Get( ShellEnvironmentVar var,out object value, bool throwException = true)
        {
            var path = Nsp(var);
            return Vars.Get(path, out value, throwException);
        }

        public DataValue GetDataValue(ShellEnvironmentVar var, bool throwException = true)
        {
            var path = Nsp(var);
            return Vars.GetValue(path,throwException);
        }

        public T GetValue<T>(ShellEnvironmentVar var, bool throwException = true) => (T)GetDataValue(var, throwException).Value;

        public bool OptionSetted(ShellEnvironmentVar @var) => GetValue<bool>(@var,false);    

        #endregion

        string Nsp(string @namespace, string key) => ToAbsNsp(@namespace + CommandLineSyntax.VariableNamePathSeparator + key);
        string Nsp(params string[] key) => ToAbsNsp(string.Join(CommandLineSyntax.VariableNamePathSeparator, key));
        string Nsp(ShellEnvironmentNamespace @namespace, params string[] key) => ToAbsNsp(ToNsp(@namespace) + (key.Length==0?"": (CommandLineSyntax.VariableNamePathSeparator + "") + string.Join(CommandLineSyntax.VariableNamePathSeparator, key)));
        string Nsp(ShellEnvironmentVar @var, params string[] key) => 
        
            ToAbsNsp(ToNsp(@var) + (key.Length==0?"": (CommandLineSyntax.VariableNamePathSeparator + "") + string.Join(CommandLineSyntax.VariableNamePathSeparator, key)));

        static string ToNsp(ShellEnvironmentVar @var) => ToNsp(ToStr(@var));
   
        /// <summary>
        /// 💥 constraint: postfix (here __) same in ShellEnvironmentVar
        /// </summary>
        public const string SPECIAL_VAR_DECL_PREFIX = "sp__";
        public const string SPECIAL_VAR_IMPL_PREFIX = "sp_";

        static string ToStr(ShellEnvironmentVar var) {
            var v = var+"";
            var isSpec = IsSpecialVar(v);
            return (isSpec?$"{SPECIAL_VAR_DECL_PREFIX}{(char)var}":v);
        }

        static bool IsSpecialVar(string s) => s.StartsWith(SPECIAL_VAR_IMPL_PREFIX) || s.StartsWith(SPECIAL_VAR_DECL_PREFIX);

        static string ToNsp(ShellEnvironmentNamespace @namespace) => ToNsp(@namespace + "");
        static string ToNsp(string shellVar)
        {
            var s = (shellVar + "").Replace("__", "¤");
            if (s.Length!=1 || s!="_")
                s = s.Replace("_", CommandLineSyntax.VariableNamePathSeparator + "");
            return s.Replace("¤", "_");
        }
        string ToAbsNsp(string @namespace) {
            var isSpecialVar = IsSpecialVar(@namespace);
            return Variables.Nsp(
                ( ""+(isSpecialVar?VariableNamespace.local:VariableNamespace.env) ), 
                isSpecialVar?@namespace.Substring(SPECIAL_VAR_IMPL_PREFIX.Length):@namespace
                );
        }
    }
}
