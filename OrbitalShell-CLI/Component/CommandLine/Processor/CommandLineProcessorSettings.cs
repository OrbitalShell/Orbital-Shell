using proc = DotNetConsoleAppToolkit.Component.CommandLine.Processor;

namespace OrbitalShell.Component.CommandLine.Processor
{
    public class CommandLineProcessorSettings
         : proc.CommandLineProcessorSettings
    {
        public CommandLineProcessorSettings()
        {
            AppName = "orbsh";
            AppLongName = "Orbital Shell";
            AppEditor = "released on June 2020 under licence MIT";
            AppVersion = "1.0-beta";

            AppDataFolderName = "OrbitalShell";
            UserProfileFileName = ".profile";
            LogFileName = ".log";
            HistoryFileName = ".history";
            CommandsAliasFileName = ".aliases";

            ShellEnvironmentVariableName = "shell";
        }
    }
}
