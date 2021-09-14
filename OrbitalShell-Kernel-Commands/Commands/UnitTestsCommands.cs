using System;
using System.Collections.Generic;

using OrbitalShell.Commands.FileSystem;
using OrbitalShell.Component.CommandLine.CommandModel;
using OrbitalShell.Component.CommandLine.CommandModel.Attributes;
using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Shell;

namespace OrbitalShell.Commands.Dev
{
    [Commands("for shell unit tests purposes")]
    [CommandsNamespace(CommandNamespace.shell, CommandNamespace.test)]
    public class UnitTestsCommands : ICommandsDeclaringType
    {
        [Command("command crash test (21/1/21)")]
        [CommandAlias("crash", "crash-test")]
        public CommandVoidResult ComCrashTest(CommandEvaluationContext context) => throw new Exception("command crash test (throws exception)");

        [Command("command with several option names types (short/long/both/none) (27/1/21)")]
        public CommandVoidResult ComOptNameTest(
            CommandEvaluationContext context,
            [Option(null, "long-name", "an option without a short name", true)] bool noShortNameOption,
            [Option("s", null, "an option without a long name", true)] bool noLongNameOption,
            [Option("b", "both", "an option with both", true)] bool withBothOption
            )
        {
            context.Out.Echo("noShortNameOption="); context.Out.Echo(noShortNameOption, true);
            context.Out.Echo("noLongNameOption="); context.Out.Echo(noLongNameOption, true);
            context.Out.Echo("withBothOption="); context.Out.Echo(withBothOption, true);
            return CommandVoidResult.Instance;
        }

        [Command("command with several option names types (short/long/both/none) and values (27/1/21)")]
        public CommandVoidResult ComOptNameWithValueTest(
            CommandEvaluationContext context,
            [Option(null, "long-name", "an option without a short name and a value", true, true)] int noShortNameOption = -32,
            [Option("s", null, "an option without a long name and a value", true, true)] object noLongNameOption = null,
            [Option("b", "both", "an option with both and a value", true, true)] string withBothOption = "a string"
            )
        {
            context.Out.Echo("noShortNameOption=");
            context.Out.Echo(noShortNameOption, true);
            context.Out.Echo("noLongNameOption=");
            context.Out.Echo(noLongNameOption, true);
            context.Out.Echo("withBothOption=");
            context.Out.Echo(withBothOption, true);
            return CommandVoidResult.Instance;
        }

        [Command("command with parameters type and value (27/1/21)")]
        public CommandVoidResult ComParamWithValueTest(
            CommandEvaluationContext context,
            [Parameter(0, "a parameter with a value", true)] object param
            )
        {
            context.Out.Echo("noShortNameOption="); context.Out.Echo(param, true);
            return CommandVoidResult.Instance;
        }

        [Command("command with collection parameters types (29/1/21)")]
        public CommandVoidResult ComParamColTest(
            CommandEvaluationContext context,
            [Option("A", "optiona", "option a", true, true)] DirSort A,
            [Option("b", "b", "option b", true, true)] List<string> b,
            [Option("c", "c", "option c", true, true)] List<int> c,
            [Parameter(1, "x", true)] DirSort B,
            [Parameter(2, "y", true)] List<DirSort> C
            )
        {
            int i = 0;
            var v = Enum.GetValues(typeof(DirSort));
            foreach (var s in Enum.GetNames(typeof(DirSort)))
            {
                context.Out.Echo($"s={s} n={(int)v.GetValue(i)}", true);
                i++;
            }

            context.Out.Echo("A="); context.Out.Echo(A, true);
            context.Out.Echo("b=");
            context.Out.Echo(b, true);
            context.Out.Echo("c="); context.Out.Echo(c, true);
            context.Out.Echo("B="); context.Out.Echo(B, true);
            context.Out.Echo("C="); context.Out.Echo(C, true);
            return CommandVoidResult.Instance;
        }
    }
}
