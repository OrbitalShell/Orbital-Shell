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

            AppDataFolderName = "OrbitalShell";
            UserProfileFileName = ".orbsh_profile";
            LogFileName = "orbsh.log";
            HistoryFileName = ".history";
            CommandsAliasFileName = ".aliases";
            
        }
    }
}
