#define dbg
#define enable_cscript

using OrbitalShell.Component.UI;
using OrbitalShell.Component.Console;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using RuntimeEnvironment = OrbitalShell.Lib.RuntimeEnvironment;
using sc = System.Console;
using static OrbitalShell.Component.EchoDirective.Shortcuts;
using OrbitalShell.Component.EchoDirective;

namespace OrbitalShell
{
    /// <summary>
    /// dotnet core sdk helps build fastly nice console applications
    /// <para>
    /// slowness due to:
    /// - many system calls on both linux (ConsolePal.Unix.cs) and windows (ConsolePal.Windows.cs)
    /// - the .net core make use of interop for each console method call in windows (ConsolePal.Windows.cs)
    /// </para>
    /// </summary>
    public class Console : IConsole
    {
        #region attributes

        #region streams : entry points to DotNetConsole output operations

        public ConsoleTextWriterWrapper Out { get; set; }
        public TextWriterWrapper Err { get; set; } = new TextWriterWrapper(sc.Error);
        public TextReader In { get; set; } = System.Console.In;

        #endregion

        #region work area settings

        readonly WorkArea _workArea = new WorkArea();
        public WorkArea WorkArea => new WorkArea(_workArea);
        public bool InWorkArea => !_workArea.Rect.IsEmpty;

        public EventHandler ViewSizeChanged { get; set; }
        public EventHandler<WorkAreaScrollEventArgs> WorkAreaScrolled { get; set; }

        public bool EnableConstraintConsolePrintInsideWorkArea { get; set; } = false;

        #endregion

        public bool IsErrorRedirected { get; set; } = false;
        public bool IsOutputRedirected { get; set; } = false;

        //public int UIWatcherThreadDelay = 500;
        //public ViewResizeStrategy ViewResizeStrategy = ViewResizeStrategy.FitViewSize;
        public bool ClearOnViewResized = true;      // false not works properly in Windows Terminal + fit view size

        public bool SaveColors = /*true*/ false; /*bug fix*/ // TODO: remove

        public bool TraceCommandErrors { get; set; } = true;

        public bool DumpExceptions { get; set; } = true;
        public ConsoleColor? DefaultForeground { get; set; }
        public ConsoleColor? DefaultBackground { get; set; }

        public char CommandBlockBeginChar { get; set; } = '(';
        public char CommandBlockEndChar { get; set; } = ')';
        public char CommandSeparatorChar { get; set; } = ',';
        public char CommandValueAssignationChar { get; set; } = '=';
        public string CodeBlockBegin { get; set; } = "[[";
        public string CodeBlockEnd { get; set; } = "]]";

        public bool ForwardLogsToSystemDiagnostics { get; set; } = true;
        public int TabLength { get; set; } = 7;

        TextWriter _errorWriter;
        StreamWriter _errorStreamWriter;
        FileStream _errorFileStream;
        TextWriter _outputWriter;
        StreamWriter _outputStreamWriter;
        FileStream _outputFileStream;

        readonly string[] _crlf = { Environment.NewLine };

        public object ConsoleLock => Out.Lock;

        //Thread _watcherThread;
        //readonly Dictionary<int, UIElement> _uielements = new Dictionary<int, UIElement>();
        public bool RedrawUIElementsEnabled = true;

        public ColorSettings Colors { get; set; }

        #endregion

        public Console()
        {
            Out = new ConsoleTextWriterWrapper(this, sc.Out);       // INFINITE LOOP
            Colors = new ColorSettings(this);
            Shortcuts.Initialize(this);
        }

        #region log methods

        public void LogError(Exception ex, bool enableForwardLogsToSystemDiagnostics = true)
        {
            if (ForwardLogsToSystemDiagnostics && enableForwardLogsToSystemDiagnostics) System.Diagnostics.Debug.WriteLine(ex + "");
            if (DumpExceptions)
                LogException(ex);
            else
            {
                var msg = ex.Message;
                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                    msg += Environment.NewLine + ex.Message;
                }
                var ls = msg.Split(_crlf, StringSplitOptions.None)
                    .Select(x => Colors.Error + x);
                Errorln(ls);
            }
        }

        public void LogException(Exception ex, string message = "", bool enableForwardLogsToSystemDiagnostics = true)
        {
            if (ForwardLogsToSystemDiagnostics && enableForwardLogsToSystemDiagnostics) System.Diagnostics.Debug.WriteLine(message + _crlf + ex + "");
            var ls = new List<string>();
            if (DumpExceptions)
            {
                ls = (ex + "").Split(_crlf, StringSplitOptions.None)
                .Select(x => Colors.Error + x)
                .ToList();
                if (message != null) ls.Insert(0, $"{Colors.Error}{message}");
            }
            else
                ls.Insert(0, $"{Colors.Error}{message}: {ex.Message}");
            Errorln(ls);
        }

        public void LogError(string s, bool enableForwardLogsToSystemDiagnostics = true)
        {
            if (ForwardLogsToSystemDiagnostics && enableForwardLogsToSystemDiagnostics) System.Diagnostics.Debug.WriteLine(s);
            var ls = (s + "").Split(_crlf, StringSplitOptions.None)
                .Select(x => Colors.Error + x);
            Errorln(ls);
        }

        public void LogWarning(string s, bool enableForwardLogsToSystemDiagnostics = true)
        {
            if (ForwardLogsToSystemDiagnostics && enableForwardLogsToSystemDiagnostics) System.Diagnostics.Debug.WriteLine(s);
            var ls = (s + "").Split(_crlf, StringSplitOptions.None)
                .Select(x => Colors.Warning + x);
            Errorln(ls);
        }

        public void Log(string s, bool enableForwardLogsToSystemDiagnostics = true)
        {
            if (ForwardLogsToSystemDiagnostics && enableForwardLogsToSystemDiagnostics) System.Diagnostics.Debug.WriteLine(s);
            var ls = (s + "").Split(_crlf, StringSplitOptions.None)
                .Select(x => Colors.Log + x);
            //Out.Echoln(ls);
            foreach (var l in ls) Out.Echoln(l);
        }

        #endregion

        #region operations

        public void Error(string s = "") => Error(s, false);
        public void Errorln(string s = "") => Error(s, true);
        public void Errorln(IEnumerable<string> ls) { foreach (var s in ls) Errorln(s); }
        public void Error(IEnumerable<string> ls) { foreach (var s in ls) Error(s); }
        public void Error(string s, bool lineBreak = false)
        {
            lock (Out.Lock)
            {
                Out.RedirecToErr = true;
                Out.Echo($"{Colors.Error}{s}{Colors.Default}", lineBreak);
                Out.RedirecToErr = false;
            }
        }

        public void Warning(string s = "") => Warning(s, false);
        public void Warningln(string s = "") => Warning(s, true);
        public void Warningln(IEnumerable<string> ls) { foreach (var s in ls) Errorln(s); }
        public void Warning(IEnumerable<string> ls) { foreach (var s in ls) Error(s); }
        public void Warning(string s, bool lineBreak = false)
        {
            lock (Out.Lock)
            {
                Out.Echo($"{Colors.Warning}{s}{Colors.Default}", lineBreak);
            }
        }

        public string Readln(string prompt = null)
        {
            lock (Out.Lock)
            {
                if (prompt != null) Out.Echo(prompt);
            }
            return sc.ReadLine();
        }

        public void Infos()
        {
            lock (Out.Lock)
            {
                Out.Echoln($"OS={Environment.OSVersion} {(Environment.Is64BitOperatingSystem ? "64" : "32")}bits plateform={RuntimeEnvironment.OSType}");
                Out.Echoln($"{White}{Bkf}{Colors.HighlightIdentifier}window:{Rsf} left={Colors.Numeric}{sc.WindowLeft}{Rsf},top={Colors.Numeric}{sc.WindowTop}{Rsf},width={Colors.Numeric}{sc.WindowWidth}{Rsf},height={Colors.Numeric}{sc.WindowHeight}{Rsf},largest width={Colors.Numeric}{sc.LargestWindowWidth}{Rsf},largest height={Colors.Numeric}{sc.LargestWindowHeight}{Rsf}");
                Out.Echoln($"{Colors.HighlightIdentifier}buffer:{Rsf} width={Colors.Numeric}{sc.BufferWidth}{Rsf},height={Colors.Numeric}{sc.BufferHeight}{Rsf} | input encoding={Colors.Numeric}{sc.InputEncoding.EncodingName}{Rsf} | output encoding={Colors.Numeric}{sc.OutputEncoding.EncodingName}{Rsf}");
                Out.Echoln($"{White}default background color={Bkf}{Colors.KeyWord}{DefaultBackground}{Rsf} | default foreground color={Colors.KeyWord}{DefaultForeground}{Rsf}");
                if (RuntimeEnvironment.OSType == OSPlatform.Windows)
                {
#pragma warning disable CA1416 // Valider la compatibilité de la plateforme
                    Out.Echoln($"number lock={Colors.Numeric}{sc.NumberLock}{Rsf} | capslock={Colors.Numeric}{sc.CapsLock}{Rsf}");
#pragma warning restore CA1416 // Valider la compatibilité de la plateforme
#pragma warning disable CA1416 // Valider la compatibilité de la plateforme
                    Out.Echoln($"cursor visible={Colors.Numeric}{sc.CursorVisible}{Rsf} | cursor size={Colors.Numeric}{sc.CursorSize}");
#pragma warning restore CA1416 // Valider la compatibilité de la plateforme
                }
            };
        }

        /// <summary>
        /// terminates current process
        /// </summary>
        /// <param name="r">return code</param>
        public void Exit(int r = 0) => Environment.Exit(r);

        #endregion

        #region work area operations

        /// <summary>
        /// this setting limit wide of lines (available width -1) to prevent sys console to automatically put a line break when reaching end of line (console bug ?)
        /// </summary>
        public bool AvoidConsoleAutoLineBreakAtEndOfLine = false;

        /// <summary>
        /// true until the contrary is detected (exception in GetCoords : sc.WindowLeft)
        /// </summary>
        public bool IsConsoleGeometryEnabled { get; protected set; } = true;

        /// <summary>
        /// update the IsConsoleGeometryEnabled field
        /// </summary>
        /// <returns>value of the field</returns>
        public bool CheckConsoleHasGeometry()
        {
            try
            {
                var x = sc.WindowLeft;
            }
            catch (Exception)
            {
                IsConsoleGeometryEnabled = false;
                return false;
            }
            return true;
        }

        public (int x, int y, int w, int h) GetCoords(int x, int y, int w, int h, bool fitToVisibleArea = true)
        {
            // (1) dos console (eg. vs debug consolehep) set WindowTop as y scroll position. WSL console doesn't (still 0)
            // scroll -> native dos console set WindowTop and WindowLeft as base scroll coordinates
            // if WorkArea defined, we must use absolute coordinates and not related
            // CursorLeft and CursorTop are always good
            lock (ConsoleLock)
            {
                if (!IsConsoleGeometryEnabled) return (x, y, 1000, 1000);

                if (x < 0) x = sc.WindowLeft + sc.WindowWidth + x;

                if (y < 0) y = /*sc.WindowTop (fix 1) */ +sc.WindowHeight + y;

                if (fitToVisibleArea)
                {
                    if (w < 0) w = sc.WindowWidth + ((AvoidConsoleAutoLineBreakAtEndOfLine) ? -1 : 0) + (w + 1)     // 1 POS TOO MUCH !!
                            /*+ sc.WindowLeft*/;

                    if (h < 0) h = sc.WindowHeight + h
                            + sc.WindowTop; /* ?? */
                }
                else
                {
                    if (w < 0) w = sc.BufferWidth + ((AvoidConsoleAutoLineBreakAtEndOfLine) ? -1 : 0) + (w + 1);

                    if (h < 0) h = sc.WindowHeight + h + sc.WindowTop;
                }
                return (x, y, w, h);
            }
        }

        // TODO: TO SLOW IN CASE OF PASTE TEXT INTO CONSOLE (reader call this on each new character inputed)
        public ActualWorkArea ActualWorkArea(bool fitToVisibleArea = true)
        {
            if (!IsConsoleGeometryEnabled) return new ActualWorkArea(_workArea.Id, 0, 0, 0, 0);
            var x0 = _workArea.Rect.IsEmpty ? 0 : _workArea.Rect.X;
            var y0 = _workArea.Rect.IsEmpty ? 0 : _workArea.Rect.Y;
            var w0 = _workArea.Rect.IsEmpty ? -1 : _workArea.Rect.Width;
            var h0 = _workArea.Rect.IsEmpty ? -1 : _workArea.Rect.Height;
            var (x, y, w, h) = GetCoords(x0, y0, w0, h0, fitToVisibleArea);
            return new ActualWorkArea(_workArea.Id, x, y, w, h);
        }



        public void SetCursorAtWorkAreaTop()
        {
            if (!IsConsoleGeometryEnabled || _workArea.Rect.IsEmpty) return;     // TODO: set cursor even if workarea empty?
            lock (Out.Lock)
            {
                Out.SetCursorPos(_workArea.Rect.X, _workArea.Rect.Y);
            }
        }

        #endregion

        #region UI operations

        public void FixCoords(ref int x, ref int y)
        {
            lock (ConsoleLock)
            {
                x = Math.Max(0, Math.Min(x, sc.BufferWidth - 1));
                y = Math.Max(0, Math.Min(y, sc.BufferHeight - 1));
            }
        }

        #endregion

        #region stream methods

        public void RedirectOut(StreamWriter sw)
        {
            if (sw != null)
            {
                Out.Redirect(sw);
                _outputWriter = sc.Out;
                sc.SetOut(sw);
                IsOutputRedirected = true;
            }
            else
            {
                Out.Redirect((TextWriter)null);
                sc.SetOut(_outputWriter);
                _outputWriter = null;
                IsOutputRedirected = false;
            }
        }

        public void RedirectErr(TextWriter sw)
        {
            if (sw != null)
            {
                Err.Redirect(sw);
                _errorWriter = sc.Error;
                sc.SetError(sw);
                IsErrorRedirected = true;
            }
            else
            {
                Err.Redirect((TextWriter)null);
                sc.SetError(_errorWriter);
                _errorWriter = null;
                IsErrorRedirected = false;
            }
        }

        public void RedirectOut(string filepath = null)
        {
            if (filepath != null)
            {
                _outputWriter = sc.Out;
                _outputFileStream = new FileStream(filepath, FileMode.Append, FileAccess.Write);
                _outputStreamWriter = new StreamWriter(_outputFileStream);
                sc.SetOut(_outputStreamWriter);
                Out.Redirect(_outputStreamWriter);
            }
            else
            {
                _outputStreamWriter.Flush();
                _outputStreamWriter.Close();
                _outputStreamWriter = null;
                sc.SetOut(_outputWriter);
                _outputWriter = null;
                Out.Redirect((string)null);
            }
        }

        public void RedirectErr(string filepath = null)
        {
            if (filepath != null)
            {
                _errorWriter = sc.Error;
                _errorFileStream = new FileStream(filepath, FileMode.Append, FileAccess.Write);
                _errorStreamWriter = new StreamWriter(_errorFileStream);
                sc.SetOut(_errorStreamWriter);
                Err.Redirect(_errorStreamWriter);
            }
            else
            {
                _errorStreamWriter.Flush();
                _errorStreamWriter.Close();
                _errorStreamWriter = null;
                sc.SetOut(_errorWriter);
                _errorWriter = null;
                Err.Redirect((string)null);
            }
        }

        #endregion

        #region implementation methods

        public int GetCursorX(object x)
        {
            if (x != null && x is string s && !string.IsNullOrWhiteSpace(s)
                && int.TryParse(s, out var v))
                return v;
            if (TraceCommandErrors) LogError($"wrong cursor x: {x}");
            if (!IsConsoleGeometryEnabled) return 0;
            lock (Out.Lock)
            {
                return sc.CursorLeft;
            }
        }

        public int GetCursorY(object x)
        {
            if (x != null && x is string s && !string.IsNullOrWhiteSpace(s)
                && int.TryParse(s, out var v))
                return v;
            if (TraceCommandErrors) LogError($"wrong cursor y: {x}");
            if (!IsConsoleGeometryEnabled) return 0;
            lock (Out.Lock)
            {
                return sc.CursorTop;
            }
        }

        #endregion

    }
}
