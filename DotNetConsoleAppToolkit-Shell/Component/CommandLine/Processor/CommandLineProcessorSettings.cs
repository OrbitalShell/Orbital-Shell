using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DotNetConsoleAppToolkit.Component.CommandLine.Processor
{
    public class CommandLineProcessorSettings
    {
        public string AppName = "CommandLineProcessor";
        public string AppLongName = "DotNetConsoleAppToolkit Component CommandLineProcessor";
        public string AppEditor = "DotNetConsoleAppToolkit-Shell";
        public string AppVersion = "AppVersion";
        public string AppLicense = "MIT";

        public string AppDataFolderName = "DotNetConsoleAppToolkit-Shell";

        public string UserProfileFileName = ".profile";
        public string LogFileName = "log";
        public string HistoryFileName = ".history";
        public string CommandsAliasFileName = ".aliases";
        public string DefaultsFolderName = "Defaults";

        public string AppDataFolderPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppDataFolderName);
        public string UserProfileFilePath => Path.Combine(AppDataFolderPath, UserProfileFileName);
        public string LogFilePath => Path.Combine(AppDataFolderPath, LogFileName);
        public string HistoryFilePath => Path.Combine(AppDataFolderPath, HistoryFileName);
        public string CommandsAliasFilePath => Path.Combine(AppDataFolderPath, CommandsAliasFileName);

        public string UserProfileFolder => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public string BinFolderPath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public string DefaultsFolderPath
        {
            get
            {
                var segs = this.GetType().FullName.Split('.');
                var splits = segs.AsSpan(1, segs.Count() - 3);
                var path = BinFolderPath;
                foreach (var split in splits)
                    path = Path.Combine(path, split);
                return Path.Combine(path, DefaultsFolderName);
            }
        }

        public string ShellEnvironmentVariableName = nameof(CommandLineProcessorSettings).ToString().Split('.').First();

        public bool LogAppendAllLinesErrorIsEnabled = true;
        public bool PrintInfo = true;

        public char ErrorPositionMarker = '^';

    }
}
