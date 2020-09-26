using DotNetConsoleAppToolkit.Component.CommandLine.Data;
using DotNetConsoleAppToolkit.Component.CommandLine.Parsing;
using DotNetConsoleAppToolkit.Component.CommandLine.Processor;
using DotNetConsoleAppToolkit.Console;
using DotNetConsoleAppToolkit.Lib.FileSystem;
using System;
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

        /// <summary>
        /// standard shell environment namespaces
        /// </summary>
        public enum ShellEnvironmentNamespace
        {            
            CommandsSettings,
            Debug,
            Display,
            Display_Colors,
        }

        /// <summary>
        /// standard shell environment namespaces
        /// </summary>
        public enum ShellEnvironmentVar
        {
            Debug_Pipeline,
            Display_TableSettings,
            OrbshPath,
            UserPath,
        }

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
                var key = Nsp((ShellEnvironmentNamespace)shellNs);
            }
            // data values
            AddValue(ShellEnvironmentVar.Debug_Pipeline,true);
            AddValue(ShellEnvironmentVar.Display_TableSettings, new TableFormattingOptions());
            AddValue(ShellEnvironmentVar.OrbshPath, new DirectoryPath(Assembly.GetExecutingAssembly().Location));
            //AddValue(ShellEnvironmentVar.)
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

        #endregion

        #region setters

        #endregion

        string Nsp(string @namespace, string key) => ToAbsNsp(@namespace + CommandLineSyntax.VariableNamePathSeparator + key);
        string Nsp(params string[] key) => ToAbsNsp(string.Join(CommandLineSyntax.VariableNamePathSeparator, key));
        string Nsp(ShellEnvironmentNamespace @namespace, params string[] key) => ToAbsNsp(ToNsp(@namespace) + (key.Length==0?"": (CommandLineSyntax.VariableNamePathSeparator + "") + string.Join(CommandLineSyntax.VariableNamePathSeparator, key)));
        string Nsp(ShellEnvironmentVar @var, params string[] key) => ToAbsNsp(ToNsp(@var) + (key.Length==0?"": (CommandLineSyntax.VariableNamePathSeparator + "") + string.Join(CommandLineSyntax.VariableNamePathSeparator, key)));

        static string ToNsp(ShellEnvironmentVar @var) => ToNsp(@var + "");
        static string ToNsp(ShellEnvironmentNamespace @namespace) => ToNsp(@namespace + "");
        static string ToNsp(string shellVar) => (shellVar + "").Replace("_", CommandLineSyntax.VariableNamePathSeparator+"" );
        string ToAbsNsp(string @namespace) => Variables.Nsp(VariableNamespace.Env, Name , @namespace);
    }
}
