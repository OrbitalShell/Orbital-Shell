using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;

using OrbitalShell.Component.CommandLine.CommandModel;
using OrbitalShell.Component.CommandLine.Parsing;
using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Console;
using OrbitalShell.Component.EchoDirective;
using OrbitalShell.Component.Shell;
using OrbitalShell.Lib;
using OrbitalShell.Lib.Data;

using static OrbitalShell.Component.EchoDirective.Shortcuts;

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
            [Option("s", "short", "short display: decrease output details")] bool shortView,
            [Option("v", "verbose", "set verbose view: increase output details")] bool verboseView,
            [Option("a", "all", "list all commands")] bool all,
            [Option("n", "namespace", "filter commands list by namespace value or wildcard or regex (wildcard have ?,* , regex starts with \\)", true, true)] PatternString @namespace,
            [Option("t", "type", "filter commands list on command declaring type name value or wildcard or regex", true, true)] PatternString type,
            [Option("m", "module", "filter commands list by module name value or wildcard or regex ", true, true)] PatternString module,
            [Option("l", "list", "if one of -n|-t|-m is set, output list of filtered values")] bool listFilter,
            [Parameter("output help for the command having the name 'commandName'", true)] string commandName
            )
        {
            var hascn = !string.IsNullOrWhiteSpace(commandName);
            var list = !all && !hascn;
            var cmds = context.CommandLineProcessor.ModuleManager.ModuleCommandManager.AllCommands.AsQueryable();
            var namespaces = cmds.Select(x => x.Namespace).Distinct().ToList();
            namespaces.Sort();
            bool ignoreCase = true;

            if (hascn)
                cmds = cmds.Where(x => x.Name.Equals(commandName, CommandLineParser.SyntaxMatchingRule));

            if (cmds.Count() > 0)
            {
                #region filter on declaring type

                if (!string.IsNullOrWhiteSpace(type))
                {
                    var typenames = context.CommandLineProcessor.ModuleManager.ModuleCommandManager.CommandDeclaringTypesAssemblyQualifiedNames.ToList();
                    var typelst = typenames
                        .Select(x => Type.GetType(x))
                        .Where(x => x != null && type.Match(x.Name))
                        .ToList();

                    typelst.Sort((x, y) => x.Name.CompareTo(y.Name));

                    shortView = !verboseView;

                    if (!listFilter)
                        cmds = cmds.Where(x => type.Match(x.DeclaringTypeShortName, ignoreCase));
                    else
                    {
                        var sfx = "Commands";
                        string TypeName(Type type)
                        {
                            var s = shortView ? type.Name : type.FullName;
                            if (shortView && s.EndsWith(sfx))
                                s = s.Substring(0, s.Length - sfx.Length);
                            return s;
                        }
                        if (typelst.Count > 0)
                        {
                            var maxtl = typelst.Select(x => TypeName(x).Length).Max();

                            foreach (var typ in typelst)
                            {
                                var cmdattr = typ.GetCustomAttribute<CommandsAttribute>();
                                context.Out.Echoln(Darkcyan + TypeName(typ).PadRight(maxtl) + Tab + DefaultForegroundCmd + cmdattr.Description);
                            }
                        }
                        return new CommandVoidResult();
                    }
                }

                #endregion

                #region filter on module

                if (cmds.Count() > 0 && !string.IsNullOrWhiteSpace(module))
                {
                    var mods = context.CommandLineProcessor.ModuleManager.Modules;
                    var modnames = mods.Values.Where(x => module.Match(x.Name)).Select(x => x.Name).ToList();
                    modnames.Sort();
                    shortView = !verboseView;

                    if (!listFilter)
                        cmds = cmds.Where(x => module.Match(x.ModuleName, ignoreCase));
                    else
                    {
                        if (modnames.Count > 0)
                        {
                            var maxml = modnames.Select(x => x.Length).Max();
                            foreach (var modname in modnames)
                                context.Out.Echoln(Darkcyan + modname.PadRight(maxml) + Tab + DefaultForegroundCmd + mods.Values.Where(x => x.Name == modname).First().Description);
                        }
                        return new CommandVoidResult();
                    }
                }

                #endregion

                #region filter on namespace

                if (cmds.Count() > 0 && !string.IsNullOrWhiteSpace(@namespace))
                {
                    var nslst = namespaces.Where(x => @namespace.Match(x));
                    shortView = !verboseView;

                    if (!listFilter)
                        cmds = cmds.Where(x => @namespace.Match(x.Namespace, ignoreCase));
                    else
                    {
                        foreach (var ns in nslst)
                            context.Out.Echoln(Darkcyan + ns);
                        return new CommandVoidResult();
                    }
                }

                #endregion

                var ncmds = cmds.ToList();
                ncmds.Sort(new Comparison<CommandSpecification>((x, y) => x.Name.CompareTo(y.Name)));
                cmds = ncmds.AsQueryable();
                if (cmds.Any())
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
                            if (grouping.Any())
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

            var f = GetCmd(EchoDirectives.f + "", context.CommandLineProcessor.Console.DefaultForeground.ToString().ToLower());

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
                        var mpl = (com.ParametersCount > 0 ? com.ParametersSpecifications.Values.Select(x => x.Dump(false).Length).Max() : 0) + context.CommandLineProcessor.Console.TabLength;
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
                            context.Out.Echoln($"{col}{Tab}{p.ToColorizedString(context.ShellEnv.Colors, false)}{pleftCol}{f}{p.Description}");
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
            docText = docText.Replace(lineStart, prfx0 + prfx1).Replace("\r", "");
            var lst = docText.Split(prfx0).AsQueryable();
            if (string.IsNullOrWhiteSpace(lst.FirstOrDefault())) lst = lst.Skip(1);
            lst = lst.Select(x => "".PadRight(leftMarginSize, ' ') + x + Br);
            return /*Br +*/ string.Join("", lst).Replace(prfx1, "");
        }

    }
}