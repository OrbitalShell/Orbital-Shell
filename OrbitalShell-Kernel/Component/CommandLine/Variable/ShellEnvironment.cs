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

        public TableFormattingOptions TableFormattingOptions => GetValue<TableFormattingOptions>(ShellEnvironmentVar.Display_TableFormattingOptions);
        public FileSystemPathFormattingOptions FileSystemPathFormattingOptions => GetValue<FileSystemPathFormattingOptions>(ShellEnvironmentVar.Display_FileSystemPathFormattingOptions);

        public ShellEnvironment(string name) : base(name, false) { }

        /// <summary>
        /// creates the standard shell env with known namespaces and values names
        /// </summary>
        /// <param name="context"></param>
        public void Initialize(CommandEvaluationContext context)
        {
            Vars = context.Variables;
            Vars.Set( Variables.Nsp(VariableNamespace.Env, Name ), this );

            // data objects
            foreach ( var shellNs in Enum.GetValues(typeof(ShellEnvironmentNamespace)) )
                AddObject((ShellEnvironmentNamespace)shellNs);
            
            // data values
            AddValue(ShellEnvironmentVar.Debug_Pipeline,false);
            AddValue(ShellEnvironmentVar.UserProfile, context.CommandLineProcessor.Settings.UserProfileFolder,true );
            
            AddValue(ShellEnvironmentVar.Display_FileSystemPathFormattingOptions, new FileSystemPathFormattingOptions());
            AddValue(ShellEnvironmentVar.Display_TableFormattingOptions, new TableFormattingOptions());
            var colorSettingsDV = AddValue(ShellEnvironmentVar.Display_Colors_ColorSettings, new ColorSettings());
            Colors = (ColorSettings)colorSettingsDV.Value;

            // bash vars for compat
            AddValue(ShellEnvironmentVar.SHELL, new DirectoryPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)), true);
            AddValue(ShellEnvironmentVar.SHELL__VERSION, context.CommandLineProcessor.Settings.AppVersion , true );
            AddValue(ShellEnvironmentVar.SHELL__NAME, context.CommandLineProcessor.Settings.AppName , true);
            AddValue(ShellEnvironmentVar.SHELL__LONG__NAME, context.CommandLineProcessor.Settings.AppLongName, true );
            AddValue(ShellEnvironmentVar.SHELL__EDITOR, context.CommandLineProcessor.Settings.AppEditor, true );
            AddValue(ShellEnvironmentVar.SHELL__LICENSE, context.CommandLineProcessor.Settings.AppLicense, true );

            AddValue(ShellEnvironmentVar.HOME, Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) , true);
            AddValue(ShellEnvironmentVar.PS1, "");      
            AddValue(ShellEnvironmentVar.PS2, "");
            AddValue(ShellEnvironmentVar.PS3, "");
            AddValue(ShellEnvironmentVar.PS4, "");

            // shell settings

            AddValue(ShellEnvironmentVar.Settings_ConsoleInitialWindowWidth,-1);
            AddValue(ShellEnvironmentVar.Settings_ConsoleInitialWindowHeight,-1);
            AddValue(ShellEnvironmentVar.Settings_EnableConsoleCompatibilityMode,false);
            AddValue(ShellEnvironmentVar.Settings_EnableAvoidEndOfLineFilledWithBackgroundColor,true);

            // @TODO: override settings from a config file .json (do also for CommandLineProcessorSettings)
            // @TODO: variables and their namespaces should be lower case
            // @TODO: commands and their namespaces should be lower case (use - in command name to replace upper cases)
        }

        DataObject AddObject(ShellEnvironmentNamespace ns)
        {
            var path = Nsp(ns);
            var name = path.Split(CommandLineSyntax.VariableNamePathSeparator).Last();
            var obj = new DataObject(name);
            Vars.Set(path, obj);
            return obj;
        }

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
        string ToAbsNsp(string @namespace) => Variables.Nsp(VariableNamespace.Env, Name , @namespace);
    }
}
