using DotNetConsoleAppToolkit.Component.CommandLine.Data;
using DotNetConsoleAppToolkit.Component.CommandLine.Parsing;
using DotNetConsoleAppToolkit.Component.CommandLine.Processor;
using DotNetConsoleAppToolkit.Console;
using DotNetConsoleAppToolkit.Lib.FileSystem;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

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
            
            // data values
            AddValue(ShellEnvironmentVar.debug_pipeline,false);
            AddValue(ShellEnvironmentVar.userProfile, new DirectoryPath(context.CommandLineProcessor.Settings.AppDataFolderPath),true );
            
            AddValue(ShellEnvironmentVar.display_fileSystemPathFormattingOptions, new FileSystemPathFormattingOptions());
            AddValue(ShellEnvironmentVar.display_tableFormattingOptions, new TableFormattingOptions());
            var colorSettingsDV = AddValue(ShellEnvironmentVar.display_colors_colorSettings, new ColorSettings());
            Colors = (ColorSettings)colorSettingsDV.Value;

            // bash vars for compat
            AddValue(ShellEnvironmentVar.shell, new DirectoryPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)), true);
            AddValue(ShellEnvironmentVar.SHELL__VERSION, context.CommandLineProcessor.Settings.AppVersion , true );
            AddValue(ShellEnvironmentVar.SHELL__NAME, context.CommandLineProcessor.Settings.AppName , true);
            AddValue(ShellEnvironmentVar.SHELL__LONG__NAME, context.CommandLineProcessor.Settings.AppLongName, true );
            AddValue(ShellEnvironmentVar.SHELL__EDITOR, context.CommandLineProcessor.Settings.AppEditor, true );
            AddValue(ShellEnvironmentVar.SHELL__LICENSE, context.CommandLineProcessor.Settings.AppLicense, true );

            AddValue(ShellEnvironmentVar.home, new DirectoryPath(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)) , true);
            AddValue(ShellEnvironmentVar.PS1, "");      
            AddValue(ShellEnvironmentVar.PS2, "");
            AddValue(ShellEnvironmentVar.PS3, "");
            AddValue(ShellEnvironmentVar.PS4, "");

            // shell settings

            AddValue(ShellEnvironmentVar.settings_consoleInitialWindowWidth,-1);
            AddValue(ShellEnvironmentVar.settings_consoleInitialWindowHeight,-1);
            AddValue(ShellEnvironmentVar.settings_enableConsoleCompatibilityMode,false);
            AddValue(ShellEnvironmentVar.settings_enableAvoidEndOfLineFilledWithBackgroundColor,true);

            // @TODO: override settings from a config file .json (do also for CommandLineProcessorSettings)
        }

        DataObject AddObject(ShellEnvironmentNamespace ns)
        {
            var path = Nsp(ns);
            var name = path.Split(CommandLineSyntax.VariableNamePathSeparator).Last();
            var obj = new DataObject(name);
            Vars.Set(path, obj);
            return obj;
        }

        bool HasObject(ShellEnvironmentNamespace ns) => Vars.GetObject(Nsp(ns),out _,false);

        DataValue AddValue(ShellEnvironmentVar var,object value,bool readOnly=false)
        {
            var path = Nsp(var);
            var name = path.Split(CommandLineSyntax.VariableNamePathSeparator).Last();
            var val = new DataValue(name, value, readOnly );
            Vars.Set(path, val);
            return val;
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

        #region setters

        #endregion

        string Nsp(string @namespace, string key) => ToAbsNsp(@namespace + CommandLineSyntax.VariableNamePathSeparator + key);
        string Nsp(params string[] key) => ToAbsNsp(string.Join(CommandLineSyntax.VariableNamePathSeparator, key));
        string Nsp(ShellEnvironmentNamespace @namespace, params string[] key) => ToAbsNsp(ToNsp(@namespace) + (key.Length==0?"": (CommandLineSyntax.VariableNamePathSeparator + "") + string.Join(CommandLineSyntax.VariableNamePathSeparator, key)));
        string Nsp(ShellEnvironmentVar @var, params string[] key) => ToAbsNsp(ToNsp(@var) + (key.Length==0?"": (CommandLineSyntax.VariableNamePathSeparator + "") + string.Join(CommandLineSyntax.VariableNamePathSeparator, key)));

        static string ToNsp(ShellEnvironmentVar @var) => ToNsp(@var + "");
        static string ToNsp(ShellEnvironmentNamespace @namespace) => ToNsp(@namespace + "");
        static string ToNsp(string shellVar)
        {
            var s = (shellVar + "").Replace("__", "¤");
            s = s.Replace("_", CommandLineSyntax.VariableNamePathSeparator + "");
            return s.Replace("¤", "_");
        }
        string ToAbsNsp(string @namespace) => Variables.Nsp(VariableNamespace.env, /*Name ,*/ @namespace);
    }
}
