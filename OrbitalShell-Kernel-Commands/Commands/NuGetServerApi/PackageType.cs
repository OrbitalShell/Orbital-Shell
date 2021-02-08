using OrbitalShell.Component.Console;

namespace OrbitalShell.Commands.NuGetServerApi
{
    public class PackageType
    {
        public string Name;

        public override string ToString() => Name;

        /// <summary>
        /// Echo method
        /// </summary>
        /// <param name="context">echo context</param>
        public void Echo(EchoEvaluationContext context)
        {
            context.Out.Echoln(Name);
        }

    }
}
