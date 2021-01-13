#define dbg

using DotNetConsoleAppToolkit.Component.UI;
using DotNetConsoleAppToolkit.Console;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using RuntimeEnvironment = DotNetConsoleAppToolkit.Lib.RuntimeEnvironment;
using sc = System.Console;

namespace DotNetConsoleAppToolkit
{
    /// <summary>
    /// dotnet core sdk helps build fastly nice console applications
    /// <para>
    /// slowness due to:
    /// - many system calls on both linux (ConsolePal.Unix.cs) and windows (ConsolePal.Windows.cs)
    /// - the .net core make use of interop for each console method call in windows (ConsolePal.Windows.cs)
    /// implemented the workaround:
    /// - 'retains mode' : echo in a buffer in order to output as much text as possible in one time
    /// - needs a refactorization: 
    /// -- dotnetconsole api placed on top of streams out,error as a wrapper (Console/ConsoleTextWriterWrapper.cs)
    /// -- explode DotNetConsole in small parts
    /// </para>
    /// </summary>
    public static class DotNetConsole
    {
        #region attributes

        #region streams : entry points to DotNetConsole output operations

        public static readonly ConsoleTextWriterWrapper Out = new ConsoleTextWriterWrapper(sc.Out);
        public static readonly TextWriterWrapper Err = new ConsoleTextWriterWrapper(sc.Error);

        #endregion

        #region work area settings

        static WorkArea _workArea = new WorkArea();
        public static WorkArea WorkArea => new WorkArea(_workArea);
        public static bool InWorkArea => !_workArea.Rect.IsEmpty;
        public static EventHandler ViewSizeChanged;
        public static EventHandler<WorkAreaScrollEventArgs> WorkAreaScrolled;
        public static bool EnableConstraintConsolePrintInsideWorkArea = false;

        #endregion

        public static bool IsErrorRedirected = false;
        public static bool IsOutputRedirected = false;

        //public static int UIWatcherThreadDelay = 500;
        //public static ViewResizeStrategy ViewResizeStrategy = ViewResizeStrategy.FitViewSize;
        public static bool ClearOnViewResized = true;      // false not works properly in Windows Terminal + fit view size
        
        public static bool SaveColors = /*true*/ false; /*bug fix*/ // TODO: remove
        
        public static bool TraceCommandErrors = true;
        public static bool DumpExceptions = true;
        public static ConsoleColor DefaultForeground;
        public static ConsoleColor DefaultBackground;

        public static char CommandBlockBeginChar = '(';
        public static char CommandBlockEndChar = ')';
        public static char CommandSeparatorChar = ',';
        public static char CommandValueAssignationChar = '=';
        public static string CodeBlockBegin = "[[";
        public static string CodeBlockEnd = "]]";

        public static bool ForwardLogsToSystemDiagnostics = true;
        public static int TabLength = 7;

        static TextWriter _errorWriter;
        static StreamWriter _errorStreamWriter;
        static FileStream _errorFileStream;
        static TextWriter _outputWriter;
        static StreamWriter _outputStreamWriter;
        static FileStream _outputFileStream;

        static readonly Dictionary<string, Script<object>> _csscripts = new Dictionary<string, Script<object>>();

        static string[] _crlf = { Environment.NewLine };

        public static object ConsoleLock => Out.Lock;

        //static Thread _watcherThread;
        //static readonly Dictionary<int, UIElement> _uielements = new Dictionary<int, UIElement>();
        public static bool RedrawUIElementsEnabled = true;

        public static ColorSettings Colors = new ColorSettings();

        #endregion

        #region log methods

        public static void LogError(Exception ex)
        {
            if (ForwardLogsToSystemDiagnostics) System.Diagnostics.Debug.WriteLine(ex + "");
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

        public static void LogException(Exception ex, string message = "")
        {
            if (ForwardLogsToSystemDiagnostics) System.Diagnostics.Debug.WriteLine(message + _crlf + ex + "");
            var ls = new List<string>();
            if (DumpExceptions)
            {
                ls = (ex + "").Split(_crlf, StringSplitOptions.None)
                .Select(x => Colors.Error + x)
                .ToList();
                if (message != null) ls.Insert(0, $"{Colors.Error}{message}");
            } else
                ls.Insert(0, $"{Colors.Error}{message}: {ex.Message}");
            Errorln(ls);
        }

        public static void LogError(string s)
        {
            if (ForwardLogsToSystemDiagnostics) System.Diagnostics.Debug.WriteLine(s);
            var ls = (s + "").Split(_crlf, StringSplitOptions.None)
                .Select(x => Colors.Error + x);
            Errorln(ls);
        }

        public static void LogWarning(string s)
        {
            if (ForwardLogsToSystemDiagnostics) System.Diagnostics.Debug.WriteLine(s);
            var ls = (s + "").Split(_crlf, StringSplitOptions.None)
                .Select(x => Colors.Warning + x);
            Errorln(ls);
        }

        public static void Log(string s)
        {
            if (ForwardLogsToSystemDiagnostics) System.Diagnostics.Debug.WriteLine(s);
            var ls = (s + "").Split(_crlf, StringSplitOptions.None)
                .Select(x => Colors.Log + x);
            Out.Echoln(ls);
        }

        #endregion

        public static void Error(string s = "") => Error(s, false);
        public static void Errorln(string s = "") => Error(s, true);
        public static void Errorln(IEnumerable<string> ls) { foreach (var s in ls) Errorln(s); }
        public static void Error(IEnumerable<string> ls) { foreach (var s in ls) Error(s); }
        public static void Error(string s, bool lineBreak = false)
        {
            lock (Out.Lock)
            {
                Out.RedirecToErr = true;
                Out.Echo($"{Colors.Error}{s}{Colors.Default}", lineBreak);
                Out.RedirecToErr = false;
            }
        }

        public static void Warning(string s = "") => Warning(s, false);
        public static void Warningln(string s = "") => Warning(s, true);
        public static void Warningln(IEnumerable<string> ls) { foreach (var s in ls) Errorln(s); }
        public static void Warning(IEnumerable<string> ls) { foreach (var s in ls) Error(s); }
        public static void Warning(string s, bool lineBreak = false)
        {
            lock (Out.Lock)
            {
                Out.Echo($"{Colors.Warning}{s}{Colors.Default}", lineBreak);
            }
        }

        public static string Readln(string prompt = null)
        {
            lock (Out.Lock)
            {
                if (prompt != null) Out.Echo(prompt);
            }
            return sc.ReadLine();
        }

        public static void Infos()
        {
            Out.Locked(() =>
            {
                Out.Echoln($"OS={Environment.OSVersion} {(Environment.Is64BitOperatingSystem ? "64" : "32")}bits");
                Out.Echoln($"{White}{Bkf}{Colors.HighlightIdentifier}window:{Rsf} left={Colors.Numeric}{sc.WindowLeft}{Rsf},top={Colors.Numeric}{sc.WindowTop}{Rsf},width={Colors.Numeric}{sc.WindowWidth}{Rsf},height={Colors.Numeric}{sc.WindowHeight}{Rsf},largest width={Colors.Numeric}{sc.LargestWindowWidth}{Rsf},largest height={Colors.Numeric}{sc.LargestWindowHeight}{Rsf}");
                Out.Echoln($"{Colors.HighlightIdentifier}buffer:{Rsf} width={Colors.Numeric}{sc.BufferWidth}{Rsf},height={Colors.Numeric}{sc.BufferHeight}{Rsf} | input encoding={Colors.Numeric}{sc.InputEncoding.EncodingName}{Rsf} | output encoding={Colors.Numeric}{sc.OutputEncoding.EncodingName}{Rsf}");
                Out.Echoln($"{White}default background color={Bkf}{Colors.KeyWord}{DefaultBackground}{Rsf} | default foreground color={Colors.KeyWord}{DefaultForeground}{Rsf}");
                if (RuntimeEnvironment.OSType == OSPlatform.Windows)
                {
                    Out.Echoln($"number lock={Colors.Numeric}{sc.NumberLock}{Rsf} | capslock={Colors.Numeric}{sc.CapsLock}{Rsf}");            // TODO: not supported on linux ubuntu 18.04 wsl
                    Out.Echoln($"cursor visible={Colors.Numeric}{sc.CursorVisible}{Rsf} | cursor size={Colors.Numeric}{sc.CursorSize}");     // TODO: not supported on linux ubuntu 18.04 wsl
                }
            });
        }

        public static object ExecCSharp(string csharpText)
        {
            try
            {
                var scriptKey = csharpText;
                if (!_csscripts.TryGetValue(scriptKey, out var script))
                {
                    script = CSharpScript.Create<object>(csharpText);
                    script.Compile();
                    _csscripts[scriptKey] = script;
                }
                var res = script.RunAsync();
                return res.Result.ReturnValue;
            }
            catch (CompilationErrorException ex)
            {
                LogError($"{csharpText}");
                LogError(string.Join(Environment.NewLine, ex.Diagnostics));
                return null;
            }
        }

        public static void Exit(int r = 0) => Environment.Exit(r);

        #region work area operations

        



        /// <summary>
        /// this setting limit wide of lines (available width -1) to prevent sys console to automatically put a line break when reaching end of line (console bug ?)
        /// </summary>
        public static bool AvoidConsoleAutoLineBreakAtEndOfLine = false;

    /// <summary>
    /// true until the contrary is detected (exception in GetCoords : sc.WindowLeft)
    /// </summary>
    /// <value></value>
        static bool IsConsoleGeometryEnabled = true;

        public static bool CheckConsoleHasGeometry() {
            try {
                var x = sc.WindowLeft;
            } catch (Exception) {
                IsConsoleGeometryEnabled = false;
                return false;
            }
            return true;
        }

        public static (int x, int y, int w, int h) GetCoords(int x, int y, int w, int h, bool fitToVisibleArea = true)
        {
            // (1) dos console (eg. vs debug consolehep) set WindowTop as y scroll position. WSL console doesn't (still 0)
            // scroll -> native dos console set WindowTop and WindowLeft as base scroll coordinates
            // if WorkArea defined, we must use absolute coordinates and not related
            // CursorLeft and CursorTop are always good
            lock (ConsoleLock)
            {
                if (!CheckConsoleHasGeometry()) return (x,y,1000,1000);

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

        public static ActualWorkArea ActualWorkArea(bool fitToVisibleArea = true)
        {
            var x0 = _workArea.Rect.IsEmpty ? 0 : _workArea.Rect.X;
            var y0 = _workArea.Rect.IsEmpty ? 0 : _workArea.Rect.Y;
            var w0 = _workArea.Rect.IsEmpty ? -1 : _workArea.Rect.Width;
            var h0 = _workArea.Rect.IsEmpty ? -1 : _workArea.Rect.Height;
            var (x, y, w, h) = GetCoords(x0, y0, w0, h0, fitToVisibleArea);
            return new ActualWorkArea(_workArea.Id, x, y, w, h);
        }



        public static void SetCursorAtWorkAreaTop()
        {
            if (_workArea.Rect.IsEmpty) return;     // TODO: set cursor even if workarea empty?
            lock (Out.Lock)
            {
                Out.SetCursorPos(_workArea.Rect.X, _workArea.Rect.Y);
            }
        }

        #endregion

        #region UI operations

        public static void FixCoords(ref int x, ref int y)
        {
            lock (ConsoleLock)
            {
                x = Math.Max(0, Math.Min(x, sc.BufferWidth - 1));
                y = Math.Max(0, Math.Min(y, sc.BufferHeight - 1));
            }
        }

        #endregion

        #region stream methods

        public static void RedirectOut(StreamWriter sw)
        {
            if (sw != null)
            {
                Out.Redirect(sw);
                _outputWriter = sc.Out;
                sc.SetOut(sw);
                IsOutputRedirected = true;
            } else
            {
                Out.Redirect((TextWriter)null);
                sc.SetOut(_outputWriter);
                _outputWriter = null;
                IsOutputRedirected = false;
            }
        }

        public static void RedirectErr(TextWriter sw)
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

        public static void RedirectOut(string filepath = null)
        {
            if (filepath!=null)
            {
                _outputWriter = sc.Out;
                _outputFileStream = new FileStream(filepath, FileMode.Append, FileAccess.Write);
                _outputStreamWriter = new StreamWriter(_outputFileStream);
                sc.SetOut(_outputStreamWriter);
                Out.Redirect(_outputStreamWriter);
            } else
            {
                _outputStreamWriter.Flush();
                _outputStreamWriter.Close();
                _outputStreamWriter = null;
                sc.SetOut(_outputWriter);
                _outputWriter = null;
                Out.Redirect((string)null);
            }
        }

        public static void RedirectErr(string filepath = null)
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

        #region folders

        //public static string TempPath => Path.Combine( Environment.CurrentDirectory , "Temp" );

        #endregion

        #region implementation methods

        public static string GetCmd(string cmd, string value = null)
        {
            if (value != null)
                return $"{CommandBlockBeginChar}{cmd}{CommandValueAssignationChar}{value}{CommandBlockEndChar}";
            return $"{CommandBlockBeginChar}{cmd}{CommandBlockEndChar}";
        }

        public static string GetCmd(EchoDirectives cmd, string value = null)
        {
            if (value != null)
                return $"{CommandBlockBeginChar}{cmd}{CommandValueAssignationChar}{value}{CommandBlockEndChar}";
            return $"{CommandBlockBeginChar}{cmd}{CommandBlockEndChar}";
        }

        public static int GetCursorX(object x)
        {
            if (x != null && x is string s && !string.IsNullOrWhiteSpace(s)
                && int.TryParse(s, out var v))
                return v;
            if (TraceCommandErrors) LogError($"wrong cursor x: {x}");
            lock (Out.Lock)
            {
                return sc.CursorLeft;
            }
        }

        public static int GetCursorY(object x)
        {
            if (x != null && x is string s && !string.IsNullOrWhiteSpace(s)
                && int.TryParse(s, out var v))
                return v;
            if (TraceCommandErrors) LogError($"wrong cursor y: {x}");
            lock (Out.Lock)
            {
                return sc.CursorTop;
            }
        }

        #endregion

        #region commands shortcuts

        public static string Clleft => GetCmd(EchoDirectives.clleft);
        public static string Clright => GetCmd(EchoDirectives.clright);
        public static string Fillright => GetCmd(EchoDirectives.fillright);
        public static string Cl => GetCmd(EchoDirectives.cl);
        public static string Chome => GetCmd(EchoDirectives.chome);

        public static string Lion => GetCmd(EchoDirectives.lion);
        public static string Bon => GetCmd(EchoDirectives.bon);
        public static string Blon => GetCmd(EchoDirectives.blon);

        public static string Cleft => GetCmd(EchoDirectives.cleft);
        public static string Cright => GetCmd(EchoDirectives.cright);
        public static string Cup => GetCmd(EchoDirectives.cup);
        public static string Cdown => GetCmd(EchoDirectives.cdown);
        public static string Cnleft(int n) => GetCmd(EchoDirectives.cleft+"",n+"");
        public static string Cnright(int n) => GetCmd(EchoDirectives.cright + "", n + "");
        public static string Cnup(int n) => GetCmd(EchoDirectives.cup + "", n + "");
        public static string Cndown(int n) => GetCmd(EchoDirectives.cdown + "", n + "");

        public static string Invon => GetCmd(EchoDirectives.invon);
        public static string Uon => GetCmd(EchoDirectives.uon);
        public static string Tdoff => GetCmd(EchoDirectives.tdoff);
        
        public static string DefaultBackgroundCmd => GetCmd(EchoDirectives.b + "", DefaultBackground.ToString().ToLower());
        public static string DefaultForegroundCmd => GetCmd(EchoDirectives.f + "", DefaultForeground.ToString().ToLower());
        public static string Rdc => GetCmd(EchoDirectives.rdc);

        public static string Bblack => GetCmd(EchoDirectives.b+"", "black");
        public static string Bdarkblue => GetCmd(EchoDirectives.b , "darkblue");
        public static string Bdarkgreen => GetCmd(EchoDirectives.b , "darkgreen");
        public static string Bdarkcyan => GetCmd(EchoDirectives.b , "darkcyan");
        public static string Bdarkred => GetCmd(EchoDirectives.b , "darkred");
        public static string Bdarkmagenta => GetCmd(EchoDirectives.b , "darkmagenta");
        public static string Bdarkyellow => GetCmd(EchoDirectives.b , "darkyellow");
        public static string Bgray => GetCmd(EchoDirectives.b , "gray");
        public static string Bdarkgray => GetCmd(EchoDirectives.b , "darkgray");
        public static string Bblue => GetCmd(EchoDirectives.b , "blue");
        public static string Bgreen => GetCmd(EchoDirectives.b , "green");
        public static string Bcyan => GetCmd(EchoDirectives.b , "cyan");
        public static string Bred => GetCmd(EchoDirectives.b , "red");
        public static string Bmagenta => GetCmd(EchoDirectives.b , "magenta");
        public static string Byellow => GetCmd(EchoDirectives.b , "yellow");
        public static string Bwhite => GetCmd(EchoDirectives.b , "white");
        public static string Black => GetCmd(EchoDirectives.f , "black");
        public static string Darkblue => GetCmd(EchoDirectives.f , "darkblue");
        public static string Darkgreen => GetCmd(EchoDirectives.f , "darkgreen");
        public static string Darkcyan => GetCmd(EchoDirectives.f , "darkcyan");
        public static string Darkred => GetCmd(EchoDirectives.f , "darkred");
        public static string Darkmagenta => GetCmd(EchoDirectives.f , "darkmagenta");
        public static string Darkyellow => GetCmd(EchoDirectives.f , "darkyellow");
        public static string Gray => GetCmd(EchoDirectives.f , "gray");
        public static string Darkgray => GetCmd(EchoDirectives.f , "darkgray");
        public static string Blue => GetCmd(EchoDirectives.f , "blue");
        public static string Green => GetCmd(EchoDirectives.f , "green");
        public static string Cyan => GetCmd(EchoDirectives.f , "cyan");
        public static string Red => GetCmd(EchoDirectives.f , "red");
        public static string Magenta => GetCmd(EchoDirectives.f , "magenta");
        public static string Yellow => GetCmd(EchoDirectives.f , "yellow");
        public static string White => GetCmd(EchoDirectives.f , "white");

        public static string Bkf => GetCmd(EchoDirectives.bkf );
        public static string Rsf => GetCmd(EchoDirectives.rsf );
        public static string Bkb => GetCmd(EchoDirectives.bkb );
        public static string Rsb => GetCmd(EchoDirectives.rsb );
        public static string Cls => GetCmd(EchoDirectives.cls );
        public static string Br => GetCmd(EchoDirectives.br );

        public static string B(ConsoleColor c) => GetCmd(EchoDirectives.b , c+"");
        public static string B8(ConsoleColor c) => GetCmd(EchoDirectives.b8 , c+"");
        public static string B24(ConsoleColor c) => GetCmd(EchoDirectives.b24 , c+"");

        public static string F(ConsoleColor c) => GetCmd(EchoDirectives.f , c+"");
        public static string F8(ConsoleColor c) => GetCmd(EchoDirectives.f8 , c+"");
        public static string F24(ConsoleColor c) => GetCmd(EchoDirectives.f24 , c+"");

        public static string Bkcr => GetCmd(EchoDirectives.bkcr );
        public static string Rscr => GetCmd(EchoDirectives.rscr );
        public static string Crx(int x) => GetCmd(EchoDirectives.crx , x +"");
        public static string Cry(int y) => GetCmd(EchoDirectives.cry , y +"");
        public static string Cr(int x, int y) => $"{GetCmd(EchoDirectives.crx , x +"" )}{GetCmd(EchoDirectives.cry , y+"" )}";

        public static string Exec(string csharpText) => GetCmd(EchoDirectives.exec , csharpText);

        public static string Tab => "".PadLeft(TabLength, ' ');

        #endregion

    }
}
