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

        public TableFormattingOptions TableFormattingOptions => GetValue<TableFormattingOptions>(ShellEnvironmentVar.Display_TableSettings);
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
            {
                AddObject((ShellEnvironmentNamespace)shellNs);
            }
            
            // data values
            AddValue(ShellEnvironmentVar.Debug_Pipeline,false);
            
            AddValue(ShellEnvironmentVar.OrbshPath, new DirectoryPath(Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location )));
            AddValue(ShellEnvironmentVar.UserPath, new DirectoryPath(Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location )));
            
            AddValue(ShellEnvironmentVar.Display_FileSystemPathFormattingOptions, new FileSystemPathFormattingOptions());
            AddValue(ShellEnvironmentVar.Display_TableSettings, new TableFormattingOptions());

            // bash vars for compat
            AddValue(ShellEnvironmentVar.SHELL, GetValue(ShellEnvironmentVar.OrbshPath).Value );
            AddValue(ShellEnvironmentVar.SHELL__VERSION, context.CommandLineProcessor.Settings.AppVersion );
            AddValue(ShellEnvironmentVar.SHELL__NAME, context.CommandLineProcessor.Settings.AppName );
            AddValue(ShellEnvironmentVar.SHELL__LONG__NAME, context.CommandLineProcessor.Settings.AppLongName );
            AddValue(ShellEnvironmentVar.SHELL__EDITOR, context.CommandLineProcessor.Settings.AppEditor );
            AddValue(ShellEnvironmentVar.SHELL__LICENSE, context.CommandLineProcessor.Settings.AppLicense );

            AddValue(ShellEnvironmentVar.HOME, GetValue(ShellEnvironmentVar.UserPath).Value);
            AddValue(ShellEnvironmentVar.PS1, "");      
            AddValue(ShellEnvironmentVar.PS2, "");
            AddValue(ShellEnvironmentVar.PS3, "");
            AddValue(ShellEnvironmentVar.PS4, "");
        }

        DataObject AddObject(ShellEnvironmentNamespace ns)
        {
            var path = Nsp(ns);
            var name = path.Split(CommandLineSyntax.VariableNamePathSeparator).Last();
            var obj = new DataObject(name);
            Vars.Set(path, obj);
            return obj;
        }

        DataValue AddValue(ShellEnvironmentVar var,object value)
        {
            var path = Nsp(var);
            var name = path.Split(CommandLineSyntax.VariableNamePathSeparator).Last();
            var val = new DataValue(name, value);
            Vars.Set(path, value);
            return val;
        }

        #region getters

        public bool Get( ShellEnvironmentVar var,out object value, bool throwException = true)
        {
            var path = Nsp(var);
            return Vars.Get(path, out value, throwException);
        }

        public DataValue GetValue(ShellEnvironmentVar var, bool throwException = true)
        {
            var path = Nsp(var);
            return Vars.GetValue(path,throwException);
        }

        public T GetValue<T>(ShellEnvironmentVar var) => (T)GetValue(var).Value;

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
