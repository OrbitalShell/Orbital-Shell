using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

using OrbitalShell.Component.CommandLine.Processor;

namespace OrbitalShell.Component
{
    /// <summary>
    /// screen / file logger for the kernel
    /// </summary>
    public class Logger
    {
        public readonly CommandEvaluationContext CommandEvaluationContext;
        static object _logFileLock = new object();
        public bool MuteLogErrors;

        public Logger(CommandEvaluationContext context)
        {
            CommandEvaluationContext = context;
        }

        #region log screeen + file methods

        private string _LogMessage(string message, string prefix, string postfix = " : ")
            => (string.IsNullOrWhiteSpace(prefix)) ? message : (prefix + (message == null ? "" : $"{postfix}{message}"));

        public void Success(string message = null, bool log = true, bool lineBreak = true, string prefix = "Success")
        {
            var logMessage = CommandEvaluationContext.ShellEnv.Colors.Success + _LogMessage(message, prefix);
            CommandEvaluationContext.Out.Echo(logMessage,lineBreak);
            if (log) Log(logMessage);
        }

        public void Done(string message = null, bool log = true, bool lineBreak = true, string prefix = "Done")
        {
            var logMessage = CommandEvaluationContext.ShellEnv.Colors.Success + _LogMessage(message, prefix);
            CommandEvaluationContext.Out.Echo(logMessage, lineBreak);
            if (log) Log(logMessage);
        }

        public void Info(string message, bool log = true, bool lineBreak = true, string prefix = "")
        {
            var logMessage = CommandEvaluationContext.ShellEnv.Colors.Log + _LogMessage(message, prefix);
            CommandEvaluationContext.Out.Echo(logMessage, lineBreak);
            if (log) Log(logMessage);
        }

        public void Fail(string message = null, bool log = true, bool lineBreak = true, string prefix = "Fail")
        {
            var logMessage = CommandEvaluationContext.ShellEnv.Colors.Error + _LogMessage(message, prefix, "");
            CommandEvaluationContext.Out.Echo(logMessage, lineBreak);
            if (log) Log(logMessage);
        }

        public void Warning(string message = null, bool log = true, bool lineBreak = true, string prefix = "Warning")
        {
            var logMessage = CommandEvaluationContext.ShellEnv.Colors.Warning + _LogMessage(message, prefix);
            CommandEvaluationContext.Out.Echo(logMessage, lineBreak);
            if (log) LogWarning(logMessage);
        }

        public void Fail(Exception exception, bool log = true, bool lineBreak = true, string prefix = "Fail : ")
        {
            var logMessage = CommandEvaluationContext.ShellEnv.Colors.Error + prefix + exception?.Message;
            CommandEvaluationContext.Out.Echo(logMessage, lineBreak);
            if (log) LogError(logMessage);
        }

        public void Error(string message = null, bool log = false, bool lineBreak = true, string prefix = "")
        {
            var logMessage = CommandEvaluationContext.ShellEnv.Colors.Error + prefix + (message == null ? "" : $"{message}");
            CommandEvaluationContext.Out.Echo(logMessage, lineBreak);
            if (log) LogError(logMessage);
        }

        public Exception Log(string text)
        {
            return LogInternal(text);
        }

        public Exception LogError(string text)
        {
            return LogInternal(text, CommandEvaluationContext.ShellEnv.Colors.Error + "ERR");
        }

        public Exception LogWarning(string text)
        {
            return LogInternal(text, CommandEvaluationContext.ShellEnv.Colors.Warning + "ERR");
        }

        Exception LogInternal(string text, string logPrefix = "INF")
        {
            var str = $"{logPrefix} [{Process.GetCurrentProcess().ProcessName}:{Process.GetCurrentProcess().Id},{Thread.CurrentThread.Name}:{Thread.CurrentThread.ManagedThreadId}] {System.DateTime.Now}.{System.DateTime.Now.Millisecond} | {text}";
            lock (_logFileLock)
            {
                try
                {
                    File.AppendAllLines(CommandEvaluationContext.CommandLineProcessor.Settings.LogFilePath, new List<string> { str });
                    return null;
                }
                catch (Exception logAppendAllLinesException)
                {
                    if (!MuteLogErrors)
                    {
                        if (CommandEvaluationContext.CommandLineProcessor.Settings.LogAppendAllLinesErrorIsEnabled)
                            CommandEvaluationContext.Errorln(logAppendAllLinesException.Message);
                    }
                    return logAppendAllLinesException;
                }
            }
        }

        #endregion
    }
}
