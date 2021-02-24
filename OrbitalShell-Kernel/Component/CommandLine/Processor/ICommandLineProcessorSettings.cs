using System.IO;

using OrbitalShell.Component.Console;

namespace OrbitalShell.Component.CommandLine.Processor
{
    public interface ICommandLineProcessorSettings
    {
        string AppDataFolderName { get; set; }
        string AppDataRoamingUserFolderPath { get; }
        string AppEditor { get; set; }
        string AppLicense { get; set; }
        string AppLongName { get; set; }
        string AppName { get; set; }
        string AppVersion { get; set; }
        CommandEvaluationContext CommandEvaluationContext { get; }
        string CommandsAliasFileName { get; set; }
        string CommandsAliasFilePath { get; }
        string DefaultCommandsAliasFileName { get; set; }
        string DefaultsFolderName { get; set; }
        string DefaultsFolderPath { get; }
        string DefaultUserProfileFileName { get; set; }
        TextWriterWrapper Err { get; }
        char ErrorPositionMarker { get; set; }
        string HistoryFileName { get; set; }
        string HistoryFilePath { get; }
        TextReader In { get; }
        string KernelCommandsModuleAssemblyName { get; set; }
        string KernelCommandsRootNamespace { get; set; }
        bool LogAppendAllLinesErrorIsEnabled { get; set; }
        string LogFileName { get; set; }
        string LogFilePath { get; }
        string ModulesInitFileName { get; set; }
        string ModulesInitFilePath { get; }
        ConsoleTextWriterWrapper Out { get; }
        string PathExtInit { get; set; }
        bool PrintInfo { get; set; }
        string ShellAppDataPath { get; }
        string ShellExecBatchExt { get; set; }
        string UserProfileFileName { get; set; }
        string UserProfileFilePath { get; }

        void Initialize(CommandEvaluationContext commandEvaluationContext);
    }
}