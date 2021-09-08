using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

using OrbitalShell.Component.CommandLine.CommandModel;
using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Console;
using OrbitalShell.Component.Shell;
using OrbitalShell.Component.Shell.Data;
using OrbitalShell.Component.Shell.Variable;
using OrbitalShell.Lib;
using OrbitalShell.Lib.Sys;

using static OrbitalShell.Component.EchoDirective.Shortcuts;

namespace OrbitalShell.Commands.Shell
{
    /// <summary>
    /// ShellCommands_Var.cs
    /// </summary>
    public partial class ShellCommands
    {
        [Command("outputs a table of environment variables and values")]
        [CommandNamespace(CommandNamespace.shell, CommandNamespace.var)]
        public CommandResult<List<IDataObject>> Env(
            CommandEvaluationContext context,
            [Parameter(0, "variable namespace or value path below the 'Env' namespace. if specified and exists, output is built from this point, otherwise outputs all variables from env root", true)] string varPath,
            [Option("u", "unfold-namespace", "unfold namespaces")] bool unfoldNamespaces = false,
            [Option("o", "unfold-value", "unfold values of type object")] bool unfoldObjects = false,
            [Option("p", "parsed", "echo string values in parsed mode (ansi and directives). By default strings objects are represented by raw text")] bool parsed = false
            )
        {
            object obj;
            if (varPath == null)
                context.Variables.GetObject(VariableNamespace.env + "", out obj);
            else
                context.Variables.GetObject(VariableNamespace.env, varPath, out obj);

            var options = new TableFormattingOptions(context.ShellEnv.TableFormattingOptions)
            {
                UnfoldCategories = unfoldNamespaces,
                UnfoldItems = unfoldObjects,
                IsRawModeEnabled = !parsed
            };

            if (obj is DataValue value)
            {
                var lst = new List<IDataObject>() { value };
                var resultValue = new CommandResult<List<IDataObject>>(lst);
                var wrapper = new DataObject("");
                wrapper.Set(new string[] { "x" }, value);
                wrapper.Echo(new EchoEvaluationContext(context.Out, context, options));
                return resultValue;
            }
            else
            {
                if (obj is IDataObject envVars)
                {
                    var values = envVars.GetAttributes();
                    envVars.Echo(new EchoEvaluationContext(context.Out, context, options));
                    return new CommandResult<List<IDataObject>>(values);
                }
                else
                {
                    // directly dump object members
                    EchoPrimitives.DumpObject(obj, new EchoEvaluationContext(context.Out, context, options));
                    return new CommandResult<List<IDataObject>>(null);
                }
            }
        }

        [Command("outputs a table of variables and values")]
        [CommandNamespace(CommandNamespace.shell, CommandNamespace.var)]
        [CommandAlias("loc", "vars local")]
        [CommandAlias("glob", "vars global")]
        [CommandAlias("settings", "env settings")]
        public CommandResult<List<IDataObject>> Var(
            CommandEvaluationContext context,
            [Parameter(0, "variable namespace or value path below the root namespace. if specified and exists, output is built from this point, otherwise outputs all variables from env root", true)] string varPath,
            [Option("u", "unfold-namespace", "unfold namespaces")] bool unfoldNamespaces = false,
            [Option("o", "unfold-value", "unfold values of type object")] bool unfoldObjects = false,
            [Option("p", "parse", "echo string values in parsed mode (ansi and directives). By default strings objects are represented by raw text")] bool parsed = false
            )
        {
            object obj;
            if (varPath == null)
                obj = context.Variables.RootObject;
            else
                context.Variables.GetObject(varPath, out obj);

            var options = new TableFormattingOptions(context.ShellEnv.TableFormattingOptions)
            {
                UnfoldCategories = unfoldNamespaces,
                UnfoldItems = unfoldObjects,
                IsRawModeEnabled = !parsed
            };

            if (obj is DataValue value)
            {
                var lst = new List<IDataObject>() { value };
                var resultValue = new CommandResult<List<IDataObject>>(lst);
                var wrapper = new DataObject("");
                wrapper.Set(new string[] { "x" }, value);
                wrapper.Echo(new EchoEvaluationContext(context.Out, context, options));
                return resultValue;
            }
            else
            {
                if (obj is IDataObject envVars)
                {
                    var values = envVars.GetAttributes();
                    envVars.Echo(new EchoEvaluationContext(context.Out, context, options));
                    return new CommandResult<List<IDataObject>>(values);
                }
                else
                {
                    // directly dump object members
                    EchoPrimitives.DumpObject(obj, new EchoEvaluationContext(context.Out, context, options));
                    return new CommandResult<List<IDataObject>>(null);
                }
            }
        }

        [Command("set a command alias if a name and a value is provided. If only the name is provided, clear the alias definition. it no parameters is specified, list all alias")]
        [CommandAlias("al", "alias")]
        public CommandResult<List<string>> Alias(
            CommandEvaluationContext context,
            [Parameter(0, "name of the alias", true)] string name,
            [Parameter(1, "text of the alias", true)][OptionRequireParameter("name")] string text,
            [Option("s", "save", "save current aliases to user aliases file")] bool save
            )
        {
            var r = new List<string>();
            if (name == null)
            {
                foreach (var kvp in context.CommandLineProcessor.CommandsAlias.Aliases)
                    context.Out.Echoln(CommandsAlias.BuildAliasCommand(kvp.Key, kvp.Value));
            }
            else
            {
                if (string.IsNullOrWhiteSpace(text))
                    context.CommandLineProcessor.CommandsAlias.UnsetAlias(context, name);
                else
                    context.CommandLineProcessor.CommandsAlias.AddOrReplaceAlias(context, name, text);
            }
            if (save)
                context.CommandLineProcessor.CommandsAlias.SaveAliases(context);
            return new CommandResult<List<string>>(r);
        }

        [Command("outputs informations about a variable")]
        [CommandNamespace(CommandNamespace.shell, CommandNamespace.var)]
        public CommandResult<IDataObject> Inf(
                    CommandEvaluationContext context,
                    [Parameter(0, "variable namespace of a value")] string varPath
               )
        {
            context.Variables.GetObject(varPath, out var obj);

            var options = new TableFormattingOptions(context.ShellEnv.TableFormattingOptions)
            {
                UnfoldCategories = false,
                UnfoldItems = false,
                IsRawModeEnabled = true
            };
            var props = new Dictionary<string, object>();

            if (obj is DataObject o)
            {
                props.Add("name", o.Name);
                props.Add("is read only", o.IsReadOnly);
                props.Add("namespace", o.ObjectPath);
                props.Add("has attributes", o.HasAttributes);
            }
            else
            if (obj is DataValue v)
            {
                props.Add("name", v.Name);
                props.Add("type", v.ValueType?.UnmangledName(false));
                props.Add("is read only", v.IsReadOnly);
                props.Add("has attributes", v.HasAttributes);
                props.Add("has value", v.HasValue);
                props.Add("namespace", v.ObjectPath);
                props.Add("value", v.Value);
            }
            else
                throw new Exception($"can't get information for a variable member");

            Table dt = new Table();
            dt.AddColumns("property", "value")
                .SetFormat("property", $"{context.ShellEnv.Colors.Label}{{0}}{Rdc}");
            dt.Columns[0].DataType = typeof(string);
            dt.Columns[1].DataType = typeof(object);

            foreach (var kv in props)
            {
                var row = dt.NewRow();
                row["property"] = kv.Key;
                row["value"] = kv.Value;
                dt.Rows.Add(row);
            }

            dt.Echo(new EchoEvaluationContext(context.Out, context, options));

            return new CommandResult<IDataObject>(obj as IDataObject);
        }

        [Command("set the value of a shell variable, or display the name and values of shell variables")]
        [CommandNamespace(CommandNamespace.shell, CommandNamespace.var)]
        public CommandResult<object> Set(
            CommandEvaluationContext context,
            [Parameter(0, "variable name with or without namespace prefix", false)] string name,
            [Parameter(1, "value that must be assigned to the variable", false)] object value,
            [Parameter(2, "name of the object type to be used in order to convert the provided value (on first assign, will assign a type to the variable. After type is assigned, the value can't be assigned from another type). default type is object", true)] string typeLabel = null,
            [Option("r", "read-only", "for a new variable, set it read only")] bool readOnly = false
            )
        {
            if (!VariableSyntax.HasValidRootNamespace(name))
                name = Variables.Nsp(VariableNamespace.local, name);

            Type type = null;
            if (!string.IsNullOrWhiteSpace(typeLabel))
            {
                type = TypeBuilder.GetType(typeLabel);

                if (type == null)
                    throw new Exception(
                        $"type label not found: '{typeLabel}'. possible values are: {string.Join(",", TypeBuilder.GetTypeLabels().Keys)}{ANSI.CRLF}or any actual .net type fullname case sensitive");
            }

            context.Variables.Set(name, value, readOnly, type);
            context.Variables.Get(name, out var @var, false);

            return new CommandResult<object>(@var);
        }

        [Command("get a shell variable. The variable is stored in the variables env.$lastComResult and _./")]
        [CommandNamespace(CommandNamespace.shell, CommandNamespace.var)]
        public CommandResult<object> Get(
            CommandEvaluationContext context,
            [Parameter(0, "variable name with or without namespace prefix", false)] string name
            //[Option("r", "read-only", "for a new variable, set it read only")] bool readOnly = false
            )
        {
            if (!VariableSyntax.HasValidRootNamespace(name))
                name = Variables.Nsp(VariableNamespace.local, name);

            context.Variables.Get(name, out var @var, false);

            return new CommandResult<object>(@var);
        }

        [Command("unset the value of shell variables. can not unset namespace, only variables")]
        [CommandNamespace(CommandNamespace.shell, CommandNamespace.var)]
        public CommandResult<object> Unset(
            CommandEvaluationContext context,
            [Parameter(0, "variable name with or without namespace prefix", false)] string name
            )
        {
            if (!VariableSyntax.HasValidRootNamespace(name))
                name = Variables.Nsp(VariableNamespace.local, name);

            context.Variables.Get(name, out var @var, true);
            if (@var is IDataObject)
                context.Variables.Unset(name);
            else
                throw new Exception($"can't unset a variable member: '{name}'");

            return new CommandResult<object>(@var);
        }

    }
}
