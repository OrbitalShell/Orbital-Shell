using System;
using System.IO;
using System.Linq;
using System.Reflection;
using OrbitalShell.Component.Console;

namespace OrbitalShell.Component.CommandLine.Processor
{
    /// <summary>
    /// @TODO: move in Orbsh settings
    /// </summary>
    public class CommandLineProcessorSettings
    {
        public CommandLineProcessorSettings()
        {
        }

        /// <summary>
        /// initialize contextual attributes and setup the context streams
        /// </summary>
        /// <param name="commandEvaluationContext">context</param>
        public void Initialize(CommandEvaluationContext commandEvaluationContext)
        {
            this.CommandEvaluationContext = commandEvaluationContext;

            // TODO: activate after DotNetConsole is no more static - see remarks below
            /*
            Out = new ConsoleTextWriterWrapper(System.Console.Out);   
            Err = new TextWriterWrapper(System.Console.Error);
            In = System.Console.In;
            */

            //Out = DotNetConsole.Out;
            Out = new ShellConsoleTextWriterWrapper(
                commandEvaluationContext,
                System.Console.Out
            );

            /*  
                it can exists only one wrapper for out,
                between the command evaluation context and dot net console
            */
            DotNetConsole.Out = Out;

            Err = DotNetConsole.Err;
            In = DotNetConsole.In;

            commandEvaluationContext.SetStreams(Out, In, Err);
        }

        public CommandEvaluationContext CommandEvaluationContext { get; protected set; }

        #region streams

        public ConsoleTextWriterWrapper Out { get; protected set; }

        public TextWriterWrapper Err;

        public TextReader In;

        #endregion

        #region product info

        public string AppName = "CommandLineProcessor";
        public string AppLongName = "DotNetConsoleAppToolkit Component CommandLineProcessor";
        public string AppEditor = "DotNetConsoleAppToolkit-Shell";
        public string AppVersion = "AppVersion";
        public string AppLicense = "MIT";

        #endregion

        #region official files names

        public string UserProfileFileName = ".profile";
        public string LogFileName = "log";
        public string HistoryFileName = ".history";
        public string CommandsAliasFileName = ".aliases";
        public string DefaultsFolderName = "Defaults";

        #endregion

        #region official paths

        /// <summary>
        /// shell app data folder name (application settings)
        /// </summary>
        public string AppDataFolderName = "DotNetConsoleAppToolkit-Shell";
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
                var segs = this.GetType().FullName.Replace("CommandLine", "Shell").Split('.');
                var splits = segs.AsSpan(1, segs.Count() - 3);
                var path = BinFolderPath;
                foreach (var split in splits)
                    path = Path.Combine(path, split);
                return Path.Combine(path, DefaultsFolderName);
            }
        }

        #endregion

        #region settings    TODO: check real useless / move to good location

        public string PathExtInit = "sh;init;";

        public string ShellExecBatchExt = ".sh";

        public bool LogAppendAllLinesErrorIsEnabled = true;

        public bool PrintInfo = true;

        public char ErrorPositionMarker = '^';

        public string KernelCommandsModuleAssemblyName = "OrbitalShell-Kernel-Commands";

        public string KernelCommandsRootNamespace = "Commands";

        #endregion
    }
}
