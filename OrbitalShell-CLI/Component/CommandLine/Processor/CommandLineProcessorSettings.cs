using System.Reflection;

namespace OrbitalShell.Component.CommandLine.Processor
{
    public class OrbitalShellCommandLineProcessorSettings : CommandLineProcessorSettings
    {
        public OrbitalShellCommandLineProcessorSettings()
        {
            AppName = "orbsh";
            AppLongName = "Orbital Shell";
            AppEditor = "released on June 2020 under licence MIT";
            AppVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>().Version;

            AppDataFolderName = "OrbitalShell";
            UserProfileFileName = ".profile";
            InitFileName = ".init";
            LogFileName = ".log";
            HistoryFileName = ".history";
            CommandsAliasFileName = ".aliases";
        }
    }
}
