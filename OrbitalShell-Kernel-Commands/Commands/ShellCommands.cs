using System.ComponentModel.Design.Serialization;
using System.Threading;
using OrbitalShell.Component.CommandLine;
using OrbitalShell.Component.CommandLine.CommandModel;
using OrbitalShell.Component.CommandLine.Data;
using OrbitalShell.Component.CommandLine.Parsing;
using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.CommandLine.Variable;
using OrbitalShell.Component.CommandLine.Module;
using OrbitalShell.Console;
using OrbitalShell.Lib;
using OrbitalShell.Lib.FileSystem;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using static OrbitalShell.Lib.Str;
using cons = OrbitalShell.DotNetConsole;
using static OrbitalShell.Component.EchoDirective.Shortcuts;
using OrbitalShell.Component.EchoDirective;

namespace OrbitalShell.Component.Commands
{
    [Commands("commands of the command line processor")]
    public class ShellCommands : ICommandsDeclaringType
    {
        #region shell exec

        [Command("runs a batch file [Experimental]")]
        public CommandVoidResult Batch(
            CommandEvaluationContext context,
            [Parameter(0, "path of the batch file (attempt a text file, starting or not by #orbsh!)")] FilePath path
            )
        {
            if (path.CheckExists(context))
            {
                context.CommandLineProcessor.CommandBatchProcessor.RunBatch(context, path.FileSystemInfo.FullName);
            }
            return CommandVoidResult.Instance;
        }

        #endregion

        #region help

        [Command("print help about commands,commands types and modules")]
        public CommandVoidResult Help(
            CommandEvaluationContext context,
            [Option("s", "set short view: decrase output details")] bool shortView,
            [Option("v", "set verbose view: increase output details")] bool verboseView,
            [Option("all", "list all commands")] bool all,
            [Option("t", "filter commands list by command declaring type. if t is * list types", true, true)] string type,
            [Option("m", "filter commands list by module name. if m is * list modules", true, true)] string module,
            [Parameter("output help for the command with name 'commandName'", true)] string commandName
            )
        {
            var hascn = !string.IsNullOrWhiteSpace(commandName);
            var list = !all && !hascn;
            var cmds = context.CommandLineProcessor.ModuleManager.ModuleCommandManager.AllCommands.AsQueryable();
            if (hascn)
                cmds = cmds.Where(x => x.Name.Equals(commandName, CommandLineParser.SyntaxMatchingRule));

            if (cmds.Count() > 0)
            {
                if (!string.IsNullOrWhiteSpace(type))
                {
                    if (type != "*" && !context.CommandLineProcessor.ModuleManager.ModuleCommandManager.CommandDeclaringShortTypesNames.Contains(type))
                    {
                        context.Errorln($"unknown command declaring type: '{type}'");
                        return new CommandVoidResult(ReturnCode.Error);
                    }

                    shortView = !verboseView;

                    if (type != "*")
                        cmds = cmds.Where(x => x.DeclaringTypeShortName == type);
                    else
                    {
                        var typenames = context.CommandLineProcessor.ModuleManager.ModuleCommandManager.CommandDeclaringTypesNames.ToList();
                        var typelst = typenames.Select(x => Type.GetType(x)).ToList();
                        typelst.Sort((x, y) => x.Name.CompareTo(y.Name));

                        var sfx = "Commands";
                        string TypeName(Type type)
                        {
                            var s = shortView ? type.Name : type.FullName;
                            if (shortView && s.EndsWith(sfx))
                                s = s.Substring(0, s.Length - sfx.Length);
                            return s;
                        }
                        var maxtl = typelst.Select(x => TypeName(x).Length).Max();

                        foreach (var typ in typelst)
                        {
                            var cmdattr = typ.GetCustomAttribute<CommandsAttribute>();
                            context.Out.Echoln(Darkcyan + TypeName(typ).PadRight(maxtl) + Tab + DefaultForegroundCmd + cmdattr.Description);
                        }
                        return new CommandVoidResult();
                    }
                }
                if (cmds.Count() > 0 && !string.IsNullOrWhiteSpace(module))
                {
                    if (module != "*" && !context.CommandLineProcessor.ModuleManager.Modules.Values.Select(x => x.Name).Contains(module))
                    {
                        context.Errorln($"unknown command module: '{module}'");
                        return new CommandVoidResult(ReturnCode.Error);
                    }

                    shortView = !verboseView;

                    if (module != "*")
                        cmds = cmds.Where(x => x.ModuleName == module);
                    else
                    {
                        var mods = context.CommandLineProcessor.ModuleManager.Modules;
                        var modnames = mods.Values.Select(x => x.Name).ToList();
                        modnames.Sort();
                        var maxml = modnames.Select(x => x.Length).Max();
                        foreach (var modname in modnames)
                            context.Out.Echoln(Darkcyan + modname.PadRight(maxml) + Tab + DefaultForegroundCmd + mods[modname].Description);
                        return new CommandVoidResult();
                    }
                }
                var ncmds = cmds.ToList();
                ncmds.Sort(new Comparison<CommandSpecification>((x, y) => x.Name.CompareTo(y.Name)));
                cmds = ncmds.AsQueryable();
                if (cmds.Count() > 0)
                {
                    var maxcmdlength = cmds.Select(x => x.Name.Length).Max() + 1;
                    var maxcmdtypelength = cmds.Select(x => x.DeclaringTypeShortName.Length).Max() + 1;
                    var maxmodlength = cmds.Select(x => Path.GetFileNameWithoutExtension(x.MethodInfo.DeclaringType.Assembly.Location).Length).Max() + 1;
                    int n = 0;

                    if (list) shortView = !verboseView;

                    foreach (var cmd in cmds)
                    {
                        if (!list && n > 0) context.Out.Echoln();
                        PrintCommandHelp(context, cmd, shortView, verboseView, list, maxcmdlength, maxcmdtypelength, maxmodlength, !string.IsNullOrWhiteSpace(commandName));
                        n++;
                    }
                }
            }
            else
            {
                context.Errorln($"Command not found: '{commandName}'");
                return new CommandVoidResult(ReturnCode.Error);
            }
            return new CommandVoidResult();
        }

        void PrintCommandHelp(
            CommandEvaluationContext context,
            CommandSpecification com,
            bool shortView = false,
            bool verboseView = false,
            bool list = false,
            int maxcnamelength = -1,
            int maxcmdtypelength = -1,
            int maxmodlength = -1,
            bool singleout = false)
        {
#pragma warning disable IDE0071 // Simplifier l’interpolation
#pragma warning disable IDE0071WithoutSuggestion // Simplifier l’interpolation
            if (maxcnamelength == -1) maxcnamelength = com.Name.Length + 1;
            if (maxcmdtypelength == -1) maxcmdtypelength = com.DeclaringTypeShortName.Length + 1;
            var col = singleout ? "" : "".PadRight(maxcnamelength, ' ');
            var f = GetCmd(EchoDirectives.f + "", cons.DefaultForeground.ToString().ToLower());
            if (list)
            {
                if (!shortView)
                    context.Out.Echoln($"{Darkcyan}{com.ModuleName.PadRight(maxmodlength, ' ')}   {com.DeclaringTypeShortName.PadRight(maxcmdtypelength, ' ')}{Tab}{context.ShellEnv.Colors.Highlight}{com.Name.PadRight(maxcnamelength, ' ')}{Tab}{f}{com.Description}{context.ShellEnv.Colors.Default}");
                else
                    context.Out.Echoln($"{context.ShellEnv.Colors.Highlight}{com.Name.PadRight(maxcnamelength, ' ')}{f}{Tab}{com.Description}{context.ShellEnv.Colors.Default}");
            }
            else
            {
                if (singleout)
                {
                    context.Out.Echoln(com.Description);
                    if (com.ParametersCount > 0) context.Out.Echo($"{Br}{col}{context.ShellEnv.Colors.Label}syntax: {f}{com.ToColorizedString(context.ShellEnv.Colors)}{(!shortView ? Br : "")}");
                    context.Out.Echoln(GetPrintableDocText(com.LongDescription, list, shortView, 0));
                }
                else
                {
                    context.Out.Echoln($"{com.Name.PadRight(maxcnamelength, ' ')}{com.Description}");
                    if (com.ParametersCount > 0) context.Out.Echo($"{Br}{col}{context.ShellEnv.Colors.Label}syntax: {f}{com.ToColorizedString(context.ShellEnv.Colors)}{(!shortView ? Br : "")}");
                    context.Out.Echo(GetPrintableDocText(com.LongDescription, list, shortView, maxcnamelength));
                }
            }

            if (!list)
            {
                if (com.ParametersCount > 0)
                {
                    if (!shortView)
                    {
                        var mpl = com.ParametersSpecifications.Values.Select(x => x.Dump(false).Length).Max() + cons.TabLength;
                        foreach (var p in com.ParametersSpecifications.Values)
                        {
                            var ptype = (!p.IsOption && p.HasValue) ? $"of type: {Darkyellow}{p.ParameterInfo.ParameterType.Name}{f}" : "";
                            var pdef = (p.HasValue && p.IsOptional && p.HasDefaultValue && p.DefaultValue != null && (!p.IsOption || p.ParameterValueTypeName != typeof(bool).Name)) ? ((ptype != "" ? ". " : "") + $"default value: {Darkyellow}{EchoPrimitives.DumpAsText(context, p.DefaultValue)}{f}") : "";
                            var supdef = $"{ptype}{pdef}";
                            // method 'Echo if has' else to string (with stream capture ?)
                            context.Out.Echoln($"{col}{Tab}{p.ToColorizedString(context.ShellEnv.Colors, false)}{"".PadRight(mpl - p.Dump(false).Length, ' ')}{p.Description}");
                            if (!string.IsNullOrWhiteSpace(supdef)) context.Out.Echoln($"{col}{Tab}{" ".PadRight(mpl)}{supdef}");
                        }

                        if (string.IsNullOrWhiteSpace(com.Documentation)) context.Out.Echoln();
                        context.Out.Echo(GetPrintableDocText(com.Documentation, list, shortView, singleout ? 0 : maxcnamelength));

                    }
                    else
                    {
                        context.Out.Echoln(GetPrintableDocText(com.Documentation, list, shortView, singleout ? 0 : maxcnamelength));
                    }
                }
                if (verboseView)
                {
                    context.Out.Echoln($"{col}{context.ShellEnv.Colors.Label}type  : {context.ShellEnv.Colors.HalfDarkLabel}{com.DeclaringTypeShortName}");
                    context.Out.Echoln($"{col}{context.ShellEnv.Colors.Label}module: {context.ShellEnv.Colors.HalfDarkLabel}{com.ModuleName}{context.ShellEnv.Colors.Default}");
                }
            }
#pragma warning restore IDE0071WithoutSuggestion // Simplifier l’interpolation
#pragma warning restore IDE0071 // Simplifier l’interpolation
        }

        string GetPrintableDocText(string docText, bool list, bool shortView, int leftMarginSize)
        {
            if (string.IsNullOrWhiteSpace(docText) || shortView || list) return "";
            var lineStart = Environment.NewLine;
            var prfx0 = "{]=);:_&é'(";
            var prfx1 = "$*^ùè-_à'";
            docText = docText.Replace(lineStart, prfx0 + prfx1);
            var lst = docText.Split(prfx0).AsQueryable();
            if (string.IsNullOrWhiteSpace(lst.FirstOrDefault())) lst = lst.Skip(1);
            lst = lst.Select(x => "".PadRight(leftMarginSize, ' ') + x + Br);
            return Br + string.Join("", lst).Replace(prfx1, "");
        }

        #endregion

        #region modules

        [Command("list modules if no option specified, else load or unload modules")]
        public CommandResult<List<ModuleSpecification>> Module(
            CommandEvaluationContext context,
            [Option("l", "load a module from the given path", true, true)] FilePath loadModulePath = null,
            [Option("u", "unload the module having the given name ", true, true)] string unloadModuleName = null
            )
        {
            var f = context.ShellEnv.Colors.Default.ToString();
            if (loadModulePath == null && unloadModuleName == null)
            {
                var col1length = context.CommandLineProcessor.ModuleManager.Modules.Values.Select(x => x.Name.Length).Max() + 1;
                int n=1;
                foreach (var kvp in context.CommandLineProcessor.ModuleManager.Modules)
                {
                    context.Out.Echoln($"{Darkcyan}{kvp.Value.Name.PadRight(col1length, ' ')}{f}{kvp.Value.Description}");
                    context.Out.Echoln($"{"".PadRight(col1length, ' ')}{kvp.Value.Info.GetDescriptor(context)}");
                    context.Out.Echoln($"{"".PadRight(col1length, ' ')}{context.ShellEnv.Colors.Label}assembly:{context.ShellEnv.Colors.HalfDark}{kvp.Value.Assembly.FullName}");
                    context.Out.Echoln($"{"".PadRight(col1length, ' ')}{context.ShellEnv.Colors.Label}path:    {context.ShellEnv.Colors.HalfDark}{kvp.Value.Assembly.Location}");
                    if (n<context.CommandLineProcessor.ModuleManager.Modules.Count) context.Out.Echoln();
                    n++;
                }
                return new CommandResult<List<ModuleSpecification>>(context.CommandLineProcessor.ModuleManager.Modules.Values.ToList());
            }
            if (loadModulePath != null)
            {
                if (loadModulePath.CheckExists(context))
                {
                    var a = Assembly.LoadFrom(loadModulePath.FileSystemInfo.FullName);
                    var moduleSpecification = context.CommandLineProcessor.ModuleManager.RegisterModule(context, a);
                    context.Out.Echoln($"loaded: {moduleSpecification.Info.GetDescriptor(context)}");
                }
                else
                    return new CommandResult<List<ModuleSpecification>>(ReturnCode.Error);
            }

            if (unloadModuleName != null)
            {
                if (context.CommandLineProcessor.ModuleManager.Modules.Values.Any(x => x.Name == unloadModuleName))
                {
                    var moduleSpecification = context.CommandLineProcessor.ModuleManager.UnregisterModule(context, unloadModuleName);
                    context.Out.Echoln($"unloaded: {moduleSpecification.Info.GetDescriptor(context)}");
                }
                else
                {
                    context.Errorln($"module '{unloadModuleName}' is not registered");
                    return new CommandResult<List<ModuleSpecification>>(ReturnCode.Error);
                }
            }
            return new CommandResult<List<ModuleSpecification>>();
        }

        #endregion

        #region variables

        [Command("outputs a table of environment variables and values")]
        public CommandResult<List<IDataObject>> Env(
            CommandEvaluationContext context,
            [Parameter(0, "variable namespace or value path below the 'Env' namespace. if specified and exists, output is built from this point, otherwise outputs all variables from env root", true)] string varPath,
            [Option("u", "unfold namespaces")] bool unfoldNamespaces = false,
            [Option("o", "unfold values of type object")] bool unfoldObjects = false,
            [Option("p", "echo string values in parsed mode (ansi and directives)")] bool parsed = false
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
        public CommandResult<List<IDataObject>> Vars(
            CommandEvaluationContext context,
            [Parameter(0, "variable namespace or value path below the root namespace. if specified and exists, output is built from this point, otherwise outputs all variables from env root", true)] string varPath,
            [Option("u", "unfold namespaces")] bool unfoldNamespaces = false,
            [Option("o", "unfold values of type object")] bool unfoldObjects = false,
            [Option("p", "echo string values in parsed mode (ansi and directives)")] bool parsed = false
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
        public CommandResult<List<string>> Alias(
            CommandEvaluationContext context,
            [Parameter(0, "name of the alias", true)] string name,
            [Parameter(1, "text of the alias", true)][OptionRequireParameter("name")] string text,
            [Option("s", "save current aliases to user aliases file")] bool save
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

        [Command("set the value of a shell variable, or display the name and values of shell variables")]
        public CommandResult<IDataObject> Set(
            CommandEvaluationContext context
            )
        {
            var vars = context.Variables.GetDataValues();
            return new CommandResult<IDataObject>();
        }

        [Command("unset the value of shell variables")]
        public CommandResult<IDataObject> Unset(
            CommandEvaluationContext context
            )
        {
            var vars = context.Variables.GetDataValues();
            return new CommandResult<IDataObject>();
        }

        #endregion

        #region command line

        [Command("set the command line prompt")]
        public CommandResult<string> Prompt(
            CommandEvaluationContext context,
            [Parameter("outputs the text of command line prompt if it is specified, else outputs the current prompt text", true)] string prompt = null
            )
        {
            context.CommandLineProcessor.AssertCommandLineProcessorHasACommandLineReader();
            if (prompt == null)
            {
                prompt = context.CommandLineProcessor.CommandLineReader.GetPrompt();
                context.Out.Echoln(prompt, true);
            }
            else
                context.CommandLineProcessor.CommandLineReader.SetPrompt(context, prompt);
            return new CommandResult<string>(prompt);
        }

        #endregion

        #region app

        [Command("exit the shell")]
        public CommandResult<int> Exit(
            CommandEvaluationContext context
            )
        {
            cons.Exit();
            return new CommandResult<int>(0);
        }

        [Command("print command processor infos")]
        public CommandVoidResult Cpinfo(
            CommandEvaluationContext context
            )
        {
            context.CommandLineProcessor.PrintInfo(context);
            return new CommandVoidResult();
        }

        #endregion

        #region history

        [Command("displays the commands history list or manipulate it")]
        [SuppressMessage("Style", "IDE0071WithoutSuggestion:Simplifier l’interpolation", Justification = "<En attente>")]
        [SuppressMessage("Style", "IDE0071:Simplifier l’interpolation", Justification = "<En attente>")]
        public CommandVoidResult History(
            CommandEvaluationContext context,
            [Option("i", "invoke the command at the entry number in the history list", true, true)] int num,
            [Option("c", "clear the loaded history list")] bool clear,
            [Option("w", "write history lines to the history file (content of the file is replaced)")]
            [OptionRequireParameter("file")]  bool writeToFile,
            [Option("a", "append history lines to the history file")]
            [OptionRequireParameter("file")]  bool appendToFile,
            [Option("r","read the history file and append the content to the history list")]
            [OptionRequireParameter("file")]  bool readFromFile,
            [Option("n","read the history file and append the content not already in the history list to the history list")]
            [OptionRequireParameter("file")] bool appendFromFile,
            [Parameter(1, "file", true)] FilePath file
            )
        {
            var hist = context.CommandLineProcessor.CmdsHistory.History;
            var max = hist.Count().ToString().Length;
            int i = 1;
            var f = DefaultForegroundCmd;

            if (num > 0)
            {
                if (num < 1 || num > hist.Count)
                {
                    context.Errorln($"history entry number out of range (1..{hist.Count})");
                    return new CommandVoidResult(ReturnCode.Error);
                }
                var h = hist[num - 1];
                context.CommandLineProcessor.CommandLineReader.SendNextInput(h);
                return new CommandVoidResult();
            }

            if (clear)
            {
                context.CommandLineProcessor.CmdsHistory.ClearHistory();
                return new CommandVoidResult();
            }

            if (appendToFile || readFromFile || appendFromFile || writeToFile)
            {
                file ??= context.CommandLineProcessor.CmdsHistory.FilePath;
                if (file.CheckPathExists(context))
                {
                    if (writeToFile)
                    {
                        File.Delete(context.CommandLineProcessor.CmdsHistory.FilePath.FullName);
                        File.AppendAllLines(file.FullName, hist);
                    }
                    if (appendToFile) File.AppendAllLines(file.FullName, hist);
                    if (readFromFile)
                    {
                        var lines = File.ReadAllLines(file.FullName);
                        foreach (var line in lines) context.CommandLineProcessor.CmdsHistory.HistoryAppend(line);
                        context.CommandLineProcessor.CmdsHistory.HistorySetIndex(-1, false);
                    }
                    if (appendFromFile)
                    {
                        var lines = File.ReadAllLines(file.FullName);
                        foreach (var line in lines) if (!context.CommandLineProcessor.CmdsHistory.HistoryContains(line)) context.CommandLineProcessor.CmdsHistory.HistoryAppend(line);
                        context.CommandLineProcessor.CmdsHistory.HistorySetIndex(-1, false);
                    }
                }
                return new CommandVoidResult();
            }

            foreach (var h in hist)
            {
                if (context.CommandLineProcessor.CancellationTokenSource.IsCancellationRequested)
                    break;
                var hp = $"  {context.ShellEnv.Colors.Numeric}{i.ToString().PadRight(max + 2, ' ')}{f}";
                context.Out.Echo(hp);
                context.Out.ConsolePrint(h, true);
                i++;
            }
            return new CommandVoidResult();
        }

        [Command("repeat the previous command if there is one, else does nothing")]
        [CommandName("!!")]
        public CommandResult<string> HistoryPreviousCommand(
            CommandEvaluationContext context
            )
        {
            var lastCmd = context.CommandLineProcessor.CmdsHistory.History.LastOrDefault();
            context.CommandLineProcessor.AssertCommandLineProcessorHasACommandLineReader();
            if (lastCmd != null) context.CommandLineProcessor.CommandLineReader.SendNextInput(lastCmd);
            return new CommandResult<string>(lastCmd);
        }

        [Command("repeat the command specified by absolute or relative line number in command history list")]
        [CommandName("!")]
        public CommandResult<string> HistoryPreviousCommand(
            CommandEvaluationContext context,
            [Parameter("line number in the command history list if positive, else current command minus n if negative (! -1 equivalent to !!)")] int n
            )
        {
            var h = context.CommandLineProcessor.CmdsHistory.History;
            var index = (n < 0) ? h.Count + n : n - 1;
            string lastCmd;
            if (index < 0 || index >= h.Count)
            {
                context.Errorln($"line number out of bounds of commands history list (1..{h.Count})");
                return new CommandResult<string>(ReturnCode.Error);
            }
            else
            {
                lastCmd = h[index];
                context.CommandLineProcessor.AssertCommandLineProcessorHasACommandLineReader();
                context.CommandLineProcessor.CommandLineReader.SendNextInput(lastCmd);
            }
            return new CommandResult<string>(lastCmd);
        }

        #endregion

        #region fixes

        [Command("enable console compatibility mode (try to fix common bugs on known consoles)")]
        public CommandVoidResult EnableConsoleCompatibilityMode(CommandEvaluationContext context)
        {
            var oFix = context.ShellEnv.GetDataValue(ShellEnvironmentVar.settings_console_enableCompatibilityMode);
            oFix.SetValue(true);

            var oWinWidth = context.ShellEnv.GetDataValue(ShellEnvironmentVar.settings_console_initialWindowWidth);
            var oWinHeight = context.ShellEnv.GetDataValue(ShellEnvironmentVar.settings_console_initialWindowHeight);

            oWinWidth.SetValue(2000);
            oWinHeight.SetValue(2000);

            var WinWidth = (int)oWinWidth.Value;
            var winHeight = (int)oWinHeight.Value;

            if (WinWidth > -1) System.Console.WindowWidth = WinWidth;
            if (winHeight > -1) System.Console.WindowHeight = winHeight;

            System.Console.Clear();
            //context.Out.Echo(ANSI.RIS);

            return CommandVoidResult.Instance;
        }

        #endregion
    }
}
