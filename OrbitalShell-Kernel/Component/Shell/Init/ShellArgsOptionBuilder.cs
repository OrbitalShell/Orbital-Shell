using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using OrbitalShell.Component.CommandLine.Parsing.Value;
using OrbitalShell.Component.CommandLine.Parsing.Variable;
using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Parser.Json;
using OrbitalShell.Component.Shell.Data;
using OrbitalShell.Component.Shell.Variable;

namespace OrbitalShell.Component.Shell.Init
{
    /// <summary>
    /// shell arguments to options builder
    /// </summary>
    public class ShellArgsOptionBuilder : IShellArgsOptionBuilder
    {
        #region attributes

        /// <summary>
        /// name - value separator in --env {variableName}{ARG_ENV_NAME_VALUE_SEPARATOR}{variableValue}
        /// </summary>
        public static readonly string ARG_ENV_NAME_VALUE_SEPARATOR = ":";

        // known arguments

        /// <summary>
        /// set a shell environment variable
        /// </summary>
        public static readonly ShellArg ARG_ENV = new("e", "env");

        /// <summary>
        /// starts a non interactive shell (no prompt)
        /// </summary>
        public static readonly ShellArg ARG_NO_INTERACTIVE = new("e", "no-interact");

        /// <summary>
        /// indicates the shell is not attached to a console (stdin is not keyboard)
        /// </summary>
        public static readonly ShellArg ARG_NO_CONSOLE = new("r", "no-console");

        /// <summary>
        /// indicates the shell runs in quiet mode (no extra ouput except commands output & prompt [if interactive] )
        /// </summary>
        public static readonly ShellArg ARG_QUIET = new("q", "quiet");

        /// <summary>
        /// indicates the shell to run the command after shell init
        /// </summary>
        public static readonly ShellArg ARG_RUN_COMMAND = new("c", "com");

        /// <summary>
        /// indicates the shell to import settings from file
        /// </summary>
        public static readonly ShellArg ARG_IMPORT_SETTINGS = new("s", "settings");

        string[] _args;

        #endregion

        public ShellArgsOptionBuilder() { }

        #region operations

        /// <summary>
        /// provides arguments to be interpreted
        /// </summary>
        /// <param name="args"></param>
        public void SetArgs(string[] args)
        {
            _args = args;
        }

        /// <summary>
        /// indicates if a named argument is setted (short -{name} or long --{name})
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool HasArg(ShellArg arg) => _args.Contains(arg.ShortOpt) || _args.Contains(arg.LongOpt);

        /// <summary>
        /// returns values of an arg if it is provided, else null
        /// </summary>
        /// <param name="arg">searched arg</param>
        /// <returns>arg values or null</returns>
        public ShellArgValue GetArg(ShellArg arg)
        {
            var shortIdx = Array.IndexOf(_args, arg.ShortOpt);
            var longIdx = Array.IndexOf(_args, arg.LongOpt);
            var idx = Math.Max(shortIdx, longIdx);
            if (idx == -1) return null;
            return new ShellArgValue(
                arg,
                _args[idx], (idx + 1 < _args.Length) ? _args[idx + 1] : null)
            { ArgIndex = idx };
        }

        /// <summary>
        /// check if the arg spec match the arg name
        /// </summary>
        /// <param name="argSpec">arg spec</param>
        /// <param name="argName">arg name</param>
        /// <returns>true if both match, false otherwize</returns>
        public bool IsArg(ShellArg argSpec, string argName) =>
            argName.StartsWith(argSpec.ShortOpt) || argName.StartsWith(argSpec.LongOpt);

        /// <summary>
        /// import settings from JSON settings files. use arg if specified + use defaults settings
        /// </summary>
        /// <param name="context">command eval context</param>
        /// <param name="appliedArgs">args that have been takent into account</param>
        /// <returns>ShellArgsOptionBuilder</returns>
        public ShellArgsOptionBuilder ImportSettingsFromJSon(
            CommandEvaluationContext context,
            ref List<ShellArgValue> appliedArgs
            )
        {
            ImportSettingsFromJSonFile(context, context.CommandLineProcessor.Settings.ShellSettingsFilePath);
            ImportSettingsFromJSonFile(context, context.CommandLineProcessor.Settings.UserSettingsFilePath);

            ShellArgValue importSettings;
            if ((importSettings = GetArg(ARG_IMPORT_SETTINGS)) != null)
            {
                var settingsPath = importSettings.ArgValue
                    ?? throw new Exception($"--settings value is null");
                ImportSettingsFromJSonFile(context, settingsPath);
                appliedArgs.Add(importSettings);
            }

            return this;
        }

        public ShellArgsOptionBuilder ImportSettingsFromJSonFile(
            CommandEvaluationContext context,
            string path
            )
        {
            if (!File.Exists(path))
            {
                context.Logger.Log("skip import settings from json file: file not present: " + path);
                return this;
            }
            context.Logger.Log("import settings from json file: " + path);

            foreach (var (name, value) in new JsonEventParser()
                .ReadJSonObjectFromFile(path))
                SetVariableFromObjectValue(context, name, value);

            return this;
        }

        /// <summary>
        /// configure the command operation context from args
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public ShellArgsOptionBuilder SetCommandOperationContextOptions(
            CommandEvaluationContext context,
            ref List<ShellArgValue> appliedArgs
        )
        {
            bool CheckArg(ShellArg arg, ref List<ShellArgValue> aa)
            {
                var argVal = GetArg(arg);
                if (argVal == null) return false;
                aa.Add(argVal);
                return true;
            }

            context.Settings.HasConsole = !CheckArg(ARG_NO_CONSOLE, ref appliedArgs);
            context.Settings.IsInteractive = !CheckArg(ARG_NO_INTERACTIVE, ref appliedArgs);
            context.Settings.IsQuiet = CheckArg(ARG_QUIET, ref appliedArgs);

            return this;
        }

        /// <summary>
        /// set command line processor options (ShellBootstrap.ShellInit)
        /// <para>apply orbsh command args -env:{varName}={varValue}</para>
        /// </summary>
        /// <param name="context">command eval context</param>
        /// <param name="appliedArgs">args that have been takent into account</param>
        /// <returns>ShellArgsOptionBuilder</returns>
        public ShellArgsOptionBuilder SetCommandLineProcessorOptions(
            CommandEvaluationContext context,
            ref List<ShellArgValue> appliedArgs)
        {
            // parse and apply any [--env|-e]:{VarName}={VarValue} argument

            foreach (var arg in _args)
            {
                if (IsArg(ARG_ENV, arg))
                {
                    try
                    {
                        var t = arg.Split(':');
                        var t2 = t[1].Split('=');
                        if (t.Length == 2 && IsArg(ARG_ENV, t[0]) && t2.Length == 2)
                        {
                            SetShellEnvVariable(context, t2[0], t2[1]);
                            appliedArgs.Add(new ShellArgValue(ARG_ENV, t2[0] + "=" + t2[1]));
                        }
                        else
                            context.Errorln($"shell arg set error: syntax error: {arg}");
                    }
                    catch (Exception ex)
                    {
                        context.Errorln($"shell arg set option error: {arg} (error is: {ex.Message})");
                    }
                }
            }

            return this;
        }

        #endregion

        #region util

        /// <summary>
        /// set a typed variable from a string value<br/>
        /// don't set the value if conversion has failed
        /// </summary>
        /// <param name="name">name including namespace</param>
        /// <param name="value">value that must be converted to var type an assigned to the var</param>
        public ShellArgsOptionBuilder SetShellEnvVariable(
            CommandEvaluationContext context,
            string name,
            string value)
        {
            var tn = VariableSyntax.SplitPath(name);
            var t = new ArraySegment<string>(tn);
            if (context.ShellEnv.Get(t, out var o) && o is DataValue val)
            {
                if (ValueTextParser.ToTypedValue(value, val.ValueType, null, out var v, out _))
                    val.SetValue(v);
            }
            else
                context.Errorln($"variable not found: {name}");
            return this;
        }

        /// <summary>
        /// set a typed variable from an object (typed) value<br/>
        /// don't set the value if conversion has failed
        /// </summary>
        /// <param name="name">name including namespace</param>
        /// <param name="value">value that must be converted to var type an assigned to the var</param>
        public ShellArgsOptionBuilder SetVariableFromObjectValue(
            CommandEvaluationContext context,
            string name,
            object value)
        {
            if (context.Variables.GetValue(name, false) is DataValue val)
            {
                try
                {
                    val.SetValue(value);
                }
                catch (Exception ex)
                {
                    context.Errorln($"set variable error: ({ex.Message}): {name}={value}");
                }
            }
            else
                context.Errorln($"variable not found: {Variables.Nsp(VariableNamespace.env, context.ShellEnv.Name, name)}");
            return this;
        }

        #endregion
    }
}
