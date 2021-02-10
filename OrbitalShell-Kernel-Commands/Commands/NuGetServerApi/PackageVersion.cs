using OrbitalShell.Component.Console;

namespace OrbitalShell.Commands.NuGetServerApi
{
    public class PackageVersion
    {
        public string Version;
        public int Downloads;

        public override string ToString() => $"Version={Version} Downloads={Downloads}";

        /// <summary>
        /// Echo method
        /// </summary>
        /// <param name="context">echo context</param>
        public void Echo(EchoEvaluationContext context)
        {
            context.Out.Echo($"Version={Version} Downloads={Downloads}", context.Options.LineBreak, context.Options.IsRawModeEnabled);
        }
    }
}
