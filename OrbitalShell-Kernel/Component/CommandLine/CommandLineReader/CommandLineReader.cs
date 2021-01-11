//#define dbg

using DotNetConsoleAppToolkit.Component.CommandLine.Processor;
using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static DotNetConsoleAppToolkit.Component.CommandLine.Processor.CommandLineProcessor;
using static DotNetConsoleAppToolkit.DotNetConsole;
using sc = System.Console;

namespace DotNetConsoleAppToolkit.Component.CommandLine.CommandLineReader
{
    public class CommandLineReader
    {
        #region attributes

        public delegate ExpressionEvaluationResult ExpressionEvaluationCommandDelegate(CommandEvaluationContext context, string com,int outputX);

        Thread _inputReaderThread;
        
        string _prompt;
        StringBuilder _inputReaderStringBuilder;
        Point _beginOfLineCurPos;
        ExpressionEvaluationCommandDelegate _evalCommandDelegate;
        string _sentInput = null;
        bool _waitForReaderExited;
        bool _readingStarted;
        string _nextPrompt = null;
        string _defaultPrompt = null;
        readonly CommandLineProcessor CommandLineProcessor;
        bool _ignoreNextKey = false;
        
        public Action<IAsyncResult> InputProcessor { get; set; }

        #endregion

        #region initialization operations

        public CommandLineReader(
            CommandLineProcessor commandLineProcessor = null,
            string prompt = null,
            ExpressionEvaluationCommandDelegate evalCommandDelegate = null)
        {
            CommandLineProcessor = commandLineProcessor;
            if (CommandLineProcessor!=null && CommandLineProcessor!=null) CommandLineProcessor.CommandLineReader = this;
            _defaultPrompt = prompt ?? $"{Green}> {White}";
            Initialize(evalCommandDelegate);
        }

        public void SetPrompt(string prompt=null)
        {
            _nextPrompt = prompt ?? _defaultPrompt;
            _defaultPrompt = _nextPrompt;
        }

        public string GetPrompt() => _prompt;

        void Initialize(ExpressionEvaluationCommandDelegate evalCommandDelegate = null)
        {
            if (evalCommandDelegate==null && CommandLineProcessor!=null) _evalCommandDelegate = CommandLineProcessor.Eval;
            /*ViewSizeChanged += (o, e) =>
            {
                if (_inputReaderThread != null)
                {
                    lock (ConsoleLock)
                    {
                        Out.Echo(_prompt);
                        _beginOfLineCurPos = Out.CursorPos;
                        Out.ConsolePrint(_inputReaderStringBuilder.ToString());
                    }
                }
            };
            WorkAreaScrolled += (o, e) =>
            {
                if (_inputReaderThread != null)
                {
                    lock (ConsoleLock)
                    {
                        _beginOfLineCurPos.X += e.DeltaX;
                        _beginOfLineCurPos.Y += e.DeltaY;
                        var p = Out.CursorPos;
                        var (id,left, top, width, height) = ActualWorkArea();
                        var txt = _inputReaderStringBuilder.ToString();
                        if (!string.IsNullOrWhiteSpace(txt))
                        {
                            var index = Out.GetIndexInWorkAreaConstraintedString(txt, _beginOfLineCurPos, p);
                            var slines = Out.GetWorkAreaStringSplits(txt, _beginOfLineCurPos).Splits;

                            if (Out.CursorTop == slines.Min(o => o.Y))
                            {
                                Out.CursorLeft = left;
                                Out.Echo(_prompt);
                            }
                            var enableConstraintConsolePrintInsideWorkArea = EnableConstraintConsolePrintInsideWorkArea;
                            EnableConstraintConsolePrintInsideWorkArea = false;
                            foreach (var sline in slines)
                                if (sline.Y >= top && sline.Y <= height)
                                {
                                    Out.SetCursorPos(sline.X, sline.Y);
                                    Out.ConsolePrint("".PadLeft(width - sline.X, ' '));
                                    Out.SetCursorPos(sline.X, sline.Y);
                                    Out.ConsolePrint(sline.Text);
                                }
                            EnableConstraintConsolePrintInsideWorkArea = enableConstraintConsolePrintInsideWorkArea;
                            Out.SetCursorPos(p);
                        }
                    }
                }
            };*/
        }

        #endregion

        #region input processing

        void ProcessInput(IAsyncResult asyncResult)
        {
            var s = (string)asyncResult.AsyncState;
            ProcessCommandLine(s, _evalCommandDelegate, true, true);
        }

        public void ProcessCommandLine(
            string commandLine, 
            ExpressionEvaluationCommandDelegate evalCommandDelegate,
            bool outputStartNextLine = false,
            bool enableHistory = false)
        {
            if (commandLine != null)
            {
                if (outputStartNextLine) Out.LineBreak();

                ExpressionEvaluationResult expressionEvaluationResult = null;

                try
                {
                    sc.CancelKeyPress += CancelKeyPress;
                    CommandLineProcessor.CancellationTokenSource = new CancellationTokenSource();
                    var task = Task.Run<ExpressionEvaluationResult>(
                        () => evalCommandDelegate(CommandLineProcessor.CommandEvaluationContext, commandLine, _prompt == null ? 0 : Out.GetPrint(_prompt).Length),
                        CommandLineProcessor.CancellationTokenSource.Token
                        );

                    try
                    {
                        try
                        {
                            task.Wait(CommandLineProcessor.CancellationTokenSource.Token);
                        } catch (ThreadInterruptedException) {
                            // get interrupted after send input
                        }
                        expressionEvaluationResult = task.Result;
                    }
                    catch (OperationCanceledException)
                    {
                        var res = task.Result;
                        Warningln($"command canceled: {commandLine}");
                    }
                    finally
                    {
                        
                    }                    
                }
                catch (Exception ex)
                {
                    LogError(ex);                        
                }
                finally
                {
                    CommandLineProcessor.CancellationTokenSource.Dispose();
                    CommandLineProcessor.CancellationTokenSource = null;
                    sc.CancelKeyPress -= CancelKeyPress;
                    lock (ConsoleLock)
                    {
                        /*if (!WorkArea.rect.IsEmpty && (WorkArea.rect.Y != CursorTop || WorkArea.rect.X != CursorLeft))
                            LineBreak();*/      // case of auto line break (spacing)
                        
                        //Out.RestoreDefaultColors();   // FGZ removed
                    }
                }
            }
            if (enableHistory && !string.IsNullOrWhiteSpace(commandLine))
                CommandLineProcessor.CmdsHistory.HistoryAppend(commandLine);
        }

        private void CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            CommandLineProcessor.CancellationTokenSource?.Cancel();
        }

        public int ReadCommandLine(
            string prompt=null, 
            bool waitForReaderExited = true
            )
        {
            prompt ??= _defaultPrompt;
            InputProcessor ??= ProcessInput;
            return BeginReadln(new AsyncCallback(InputProcessor), prompt, waitForReaderExited);
        }

        public void SendInput(string text,bool sendEnter=true)
        {
            _sentInput = text + ((sendEnter)?Environment.NewLine:"");
            if (_inputReaderThread == null) return;
            StopBeginReadln();
            InputProcessor ??= ProcessInput;
            BeginReadln(new AsyncCallback(InputProcessor), _prompt, _waitForReaderExited);
        }

        public void IgnoreNextKey() { _ignoreNextKey = true; }

        public void SendNextInput(string text, bool sendEnter = true)
        {
            _sentInput = text + ((sendEnter) ? Environment.NewLine : "");
            _readingStarted = false;
        }

        public int BeginReadln(
            AsyncCallback asyncCallback, 
            string prompt = null,
            bool waitForReaderExited = true,
            bool loop=true
            )
        {            
            _waitForReaderExited = waitForReaderExited;
            prompt ??= _defaultPrompt;
            _prompt = prompt;
            bool noWorkArea = !InWorkArea;
            _inputReaderThread = new Thread(() =>
            {
                try
                {
                    var isRunning = true;
                    while (isRunning)
                    {
                        if (!loop)
                            isRunning = false;
                        _inputReaderStringBuilder ??= new StringBuilder();
                        if (!_readingStarted)
                        {
                            lock (ConsoleLock)
                            {
                                Out.Echo(prompt);
                                _beginOfLineCurPos = Out.CursorPos;
                            }
                            _readingStarted = true;
                        }
                        var eol = false;
                        while (!eol)
                        {
                            ConsoleKeyInfo c;
                            var printed = false;
                            string printedStr = "";
                            var (id, left, top, right, bottom) = ActualWorkArea();

                            if (sc.IsInputRedirected)
                            {
                                _sentInput = sc.In.ReadToEnd();
                                isRunning = false;
                            }

                            if (_sentInput == null)
                            {
                                c = sc.ReadKey(true);
#if dbg
                                System.Diagnostics.Debug.WriteLine($"{c.KeyChar}={c.Key}");
#endif
                                #region handle special keys - edition mode, movement

                                if (!_ignoreNextKey)
                                {
                                    (id, left, top, right, bottom) = ActualWorkArea();

                                    switch (c.Key)
                                    {
                                        case ConsoleKey.Enter:
                                            eol = true;
                                            break;
                                        case ConsoleKey.Escape:
                                            Out.HideCur();
                                            CleanUpReadln();
                                            Out.ShowCur();
                                            break;
                                        case ConsoleKey.Home:
                                            lock (ConsoleLock)
                                            {
                                                Out.SetCursorPosConstraintedInWorkArea(_beginOfLineCurPos);
                                            }
                                            break;
                                        case ConsoleKey.End:
                                            lock (ConsoleLock)
                                            {
                                                var slines = Out.GetWorkAreaStringSplits(_inputReaderStringBuilder.ToString(), _beginOfLineCurPos);
                                                var sline = slines.Splits.Last();
                                                Out.SetCursorPosConstraintedInWorkArea(sline.X + sline.Length, sline.Y);
                                            }
                                            break;
                                        case ConsoleKey.Tab:
                                            lock (ConsoleLock)
                                            {
                                                printedStr = "".PadLeft(TabLength, ' ');
                                                printed = true;
                                            }
                                            break;
                                        case ConsoleKey.LeftArrow:
                                            lock (ConsoleLock)
                                            {
                                                var p = Out.CursorPos;
                                                if (p.Y == _beginOfLineCurPos.Y)
                                                {
                                                    if (p.X > _beginOfLineCurPos.X)
                                                        Out.CursorLeft = p.X - 1;
                                                }
                                                else
                                                {
                                                    var x = p.X - 1;
                                                    if (x < left)
                                                        Out.SetCursorPosConstraintedInWorkArea(right - 1, p.Y - 1);
                                                    else
                                                        Out.CursorLeft = x;
                                                }
                                            }
                                            break;
                                        case ConsoleKey.RightArrow:
                                            lock (ConsoleLock)
                                            {
                                                var txt = _inputReaderStringBuilder.ToString();
                                                var index = Out.GetIndexInWorkAreaConstraintedString(txt, _beginOfLineCurPos, Out.CursorPos);
                                                if (index < txt.Length)
                                                    Out.SetCursorPosConstraintedInWorkArea(Out.CursorLeft + 1, Out.CursorTop);
                                            }
                                            break;
                                        case ConsoleKey.Backspace:
                                            lock (ConsoleLock)
                                            {
                                                var txt = _inputReaderStringBuilder.ToString();
                                                var index = Out.GetIndexInWorkAreaConstraintedString(txt, _beginOfLineCurPos, Out.CursorPos) - 1;
                                                var x = Out.CursorLeft - 1;
                                                var y = Out.CursorTop;
                                                if (index >= 0)
                                                {
                                                    _inputReaderStringBuilder.Remove(index, 1);
                                                    _inputReaderStringBuilder.Append(" ");
                                                    Out.HideCur();
                                                    Out.SetCursorPosConstraintedInWorkArea(ref x, ref y);
                                                    var slines = Out.GetWorkAreaStringSplits(_inputReaderStringBuilder.ToString(), _beginOfLineCurPos).Splits;
                                                    var enableConstraintConsolePrintInsideWorkArea = EnableConstraintConsolePrintInsideWorkArea;
                                                    EnableConstraintConsolePrintInsideWorkArea = false;
                                                    foreach (var sline in slines)
                                                        if (sline.Y >= top && sline.Y <= bottom)
                                                        {
                                                            Out.SetCursorPos(sline.X, sline.Y);
                                                            Out.ConsolePrint("".PadLeft(right - sline.X, ' '));
                                                            Out.SetCursorPos(sline.X, sline.Y);
                                                            Out.ConsolePrint(sline.Text);
                                                        }
                                                    _inputReaderStringBuilder.Remove(_inputReaderStringBuilder.Length - 1, 1);
                                                    EnableConstraintConsolePrintInsideWorkArea = enableConstraintConsolePrintInsideWorkArea;
                                                    Out.SetCursorPos(x, y);
                                                    Out.ShowCur();
                                                }
                                            }
                                            break;
                                        case ConsoleKey.Delete:
                                            lock (ConsoleLock)
                                            {
                                                var txt = _inputReaderStringBuilder.ToString();
                                                var index = Out.GetIndexInWorkAreaConstraintedString(txt, _beginOfLineCurPos, Out.CursorPos);
                                                var x = Out.CursorLeft;
                                                var y = Out.CursorTop;
                                                if (index >= 0 && index < txt.Length)
                                                {
                                                    _inputReaderStringBuilder.Remove(index, 1);
                                                    _inputReaderStringBuilder.Append(" ");
                                                    Out.HideCur();
                                                    Out.SetCursorPosConstraintedInWorkArea(ref x, ref y);
                                                    var slines = Out.GetWorkAreaStringSplits(_inputReaderStringBuilder.ToString(), _beginOfLineCurPos).Splits;
                                                    var enableConstraintConsolePrintInsideWorkArea = EnableConstraintConsolePrintInsideWorkArea;
                                                    EnableConstraintConsolePrintInsideWorkArea = false;
                                                    foreach (var sline in slines)
                                                        if (sline.Y >= top && sline.Y <= bottom)
                                                        {
                                                            Out.SetCursorPos(sline.X, sline.Y);
                                                            Out.ConsolePrint("".PadLeft(right - sline.X, ' '));
                                                            Out.SetCursorPos(sline.X, sline.Y);
                                                            Out.ConsolePrint(sline.Text);
                                                        }
                                                    _inputReaderStringBuilder.Remove(_inputReaderStringBuilder.Length - 1, 1);
                                                    EnableConstraintConsolePrintInsideWorkArea = enableConstraintConsolePrintInsideWorkArea;
                                                    Out.SetCursorPos(x, y);
                                                    Out.ShowCur();
                                                }
                                            }
                                            break;
                                        case ConsoleKey.UpArrow:
                                            lock (ConsoleLock)
                                            {
                                                if (Out.CursorTop == _beginOfLineCurPos.Y)
                                                {
                                                    var h = CommandLineProcessor.CmdsHistory.GetBackwardHistory();
                                                    if (h != null)
                                                    {
                                                        Out.HideCur();
                                                        CleanUpReadln();
                                                        _inputReaderStringBuilder.Append(h);
                                                        Out.ConsolePrint(h);
                                                        Out.ShowCur();
                                                    }
                                                }
                                                else
                                                {
                                                    Out.SetCursorPosConstraintedInWorkArea(
                                                        (Out.CursorTop - 1) == _beginOfLineCurPos.Y ?
                                                            Math.Max(_beginOfLineCurPos.X, Out.CursorLeft) : Out.CursorLeft,
                                                        Out.CursorTop - 1);
                                                }
                                            }
                                            break;
                                        case ConsoleKey.DownArrow:
                                            lock (ConsoleLock)
                                            {
                                                var slines = Out.GetWorkAreaStringSplits(_inputReaderStringBuilder.ToString(), _beginOfLineCurPos).Splits;
                                                if (Out.CursorTop == slines.Max(o => o.Y))
                                                {
                                                    var fh = CommandLineProcessor.CmdsHistory.GetForwardHistory();
                                                    if (fh != null)
                                                    {
                                                        Out.HideCur();
                                                        CleanUpReadln();
                                                        _inputReaderStringBuilder.Append(fh);
                                                        Out.ConsolePrint(fh);
                                                        Out.ShowCur();
                                                    }
                                                }
                                                else
                                                {
                                                    var sline = slines.Where(o => o.Y == Out.CursorTop + 1).FirstOrDefault();
                                                    // BUG: ici sline est null
                                                    if (sline.Text!=null)
                                                        Out.SetCursorPosConstraintedInWorkArea(Math.Min(Out.CursorLeft, sline.X + sline.Length), Out.CursorTop + 1);
                                                }
                                            }
                                            break;
                                        default:
                                            printedStr = c.KeyChar + "";
                                            printed = true;
                                            break;
                                    }
                                }
                                else 
                                    _ignoreNextKey = false;

                            #endregion
                            }
                            else
                            {
                                printedStr = _sentInput;
                                _sentInput = null;
                                printed = true;
                                eol = printedStr.EndsWith(Environment.NewLine);
                                if (eol) printedStr = printedStr.Trim();
                            }

                            if (printed)
                            {
                                var index = 0;
                                var insert = false;
                                lock (ConsoleLock)
                                {
                                    var x0 = Out.CursorLeft;
                                    var y0 = Out.CursorTop;
                                    var txt = _inputReaderStringBuilder.ToString();
                                    index = Out.GetIndexInWorkAreaConstraintedString(txt, _beginOfLineCurPos, x0, y0);
                                    insert = index - txt.Length < 0;

                                    if (insert)
                                    {
                                        Out.HideCur();
                                        var x = x0;
                                        var y = y0;
                                        Out.SetCursorPosConstraintedInWorkArea(ref x, ref y);
                                        _inputReaderStringBuilder.Insert(index, printedStr);
                                        var slines = Out.GetWorkAreaStringSplits(_inputReaderStringBuilder.ToString(), _beginOfLineCurPos).Splits;
                                        var enableConstraintConsolePrintInsideWorkArea = EnableConstraintConsolePrintInsideWorkArea;
                                        EnableConstraintConsolePrintInsideWorkArea = false;
                                        foreach (var sline in slines)
                                            if (sline.Y >= top && sline.Y <= bottom)
                                            {
                                                Out.SetCursorPos(sline.X, sline.Y);
                                                Out.ConsolePrint(sline.Text);
                                            }
                                        EnableConstraintConsolePrintInsideWorkArea = enableConstraintConsolePrintInsideWorkArea;
                                        x += printedStr.Length;
                                        Out.SetCursorPosConstraintedInWorkArea(ref x, ref y);
                                        Out.ShowCur();
                                    }
                                    if (!insert)
                                    {
                                        _inputReaderStringBuilder.Append(printedStr);
                                        Out.ConsolePrint(printedStr, false);
                                    }
                                }
                            }

                            if (eol) break;
                        }

                        // process input
                        var s = _inputReaderStringBuilder.ToString();
                        _inputReaderStringBuilder.Clear();

                        var _enableConstraintConsolePrintInsideWorkArea = EnableConstraintConsolePrintInsideWorkArea;
                        if (noWorkArea)
                            EnableConstraintConsolePrintInsideWorkArea = false;

                        asyncCallback?.Invoke(
                            new BeginReadlnAsyncResult(s)
                            );

                        if (noWorkArea)
                            EnableConstraintConsolePrintInsideWorkArea = _enableConstraintConsolePrintInsideWorkArea;

                        _readingStarted = false;
                        if (_nextPrompt!=null)
                        {
                            prompt = _prompt = _nextPrompt;
                            _nextPrompt = null;
                        }
                    }
                }
                catch (ThreadInterruptedException) { 
                }
                catch (Exception ex)
                {
                    LogException(ex,"input stream reader crashed");
                }
            })
            {
                Name = "input stream reader"
            };
            _inputReaderThread.Start();
            if (waitForReaderExited) _inputReaderThread.Join();
            return (int)ReturnCode.OK;
        }

        public void WaitReadln()
        {
            _inputReaderThread?.Join();
        }

        public void CleanUpReadln()
        {
            if (_inputReaderThread != null)
            {
                lock (ConsoleLock)
                {
                    var (id,left, top, right, bottom) = ActualWorkArea();
                    Out.SetCursorPosConstraintedInWorkArea(_beginOfLineCurPos);
                    var txt = _inputReaderStringBuilder.ToString();
                    var slines = Out.GetWorkAreaStringSplits(txt, _beginOfLineCurPos).Splits;
                    var enableConstraintConsolePrintInsideWorkArea = EnableConstraintConsolePrintInsideWorkArea;
                    EnableConstraintConsolePrintInsideWorkArea = false;
                    foreach (var sline in slines)
                        if (sline.Y>=top && sline.Y<= bottom)
                        {
                            Out.SetCursorPos(sline.X, sline.Y);
                            Out.ConsolePrint("".PadLeft(right - sline.X, ' '));
                        }
                    EnableConstraintConsolePrintInsideWorkArea = enableConstraintConsolePrintInsideWorkArea;
                    Out.SetCursorPosConstraintedInWorkArea(_beginOfLineCurPos);
                    _inputReaderStringBuilder.Clear();
                }
            }
        }

        public void StopBeginReadln()
        {
            _inputReaderThread?.Interrupt();
            _inputReaderThread = null;
            _readingStarted = false;
        }

        #endregion        
    }
}
