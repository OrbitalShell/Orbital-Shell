using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OrbitalShell.Component.CommandLine.Parsing;
using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Shell.Data;
using OrbitalShell.Component.Shell.Variable;

namespace OrbitalShell.Component.Shell
{
    /// <summary>
    /// shell arguments to options builder
    /// </summary>
    public class ShellArgsOptionBuilder
        // : IArgsOptionBuilder
    {
        /// <summary>
        /// name - value separator in --env {variableName}:{variableValue}
        /// </summary>
        public static readonly string ARG_ENV_NAME_VALUE_SEPARATOR = ":";

        // known arguments

        /// <summary>
        /// set a shell environment variable
        /// </summary>
        public static readonly (string, string) ARG_ENV = ("e", "env");

        /// <summary>
        /// starts a non interactive shell
        /// </summary>
        public static readonly (string, string) ARG_NO_INTERACTIVE = ("n", "no-interactive");

        /// <summary>
        /// indicates the shell is not attached to a console
        /// </summary>
        public static readonly (string, string) ARG_NO_CONSOLE = ("c", "no-console");

        string[] Args;

        public bool HasArg(string name) => Args.Contains("-" + name) || Args.Contains("--" + name);

        public ShellArgsOptionBuilder() { }

        public void SetArgs(string[] args)
        {
            Args = args;
        }

        /// <summary>
        /// set a typed variable from a string value<br/>
        /// don't set the value if conversion has failed
        /// </summary>
        /// <param name="name">name including namespace</param>
        /// <param name="value">value that must be converted to var type an assigned to the var</param>
        public ShellArgsOptionBuilder SetVariable(CommandEvaluationContext context, string name, string value)
        {
            var tn = VariableSyntax.SplitPath(name);
            var t = new ArraySegment<string>(tn);
            if (context.ShellEnv.Get(t, out var o) && o is DataValue val)
            {
                if (ValueTextParser.ToTypedValue(value, val.ValueType, null, out var v, out _))
                    val.SetValue(v);
            }
            else
                context.CommandLineProcessor.Error($"variable not found: {Variables.Nsp(VariableNamespace.env, context.ShellEnv.Name, name)}", true);
            return this;
        }
    } 
}
