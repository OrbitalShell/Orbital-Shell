using OrbitalShell.Component.CommandLine.Processor;

namespace OrbitalShell.Component.Shell.Module
{
    /// <summary>
    /// infos about what inside the module
    /// </summary>
    public class ModuleInfo
    {
        public readonly int DeclaringTypesCount = -1;

        public readonly int CommandsCount = -1;

        public readonly int HooksCount = -1;

        public static ModuleInfo ModuleInfoNotDefined = new ModuleInfo();

        public ModuleInfo(
            int declaringTypesCount,
            int commandsCount,
            int hooksCount = 0
        )
        {
            DeclaringTypesCount = declaringTypesCount;
            CommandsCount = commandsCount;
            HooksCount = hooksCount;
        }

        public ModuleInfo() { }

        public string GetDescriptor(CommandEvaluationContext context)
        {
            var f = "" + context.ShellEnv.Colors.Log;
            var r = $"{f}[dt={context.ShellEnv.Colors.Numeric}{DeclaringTypesCount}{f}";
            r += $",com={context.ShellEnv.Colors.Numeric}{CommandsCount}{f}";
            r += $",hk={context.ShellEnv.Colors.Numeric}{HooksCount}{f}](rdc)";
            return r;
        }
    }
}