using DotNetConsoleAppToolkit.Component.CommandLine.Data;
using DotNetConsoleAppToolkit.Component.CommandLine.Parsing;
using DotNetConsoleAppToolkit.Component.CommandLine.Processor;
using System;

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
            Colors,
            CommandsSettings,
            Debug            
        }

        /// <summary>
        /// standard shell environment namespaces
        /// </summary>
        public enum ShellEnvironmentVar
        {
            Debug_Pipeline
        }

        public ShellEnvironment(string name) : base(name, false) { }

        public void Initialize(CommandEvaluationContext context)
        {
            Vars = context.Variables;
            // data objects
            foreach ( var shellNs in Enum.GetValues(typeof(ShellEnvironmentNamespace)) )
            {
                var key = Nsp((ShellEnvironmentNamespace)shellNs);
            }
            // data values
            foreach ( var shellEnvVar in Enum.GetValues(typeof(ShellEnvironmentVar)))
            {
                var key = Nsp((ShellEnvironmentVar)shellEnvVar);
            }
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
        string ToAbsNsp(string @namespace) => Variables.Nsp(VariableNamespace.Env) + Variables.Nsp( Name , @namespace);
    }
}
