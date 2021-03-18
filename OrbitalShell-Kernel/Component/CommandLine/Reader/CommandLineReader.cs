//#define dbg
#define FIX_LOW_ANSI

using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.ObjectPool;

using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Console;
using OrbitalShell.Component.Shell;
using OrbitalShell.Component.Shell.Hook;
using OrbitalShell.Component.Shell.Variable;
using OrbitalShell.Lib;

using sc = System.Console;

namespace OrbitalShell.Component.CommandLine.Reader
{
    public class CommandLineReader : ICommandLineReader
    {
        #region attributes

        Thread _inputReaderThread;
        string _prompt;
        StringBuilder _inputReaderStringBuilder;
        Point _beginOfLineCurPos;
        Delegates.ExpressionEvaluationCommandDelegate _evalCommandDelegate;
        string _sentInput = null;
        bool _waitForReaderExited;
        bool _readingStarted;
        string _nextPrompt = null;
        string _defaultPrompt = null;
        ICommandLineProcessor _commandLineProcessor;
        bool _ignoreNextKey = false;

        public Action<IAsyncResult> InputProcessor { get; set; }

        public IConsole Console { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Supprimer les membres privés non lus", Justification = "<En attente>")]
        static int _instanceId = 0;
        readonly ObjectPool<EventArgs<ConsoleKeyInfo>> _keyEventArgsPool = new DefaultObjectPool<EventArgs<ConsoleKeyInfo>>(
            new DefaultPooledObjectPolicy<EventArgs<ConsoleKeyInfo>>()
            );

        #endregion

        #region initialization operations

        public CommandLineReader( )
        {            
            _instanceId++;
#if DBG_DI_INSTANCE
            System.Console.Out.WriteLine($"new CLR #{_InstanceId}");
#endif                        
        }

        public void Initialize(
            string prompt = null,
            ICommandLineProcessor clp = null,
            Delegates.ExpressionEvaluationCommandDelegate evalCommandDelegate = null)
        {
            _defaultPrompt = prompt ?? $"> ";
            Console = clp.Console;
            _commandLineProcessor = clp;
            if (_commandLineProcessor != null && _commandLineProcessor != null) 
                _commandLineProcessor.CommandLineReader = this;
            Initialize(evalCommandDelegate);
        }

        public void SetDefaultPrompt(string prompt)
        {
            _defaultPrompt = prompt;
        }

        public void SetPrompt(string prompt = null)
        {
            _nextPrompt = prompt ?? _defaultPrompt;
            _defaultPrompt = _nextPrompt;
        }

        public void SetPrompt(CommandEvaluationContext context, string prompt)
        {
            SetPrompt(prompt);
            context.ShellEnv.SetValue(ShellEnvironmentVar.settings_console_prompt, prompt);
        }

        public string GetPrompt() => _prompt;

        void Initialize(Delegates.ExpressionEvaluationCommandDelegate evalCommandDelegate = null)
        {
            if (evalCommandDelegate == null && _commandLineProcessor != null) _evalCommandDelegate = _commandLineProcessor.Eval;

            #region disabled

#if manage_embeded_view
            ViewSizeChanged += (o, e) =>
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
            };
#endif
            #endregion
        }

        #endregion

        #region input processing

        void ProcessInput(IAsyncResult asyncResult)
        {
            var s = (string)asyncResult.AsyncState;
            ProcessCommandLine(s, _evalCommandDelegate, true, true/*TODO: , enablePrePostComOutput setting*/);
        }

        public void ProcessCommandLine(
            string commandLine,
            Delegates.ExpressionEvaluationCommandDelegate evalCommandDelegate,
            bool outputStartNextLine = false,
            bool enableHistory = false,
            bool enablePrePostComOutput = true)
        {
            var clp = _commandLineProcessor;

            if (commandLine != null)
            {
                if (outputStartNextLine)
                {
                    Console.Out.LineBreak();
                }
                ExpressionEvaluationResult expressionEvaluationResult = null;

                try
                {
                    sc.CancelKeyPress += CancelKeyPress;
                    clp.CancellationTokenSource = new CancellationTokenSource();
                    Console.Out.IsModified = false;
                    Console.Err.IsModified = false;

                    clp.ModuleManager.ModuleHookManager.InvokeHooks(
                        clp.CommandEvaluationContext, Hooks.PreProcessCommandLine, commandLine );

                    var task = Task.Run<ExpressionEvaluationResult>(
                        () => evalCommandDelegate(
                                clp.CommandEvaluationContext,
                                commandLine,
                                _prompt == null ? 0 : Console.Out.GetPrint(_prompt).Length,        // TODO has no sens with multi line prompt !!!
                                (enablePrePostComOutput && clp != null) ?
                                    clp.CommandEvaluationContext.ShellEnv.GetValue<string>(ShellEnvironmentVar.settings_clr_comPreAnalysisOutput) : ""),
                            clp.CancellationTokenSource.Token
                        );

                    try
                    {
                        try
                        {
                            task.Wait(clp.CancellationTokenSource.Token);       // TODO: not if {com} &
                        }
                        catch (ThreadInterruptedException)
                        {
                            // get interrupted after send input
                        }
                        expressionEvaluationResult = task.Result;
                    }
                    catch (OperationCanceledException)
                    {
                        clp.ModuleManager.ModuleHookManager.InvokeHooks<CommandLineReader>(
                            clp.CommandEvaluationContext, Hooks.ProcessCommandLineCanceled ) ;

                        expressionEvaluationResult = task.Result;
                        Console.Out.Warningln($"command canceled: {commandLine}");
                    }
                    finally { }
                }
                catch (Exception ex)
                {
                    clp.ModuleManager.ModuleHookManager.InvokeHooks(
                        clp.CommandEvaluationContext,Hooks.ProcessCommandLineError);
                    Console.LogError(ex);
                }
                finally
                {
                    clp.ModuleManager.ModuleHookManager
                        .InvokeHooks(clp.CommandEvaluationContext, Hooks.PostProcessCommandLine );

                    if (enablePrePostComOutput && clp != null)
                    {
                        if (Console.Out.IsModified || Console.Err.IsModified)
                        {
                            if (!(Console.Out.CursorLeft == 0 && Console.Out.CursorTop == 0))
                                Console.Out.Echo(clp.CommandEvaluationContext.ShellEnv.GetValue<string>(ShellEnvironmentVar.settings_clr_comPostExecOutModifiedOutput));
                        }
                        Console.Out.Echo(clp.CommandEvaluationContext.ShellEnv.GetValue<string>(ShellEnvironmentVar.settings_clr_comPostExecOutput));
                    }

                    clp.CancellationTokenSource.Dispose();
                    clp.CancellationTokenSource = null;
                    sc.CancelKeyPress -= CancelKeyPress;
                }
            }

            if (enableHistory && !string.IsNullOrWhiteSpace(commandLine))
                clp.CmdsHistory.HistoryAppend(clp.CommandEvaluationContext,commandLine);
        }

        private void CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            _commandLineProcessor.CancellationTokenSource?.Cancel();
        }

        public int ReadCommandLine(
            string prompt = null,
            bool waitForReaderExited = true
            )
        {
            prompt ??= _defaultPrompt;
            InputProcessor ??= ProcessInput;
            return BeginReadln(new AsyncCallback(InputProcessor), prompt, waitForReaderExited);
        }

        public void SendInput(string text, bool sendEnter = true, bool waitEndOfInput = false)
        {
            _sentInput = text + ((sendEnter) ? Environment.NewLine : "");
            if (_inputReaderThread == null) return;
            StopBeginReadln();
            InputProcessor ??= ProcessInput;
            BeginReadln(new AsyncCallback(InputProcessor), _prompt, _waitForReaderExited | waitEndOfInput);
        }

        public void IgnoreNextKey() { _ignoreNextKey = true; }

        public void SendNextInput(string text, bool sendEnter = true)
        {
            _sentInput = text + ((sendEnter) ? Environment.NewLine : "");
            _readingStarted = false;
        }

        /// <summary>
        /// begin input reader thread
        /// </summary>
        /// <param name="asyncCallback">reader async call back at end of input</param>
        /// <param name="prompt">prompt text</param>
        /// <param name="waitForReaderExited">if true wait for end of input reader</param>
        /// <param name="loop">mandatory for usage of SendInput</param>
        /// <returns>forwarded state result code from asyncCallback</returns>
        public int BeginReadln(
            AsyncCallback asyncCallback,
            string prompt = null,
            bool waitForReaderExited = true,
            bool loop = true
            )
        {
            _waitForReaderExited = waitForReaderExited;
            prompt ??= _defaultPrompt;
            _prompt = prompt;
            bool noWorkArea = !Console.InWorkArea;
            Point? lastInputPos = null;
            var hm = _commandLineProcessor?.ModuleManager?.ModuleHookManager;
            var context = _commandLineProcessor?.CommandEvaluationContext;
            if (context == null) throw new Exception("command line reader is badly initialized: context is null");

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
                            lock (Console.ConsoleLock)
                            {
                                hm?.InvokeHooks(context, Hooks.PromptOutputBegin);

                                var _beginPromptPos = Console.Out.CursorPos;
                                Console.Out.Echo(prompt);

                                hm?.InvokeHooks(context, Hooks.PromptOutputEnd);

                                _beginOfLineCurPos = Console.Out.CursorPos;
                                lastInputPos = _beginOfLineCurPos;
                                Console.Out.ConsoleCursorPosBackup();

#if FIX_LOW_ANSI    // TODO: check disabled (check the best)
                                if (context.ShellEnv.IsOptionSetted(ShellEnvironmentVar.settings_console_enableCompatibilityMode))
                                {
                                    Thread.Sleep(25);
                                    Console.Out.ConsoleCursorPosRestore();
                                }
#endif
                                _readingStarted = true;

                                context.CommandLineProcessor.ModuleManager.ModuleHookManager
                                    .InvokeHooks(context, Hooks.BeginReadCommandLine );
                            }
                        }
                        var eol = false;

                        try
                        {

                            while (!eol)
                            {
                                ConsoleKeyInfo c;
                                var printed = false;
                                string printedStr = "";
                                var (id, left, top, right, bottom) = Console.ActualWorkArea();

                                if (sc.IsInputRedirected)
                                {
                                    _sentInput = sc.In.ReadToEnd();
                                    isRunning = false;
                                }

                                if (_sentInput == null)
                                {
                                    c = sc.ReadKey(true);

                                    var keyPressedEventArgs = _keyEventArgsPool.Get();
                                    keyPressedEventArgs.Value = c;
                                    context.CommandLineProcessor.ModuleManager.ModuleHookManager
                                        .InvokeHooks(context, Hooks.ReadCommandLineKeyPressed, keyPressedEventArgs );                                    
                                    
                                    #if dbg
                                    System.Diagnostics.Debug.WriteLine($"{c.KeyChar}={c.Key}");
                                    #endif

                                    #region handle special keys - edition mode, movement

                                    if (!_ignoreNextKey && !keyPressedEventArgs.IsCanceled)
                                    {
                                        keyPressedEventArgs.IsCanceled = false;

                                        // normally the cursor has moved of 1 character right or left
                                        var cPos = Console.Out.CursorPos;
                                        if (lastInputPos.HasValue
                                            && lastInputPos.Value != cPos)
                                        {
                                            int dx = Math.Abs(cPos.X - lastInputPos.Value.X);
                                            int dy = Math.Abs(cPos.Y - lastInputPos.Value.Y);
                                            if (dx > 1 || dy > 1)
                                            {
                                                // restore the good position
                                                Console.Out.CursorPos =
                                                new Point(
                                                    lastInputPos.Value.X + 1,
                                                    lastInputPos.Value.Y);
                                            }
                                        }

                                        lastInputPos = Console.Out.CursorPos;

                                        (id, left, top, right, bottom) = Console.ActualWorkArea();

                                        void AcceptChar()
                                        {
                                            printedStr = c.KeyChar + "";
                                            printed = true;
                                        } 

                                        switch (c.Key)
                                        {
                                            // CR: default end of input
                                            case ConsoleKey.Enter:

                                                context.CommandLineProcessor.ModuleManager.ModuleHookManager
                                                    .InvokeHooks(context, Hooks.ReadCommandLineEnterPressed, keyPressedEventArgs);
                                                eol = true;

                                                break;

                                            // ESC : clean-up input and set cursor at begin of line (after prompt)
                                            case ConsoleKey.Escape:

                                                context.CommandLineProcessor.ModuleManager.ModuleHookManager
                                                    .InvokeHooks(context, Hooks.ReadCommandLineEscPressed, keyPressedEventArgs);

                                                CleanUpReadln();
                                                lastInputPos = _beginOfLineCurPos;

                                                break;

                                            // HOME : set cursor position at begin of input (just after prompt) 
                                            case ConsoleKey.Home:

                                                context.CommandLineProcessor.ModuleManager.ModuleHookManager
                                                    .InvokeHooks(context, Hooks.ReadCommandLineHomePressed, keyPressedEventArgs);

                                                lock (Console.ConsoleLock)
                                                {
                                                    Console.Out.SetCursorPosConstraintedInWorkArea(_beginOfLineCurPos);
                                                    lastInputPos = _beginOfLineCurPos;
                                                }
                                                break;

                                            // END : set cursor position at end of input
                                            case ConsoleKey.End:

                                                context.CommandLineProcessor.ModuleManager.ModuleHookManager
                                                    .InvokeHooks(context, Hooks.ReadCommandLineEndPressed, keyPressedEventArgs);

                                                lock (Console.ConsoleLock)
                                                {
                                                    var slines = Console.Out.GetWorkAreaStringSplits(_inputReaderStringBuilder.ToString(), _beginOfLineCurPos);
                                                    var sline = slines.Splits.Last();
                                                    Console.Out.SetCursorPosConstraintedInWorkArea(sline.X + sline.Length, sline.Y);
                                                    lastInputPos = Console.Out.CursorPos;
                                                }
                                                break;

                                            case ConsoleKey.Tab:

                                                context.CommandLineProcessor.ModuleManager.ModuleHookManager
                                                    .InvokeHooks(context, Hooks.ReadCommandLineTabPressed, keyPressedEventArgs);

                                                lock (Console.ConsoleLock)
                                                {
                                                    printedStr = "".PadLeft(Console.TabLength, ' ');
                                                    printed = true;
                                                }
                                                break;

                                            case ConsoleKey.LeftArrow:

                                                context.CommandLineProcessor.ModuleManager.ModuleHookManager
                                                    .InvokeHooks(context, Hooks.ReadCommandLineLeftArrowPressed, keyPressedEventArgs);

                                                lock (Console.ConsoleLock)
                                                {
                                                    var p = Console.Out.CursorPos;
                                                    if (p.Y == _beginOfLineCurPos.Y)
                                                    {
                                                        if (p.X > _beginOfLineCurPos.X)
                                                            Console.Out.CursorLeft = p.X - 1;
                                                    }
                                                    else
                                                    {
                                                        var x = p.X - 1;
                                                        if (x < left)
                                                            Console.Out.SetCursorPosConstraintedInWorkArea(right - 1, p.Y - 1);
                                                        else
                                                            Console.Out.CursorLeft = x;
                                                    }
                                                }
                                                break;

                                            case ConsoleKey.RightArrow:

                                                context.CommandLineProcessor.ModuleManager.ModuleHookManager
                                                    .InvokeHooks(context, Hooks.ReadCommandLineRightArrowPressed, keyPressedEventArgs);

                                                lock (Console.ConsoleLock)
                                                {
                                                    var txt = _inputReaderStringBuilder.ToString();
                                                    var index = Console.Out.GetIndexInWorkAreaConstraintedString(txt, _beginOfLineCurPos, Console.Out.CursorPos);
                                                    if (index < txt.Length)
                                                        Console.Out.SetCursorPosConstraintedInWorkArea(Console.Out.CursorLeft + 1, Console.Out.CursorTop);
                                                }
                                                break;

                                            // CTRL + / : transform \ to /
                                            case ConsoleKey.Divide:

                                                if (c.Modifiers.HasFlag(ConsoleModifiers.Control))
                                                {
                                                    lock (Console.ConsoleLock)
                                                    {
                                                        var txt = _inputReaderStringBuilder.ToString().Replace('\\', '/');
                                                        CleanUpReadln();
                                                        _inputReaderStringBuilder.Append(txt);
                                                        Console.Out.Echo(txt, false, true);
                                                    }
                                                }
                                                else
                                                    AcceptChar();

                                                break;

                                            case ConsoleKey.Backspace:

                                                context.CommandLineProcessor.ModuleManager.ModuleHookManager
                                                    .InvokeHooks(context, Hooks.ReadCommandLineBackspacePressed, keyPressedEventArgs);

                                                lock (Console.ConsoleLock)
                                                {
                                                    var txt = _inputReaderStringBuilder.ToString();
                                                    var index = Console.Out.GetIndexInWorkAreaConstraintedString(txt, _beginOfLineCurPos, Console.Out.CursorPos) - 1;
                                                    var x = Console.Out.CursorLeft - 1;
                                                    var y = Console.Out.CursorTop;
                                                    if (index >= 0)
                                                    {
                                                        _inputReaderStringBuilder.Remove(index, 1);
                                                        _inputReaderStringBuilder.Append(' ');
                                                        Console.Out.HideCur();
                                                        Console.Out.SetCursorPosConstraintedInWorkArea(ref x, ref y);
                                                        var slines = Console.Out.GetWorkAreaStringSplits(_inputReaderStringBuilder.ToString(), _beginOfLineCurPos).Splits;
                                                        var enableConstraintConsolePrintInsideWorkArea = Console.EnableConstraintConsolePrintInsideWorkArea;
                                                        Console.EnableConstraintConsolePrintInsideWorkArea = false;
                                                        foreach (var sline in slines)
                                                            if (sline.Y >= top && sline.Y <= bottom)
                                                            {
                                                                Console.Out.SetCursorPos(sline.X, sline.Y);
                                                                Console.Out.ConsolePrint("".PadLeft(right - sline.X, ' '));
                                                                Console.Out.SetCursorPos(sline.X, sline.Y);
                                                                Console.Out.ConsolePrint(sline.Text);
                                                            }
                                                        _inputReaderStringBuilder.Remove(_inputReaderStringBuilder.Length - 1, 1);
                                                        Console.EnableConstraintConsolePrintInsideWorkArea = enableConstraintConsolePrintInsideWorkArea;
                                                        Console.Out.SetCursorPos(x, y);
                                                        Console.Out.ShowCur();
                                                    }
                                                }
                                                break;

                                            case ConsoleKey.Delete:

                                                context.CommandLineProcessor.ModuleManager.ModuleHookManager
                                                    .InvokeHooks(context, Hooks.ReadCommandLineDeletePressed, keyPressedEventArgs);

                                                lock (Console.ConsoleLock)
                                                {
                                                    var txt = _inputReaderStringBuilder.ToString();
                                                    var index = Console.Out.GetIndexInWorkAreaConstraintedString(txt, _beginOfLineCurPos, Console.Out.CursorPos);
                                                    var x = Console.Out.CursorLeft;
                                                    var y = Console.Out.CursorTop;
                                                    if (index >= 0 && index < txt.Length)
                                                    {
                                                        _inputReaderStringBuilder.Remove(index, 1);
                                                        _inputReaderStringBuilder.Append(' ');
                                                        Console.Out.HideCur();
                                                        Console.Out.SetCursorPosConstraintedInWorkArea(ref x, ref y);
                                                        var slines = Console.Out.GetWorkAreaStringSplits(_inputReaderStringBuilder.ToString(), _beginOfLineCurPos).Splits;
                                                        var enableConstraintConsolePrintInsideWorkArea = Console.EnableConstraintConsolePrintInsideWorkArea;
                                                        Console.EnableConstraintConsolePrintInsideWorkArea = false;
                                                        foreach (var sline in slines)
                                                            if (sline.Y >= top && sline.Y <= bottom)
                                                            {
                                                                Console.Out.SetCursorPos(sline.X, sline.Y);
                                                                Console.Out.ConsolePrint("".PadLeft(right - sline.X, ' '));
                                                                Console.Out.SetCursorPos(sline.X, sline.Y);
                                                                Console.Out.ConsolePrint(sline.Text);
                                                            }
                                                        _inputReaderStringBuilder.Remove(_inputReaderStringBuilder.Length - 1, 1);
                                                        Console.EnableConstraintConsolePrintInsideWorkArea = enableConstraintConsolePrintInsideWorkArea;
                                                        Console.Out.SetCursorPos(x, y);
                                                        Console.Out.ShowCur();
                                                    }
                                                }
                                                break;

                                            case ConsoleKey.UpArrow:

                                                context.CommandLineProcessor.ModuleManager.ModuleHookManager
                                                    .InvokeHooks(context, Hooks.ReadCommandLineUpArrowPressed, keyPressedEventArgs);

                                                lock (Console.ConsoleLock)
                                                {
                                                    if (Console.Out.CursorTop == _beginOfLineCurPos.Y)
                                                    {
                                                        var h = _commandLineProcessor.CmdsHistory.GetBackwardHistory();
                                                        if (h != null)
                                                        {
                                                            Console.Out.HideCur();
                                                            CleanUpReadln();
                                                            _inputReaderStringBuilder.Append(h);
                                                            Console.Out.ConsolePrint(h);
                                                            lastInputPos = Console.Out.CursorPos;
                                                            Console.Out.ShowCur();
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Console.Out.SetCursorPosConstraintedInWorkArea(
                                                            (Console.Out.CursorTop - 1) == _beginOfLineCurPos.Y ?
                                                                Math.Max(_beginOfLineCurPos.X, Console.Out.CursorLeft) : Console.Out.CursorLeft,
                                                            Console.Out.CursorTop - 1);
                                                    }
                                                }
                                                break;

                                            case ConsoleKey.DownArrow:

                                                context.CommandLineProcessor.ModuleManager.ModuleHookManager
                                                    .InvokeHooks(context, Hooks.ReadCommandLineDownArrowPressed, keyPressedEventArgs);

                                                lock (Console.ConsoleLock)
                                                {
                                                    var slines = Console.Out.GetWorkAreaStringSplits(_inputReaderStringBuilder.ToString(), _beginOfLineCurPos).Splits;
                                                    if (Console.Out.CursorTop == slines.Max(o => o.Y))
                                                    {
                                                        var fh = _commandLineProcessor.CmdsHistory.GetForwardHistory();
                                                        if (fh != null)
                                                        {
                                                            Console.Out.HideCur();
                                                            CleanUpReadln();
                                                            _inputReaderStringBuilder.Append(fh);
                                                            Console.Out.ConsolePrint(fh);
                                                            lastInputPos = Console.Out.CursorPos;
                                                            Console.Out.ShowCur();
                                                        }
                                                    }
                                                    else
                                                    {
                                                        var sline = slines.Where(o => o.Y == Console.Out.CursorTop + 1).FirstOrDefault();
                                                        if (sline?.Text != null)
                                                            Console.Out.SetCursorPosConstraintedInWorkArea(Math.Min(Console.Out.CursorLeft, sline.X + sline.Length), Console.Out.CursorTop + 1);
                                                    }
                                                }
                                                break;

                                            default:
                                                AcceptChar();
                                                break;
                                        }
                                    }
                                    else
                                        _ignoreNextKey = false;

                                    #endregion

                                    _keyEventArgsPool.Return(keyPressedEventArgs);
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
                                    lock (Console.ConsoleLock)
                                    {
                                        var x0 = Console.Out.CursorLeft;
                                        var y0 = Console.Out.CursorTop;
                                        var txt = _inputReaderStringBuilder.ToString();
                                        index = Console.Out.GetIndexInWorkAreaConstraintedString(txt, _beginOfLineCurPos, x0, y0);
                                        insert = index - txt.Length < 0;

                                        if (insert)
                                        {
                                            Console.Out.HideCur();
                                            var x = x0;
                                            var y = y0;
                                            Console.Out.SetCursorPosConstraintedInWorkArea(ref x, ref y);
                                            _inputReaderStringBuilder.Insert(index, printedStr);
                                            var slines = Console.Out.GetWorkAreaStringSplits(_inputReaderStringBuilder.ToString(), _beginOfLineCurPos).Splits;
                                            var enableConstraintConsolePrintInsideWorkArea = Console.EnableConstraintConsolePrintInsideWorkArea;
                                            Console.EnableConstraintConsolePrintInsideWorkArea = false;
                                            foreach (var sline in slines)
                                                if (sline.Y >= top && sline.Y <= bottom)
                                                {
                                                    Console.Out.SetCursorPos(sline.X, sline.Y);
                                                    Console.Out.ConsolePrint(sline.Text);
                                                }
                                            Console.EnableConstraintConsolePrintInsideWorkArea = enableConstraintConsolePrintInsideWorkArea;
                                            x += printedStr.Length;
                                            Console.Out.SetCursorPosConstraintedInWorkArea(ref x, ref y);
                                            Console.Out.ShowCur();
                                        }
                                        if (!insert)
                                        {
                                            _inputReaderStringBuilder.Append(printedStr);
                                            Console.Out.ConsolePrint(printedStr, false);
                                        }
                                    }
                                }

                                if (eol) break;
                            }

                        }
                        catch
                        {
                            // input processing crashed : mute error, re-engage prompt
                        }

                        // process input

                        var s = _inputReaderStringBuilder.ToString();
                        _inputReaderStringBuilder.Clear();

                        // put the cursor at the end of the input
                        try
                        {
                            var slines = Console.Out.GetWorkAreaStringSplits(s, _beginOfLineCurPos).Splits;
                            if (slines.Count > 0)
                            {
                                var sline = slines.Last();
                                Console.Out.CursorPos = new Point( /*(sline.Text.Length==0)?0:*/sline.Text.Length + sline.X, sline.Y);
                            }
                        }
                        catch (Exception) { }

                        var _enableConstraintConsolePrintInsideWorkArea = Console.EnableConstraintConsolePrintInsideWorkArea;
                        if (noWorkArea)
                            Console.EnableConstraintConsolePrintInsideWorkArea = false;

                        asyncCallback?.Invoke(
                            new BeginReadlnAsyncResult(s)
                            );

                        if (noWorkArea)
                            Console.EnableConstraintConsolePrintInsideWorkArea = _enableConstraintConsolePrintInsideWorkArea;

                        _readingStarted = false;
                        if (_nextPrompt != null)
                        {
                            prompt = _prompt = _nextPrompt;
                            _nextPrompt = null;
                        }
                    }
                }
                catch (ThreadInterruptedException)
                {
                    // normal end
                }
                catch (Exception inputStreamReaderThreadException)
                {
                    Console.LogException(inputStreamReaderThreadException, "input stream reader crashed");
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
                lock (Console.ConsoleLock)
                {
                    //Out.ConsoleCursorPosRestore();    // both works
                    Console.Out.CursorPos = _beginOfLineCurPos;
                    if (_commandLineProcessor.CommandEvaluationContext.ShellEnv.IsOptionSetted(ShellEnvironmentVar.settings_console_enableCompatibilityMode))
                        /* 💥 */ // ED p0 clean up screen in ConPty
                        Console.Out.Write(ANSI.EL(ANSI.ELParameter.p0));    // minimum compatible
                    else
                        Console.Out.Write(ANSI.ED(ANSI.EDParameter.p0));  // not in compatibility mode ( TODO: check)
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
