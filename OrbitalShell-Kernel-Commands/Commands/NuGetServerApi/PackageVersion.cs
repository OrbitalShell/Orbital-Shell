namespace OrbitalShell.Commands.NuGetServerApi
{
    public class PackageVersion
    {
        public string Version;
        public int Downloads;

        public override string ToString() => $"Version={Version} Downloads={Downloads}";
    }
}
