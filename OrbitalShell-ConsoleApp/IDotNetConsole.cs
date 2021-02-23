using System;
using System.Collections.Generic;
using System.IO;
using OrbitalShell.Component.UI;

namespace OrbitalShell.Component.Console
{
    public interface IDotNetConsole
    {
        public EventHandler ViewSizeChanged { get; set; }
        public EventHandler<WorkAreaScrollEventArgs> WorkAreaScrolled { get; set; }
        bool EnableConstraintConsolePrintInsideWorkArea { get; set; }
        string CodeBlockBegin { get; set; }
        string CodeBlockEnd { get; set; }
        ColorSettings Colors { get; set; }
        char CommandBlockBeginChar { get; set; }
        char CommandBlockEndChar { get; set; }
        char CommandSeparatorChar { get; set; }
        char CommandValueAssignationChar { get; set; }
        object ConsoleLock { get; }
        ConsoleColor? DefaultBackground { get; set; }
        ConsoleColor? DefaultForeground { get; set; }
        bool DumpExceptions { get; set; }
        TextWriterWrapper Err { get; set; }
        bool ForwardLogsToSystemDiagnostics { get; set; }
        TextReader In { get; set; }
        bool InWorkArea { get; }
        bool IsConsoleGeometryEnabled { get; }
        bool IsErrorRedirected { get; set; }
        bool IsOutputRedirected { get; set; }
        ConsoleTextWriterWrapper Out { get; set; }
        int TabLength { get; set; }
        bool TraceCommandErrors { get; set; }
        WorkArea WorkArea { get; }

        ActualWorkArea ActualWorkArea(bool fitToVisibleArea = true);
        bool CheckConsoleHasGeometry();
        void Error(string s = "");
        void Error(IEnumerable<string> ls);
        void Error(string s, bool lineBreak = false);
        void Errorln(string s = "");
        void Errorln(IEnumerable<string> ls);
        void Exit(int r = 0);
        void FixCoords(ref int x, ref int y);
        (int x, int y, int w, int h) GetCoords(int x, int y, int w, int h, bool fitToVisibleArea = true);
        int GetCursorX(object x);
        int GetCursorY(object x);
        void Infos();
        void Log(string s, bool enableForwardLogsToSystemDiagnostics = true);
        void LogError(Exception ex, bool enableForwardLogsToSystemDiagnostics = true);
        void LogError(string s, bool enableForwardLogsToSystemDiagnostics = true);
        void LogException(Exception ex, string message = "", bool enableForwardLogsToSystemDiagnostics = true);
        void LogWarning(string s, bool enableForwardLogsToSystemDiagnostics = true);
        string Readln(string prompt = null);
        void RedirectErr(string filepath = null);
        void RedirectErr(TextWriter sw);
        void RedirectOut(string filepath = null);
        void RedirectOut(StreamWriter sw);
        void SetCursorAtWorkAreaTop();
        void Warning(string s = "");
        void Warning(IEnumerable<string> ls);
        void Warning(string s, bool lineBreak = false);
        void Warningln(string s = "");
        void Warningln(IEnumerable<string> ls);
    }
}