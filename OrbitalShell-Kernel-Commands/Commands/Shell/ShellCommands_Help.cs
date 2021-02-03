using OrbitalShell.Component.CommandLine;
using OrbitalShell.Component.CommandLine.CommandModel;
using OrbitalShell.Component.CommandLine.Parsing;
using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Console;
using OrbitalShell.Lib;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using cons = OrbitalShell.DotNetConsole;
using static OrbitalShell.Component.EchoDirective.Shortcuts;
using OrbitalShell.Component.EchoDirective;
using System.Collections.Immutable;

namespace OrbitalShell.Commands.Shell
{
    /// <summary>
    /// ShellCommands_Help.cs
    /// </summary>
    public partial class ShellCommands
    {
        [Command("print help about commands, namespaces, commands declaring types and modules")]
        [CommandNamespace(CommandNamespace.shell, CommandNamespace.help)]
        public CommandVoidResult Help(
            CommandEvaluationContext context,
            [Option("s", "short", "short display: decrase output details")] bool shortView,
            [Option("v", "verbose", "set verbose view: increase output details")] bool verboseView,
            [Option("l", "all", "list all commands")] bool all,
            [Option("n", "namespace", "filter commands list by namespace. if t is * list namespaces", true, true)] string @namespace,
            [Option("t", "type", "filter commands list by command declaring type. if t is * list declaring types", true, true)] string type,
            [Option("m", "module", "filter commands list by module name. if m is * list modules", true, true)] string module,
            [Parameter("output help for the command having the name 'commandName'", true)] string commandName
            )
        {
            var hascn = !string.IsNullOrWhiteSpace(commandName);
            var list = !all && !hascn;
            var cmds = context.CommandLineProcessor.ModuleManager.ModuleCommandManager.AllCommands.AsQueryable();
            var namespaces = cmds.Select(x => x.Namespace).Distinct().ToList();
            namespaces.Sort();

            if (hascn)
                cmds = cmds.Where(x => x.Name.Equals(commandName, CommandLineParser.SyntaxMatchingRule));

            if (cmds.Count() > 0)
            {
                #region filter on declaring type
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
                #endregion

                #region filter on module
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
                #endregion

                #region filter on namespace
                if (cmds.Count() > 0 && !string.IsNullOrWhiteSpace(@namespace))
                {
                    if (@namespace != "*" && !namespaces.Contains(@namespace))
                    {
                        context.Errorln($"unknown command namespace: '{@namespace}'");
                        return new CommandVoidResult(ReturnCode.Error);
                    }

                    shortView = !verboseView;

                    if (@namespace != "*")
                        cmds = cmds.Where(x => x.Namespace == @namespace);
                    else
                    {
                        foreach (var ns in namespaces)
                            context.Out.Echoln(Darkcyan + ns);
                        return new CommandVoidResult();
                    }
                }
                #endregion

                var ncmds = cmds.ToList();
                ncmds.Sort(new Comparison<CommandSpecification>((x, y) => x.Name.CompareTo(y.Name)));
                cmds = ncmds.AsQueryable();
                if (cmds.Count() > 0)
                {
                    var maxcmdlength = cmds.Select(x => x.Name.Length).Max() + 1;
                    var maxcmdtypelength = cmds.Select(x => x.DeclaringTypeShortName.Length).Max() + 1;
                    var maxmodlength = cmds.Select(x => Path.GetFileNameWithoutExtension(x.MethodInfo.DeclaringType.Assembly.Location).Length).Max() + 1;
                    var maxnslength = cmds.Select(x => x.Namespace.Length).Max() + 1;

                    if (list) shortView = !verboseView;

                    var groupByNs = list;
                    if (groupByNs)
                    {
                        // get all fs
                        var cmdByNs = cmds.GroupBy((x) => x.Namespace).ToList(); // ordered
                        cmdByNs.Sort((x, y) => x.Key.CompareTo(y.Key));
                        int g = 0;
                        foreach (var grouping in cmdByNs)
                        {
                            if (grouping.Count() > 0)
                            {
                                if (g > 0) context.Out.Echoln();
                                context.Out.Echoln($"{ANSI.SGR_Underline}{context.ShellEnv.Colors.Label}{grouping.Key}{ANSI.SGR_UnderlineOff}{context.ShellEnv.Colors.Default}{ANSI.CRLF}");
                                foreach (var item in grouping)
                                {
                                    _PrintCommandHelp(context, item, shortView, verboseView, list, maxnslength, maxcmdlength, maxcmdtypelength, maxmodlength, !string.IsNullOrWhiteSpace(commandName));
                                }
                                g++;
                            }
                        }
                    }
                    else
                    {
                        int n = 0;
                        foreach (var cmd in cmds)
                        {
                            if (!list && n > 0) context.Out.Echoln();
                            _PrintCommandHelp(context, cmd, shortView, verboseView, list, maxnslength, maxcmdlength, maxcmdtypelength, maxmodlength, !string.IsNullOrWhiteSpace(commandName));
                            n++;
                        }
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

        void _PrintCommandHelp(
            CommandEvaluationContext context,
            CommandSpecification com,
            bool shortView = false,
            bool verboseView = false,
            bool list = false,
            int maxnslength = -1,
            int maxcnamelength = -1,
            int maxcmdtypelength = -1,
            int maxmodlength = -1,
            bool singleout = false
            )
        {
#pragma warning disable IDE0071 // Simplifier l’interpolation
#pragma warning disable IDE0071WithoutSuggestion // Simplifier l’interpolation
            if (maxcnamelength == -1) maxcnamelength = com.Name.Length + 1;
            if (maxnslength == -1) maxnslength = com.Namespace.Length + 1;
            if (maxcmdtypelength == -1) maxcmdtypelength = com.DeclaringTypeShortName.Length + 1;
            var col = singleout ? "" : "".PadRight(maxcnamelength, ' ');
            var f = GetCmd(EchoDirectives.f + "", cons.DefaultForeground.ToString().ToLower());
            if (list)
            {
                if (!shortView)
                    context.Out.Echoln($"{Darkcyan}{com.ModuleName.PadRight(maxmodlength, ' ')}   {com.DeclaringTypeShortName.PadRight(maxcmdtypelength, ' ')}   {com.Namespace.PadRight(maxnslength, ' ')}{Tab}{context.ShellEnv.Colors.Highlight}{com.Name.PadRight(maxcnamelength, ' ')}{Tab}{f}{com.Description}{context.ShellEnv.Colors.Default}");
                else
                    context.Out.Echoln($"{context.ShellEnv.Colors.Highlight}{com.Name.PadRight(maxcnamelength, ' ')}{f}{Tab}{com.Description}{context.ShellEnv.Colors.Default}");
            }
            else
            {
                bool hasrtt = com.ReturnType != null;
                bool hasalias = com.Aliases != null;
                if (singleout)
                {
                    context.Out.Echoln(com.Description);
                    context.Out.Echo($"{Br}{col}{context.ShellEnv.Colors.Label}syntax: {f}{com.ToColorizedString(context.ShellEnv.Colors)}", hasrtt | hasalias);
                    if (hasrtt) context.Out.Echoln($"{col}{context.ShellEnv.Colors.Label}returns: {context.ShellEnv.Colors.TypeName}{com.ReturnType.UnmangledName()}");
                    if (hasalias) context.Out.Echoln($"{col}{context.ShellEnv.Colors.Label}aliases: {context.ShellEnv.Colors.TypeName}{string.Join(",", com.Aliases.Select(x => x.name))}");
                    context.Out.Echo(!shortView ? Br : "");
                    if (!string.IsNullOrWhiteSpace(com.LongDescription)) context.Out.Echoln(_GetPrintableDocText(com.LongDescription, list, shortView, 0));
                }
                else
                {
                    context.Out.Echoln($"{com.Name.PadRight(maxcnamelength, ' ')}{com.Description}");
                    context.Out.Echoln($"{Br}{col}{context.ShellEnv.Colors.Label}syntax: {f}{com.ToColorizedString(context.ShellEnv.Colors)}");
                    if (hasrtt) context.Out.Echoln($"{col}{context.ShellEnv.Colors.Label}returns: {context.ShellEnv.Colors.TypeName}{com.ReturnType.UnmangledName()}");
                    if (hasalias) context.Out.Echoln($"{col}{context.ShellEnv.Colors.Label}aliases: {context.ShellEnv.Colors.TypeName}{string.Join(",", com.Aliases.Select(x => x.name))}");
                    context.Out.Echo(!shortView ? Br : "");
                    if (!string.IsNullOrWhiteSpace(com.LongDescription)) context.Out.Echo(_GetPrintableDocText(com.LongDescription + "(br)", list, shortView, maxcnamelength));
                }
            }

            if (!list)
            {
                if (com.ParametersCount > 0)
                {
                    if (!shortView)
                    {
                        var mpl = (com.ParametersCount > 0 ? com.ParametersSpecifications.Values.Select(x => x.Dump(false).Length).Max() : 0) + cons.TabLength;
                        foreach (var p in com.ParametersSpecifications.Values)
                        {
                            var ptype = (!p.IsOption && p.HasValue) ? $"of type: {context.ShellEnv.Colors.TypeName}{p.ParameterInfo.ParameterType.UnmangledName()}{f}" : "";
                            var pdef = (p.HasValue && p.IsOptional && p.HasDefaultValue && p.DefaultValue != null &&
                                    (!p.IsOption || p.ParameterValueTypeName != typeof(bool).Name)) ?
                                        ((ptype != "" ? ". " : "") + $"default value: {context.ShellEnv.Colors.OptionValue}{EchoPrimitives.DumpAsText(context, p.DefaultValue)}{f}")
                                        : "";

                            var pleftCol = "".PadRight(mpl - p.Dump(false).Length, ' ');
                            var leftCol = "".PadRight(mpl, ' ');

                            if (p.ParameterInfo.ParameterType.IsEnum)
                                pdef += $"(br){col}{Tab}{leftCol}possibles values: {context.ShellEnv.Colors.OptionValue}{string.Join(CommandLineSyntax.ParameterTypeListValuesSeparator, Enum.GetNames(p.ParameterInfo.ParameterType))}(rdc)";

                            var supdef = $"{ptype}{pdef}";
                            // method 'Echo if has' else to string
                            context.Out.Echoln($"{col}{Tab}{p.ToColorizedString(context.ShellEnv.Colors, false)}{pleftCol}{p.Description}");
                            if (!string.IsNullOrWhiteSpace(supdef)) context.Out.Echoln($"{col}{Tab}{" ".PadRight(mpl)}{supdef}");
                        }

                        //if (string.IsNullOrWhiteSpace(com.Documentation)) context.Out.Echoln();
                        if (!string.IsNullOrWhiteSpace(com.Documentation))
                            context.Out.Echo(_GetPrintableDocText("(br)" + com.Documentation, list, shortView, singleout ? 0 : maxcnamelength));

                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(com.Documentation))
                            context.Out.Echoln(_GetPrintableDocText("(br)" + com.Documentation, list, shortView, singleout ? 0 : maxcnamelength));
                    }
                }
                if (verboseView)
                {
                    if (com.ParametersCount > 0) context.Out.Echoln("");
                    context.Out.Echoln($"{col}{context.ShellEnv.Colors.Label}namespace       : {context.ShellEnv.Colors.HalfDarkLabel}{com.Namespace}");
                    context.Out.Echoln($"{col}{context.ShellEnv.Colors.Label}declaring type  : {context.ShellEnv.Colors.HalfDarkLabel}{com.DeclaringTypeShortName}");
                    context.Out.Echoln($"{col}{context.ShellEnv.Colors.Label}module          : {context.ShellEnv.Colors.HalfDarkLabel}{com.ModuleName}{context.ShellEnv.Colors.Default}");
                }
            }
#pragma warning restore IDE0071WithoutSuggestion // Simplifier l’interpolation
#pragma warning restore IDE0071 // Simplifier l’interpolation
        }

        string _GetPrintableDocText(string docText, bool list, bool shortView, int leftMarginSize)
        {
            if (string.IsNullOrWhiteSpace(docText) || shortView || list) return "";
            var lineStart = Environment.NewLine;
            var prfx0 = "{]=);:_&é'(";
            var prfx1 = "$*^ùè-_à'";
            docText = docText.Replace(lineStart, prfx0 + prfx1);
            var lst = docText.Split(prfx0).AsQueryable();
            if (string.IsNullOrWhiteSpace(lst.FirstOrDefault())) lst = lst.Skip(1);
            lst = lst.Select(x => "".PadRight(leftMarginSize, ' ') + x + Br);
            return /*Br +*/ string.Join("", lst).Replace(prfx1, "");
        }

    }
}