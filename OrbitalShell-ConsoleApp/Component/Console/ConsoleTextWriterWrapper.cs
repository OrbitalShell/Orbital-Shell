using OrbitalShell.Component.UI;
using OrbitalShell.Component.EchoDirective;
using OrbitalShell.Lib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using itpsrv = System.Runtime.InteropServices;
using System.Threading.Tasks;
using static OrbitalShell.Lib.Str;
using sc = System.Console;
using static OrbitalShell.Component.Console.ANSI;
using static OrbitalShell.Component.EchoDirective.Shortcuts;
using OrbitalShell.Component.Script;

namespace OrbitalShell.Component.Console
{
    public class ConsoleTextWriterWrapper : TextWriterWrapper
    {
        #region attributes

        public bool RedirecToErr = false;

        public IConsole Console;

        public ColorSettings ColorSettings;

        protected int _cursorLeftBackup;
        protected int _cursorTopBackup;
        protected ConsoleColor _backgroundBackup = ConsoleColor.Black;
        protected ConsoleColor _foregroundBackup = ConsoleColor.White;

        EchoDirectiveProcessor EchoDirectiveProcessor;

        public static readonly string ESC = (char)27 + "";

        public string LNBRK
        {
            get
            {
                // fix end of line remained filled with last colors
                return
                    EnableAvoidEndOfLineFilledWithBackgroundColor ?
                        $"{ANSI.RSTXTA}{ANSI.EL(ANSI.ELParameter.p0)}{CRLF}{GetRestoreDefaultColors}"
                        : $"{CRLF}";
            }
        }

        public CSharpScriptEngine CSharpScriptEngine;

        #endregion

        #region console output settings

        public int CropX = -1;
        public bool EnableFillLineFromCursor = true;

        public bool EnableAvoidEndOfLineFilledWithBackgroundColor = true;

        #endregion

        #region console information cache

        protected Point _cachedCursorPosition = Point.Empty;
        protected Size _cachedBufferSize = Size.Empty;
        ConsoleColor? _cachedForegroundColor;
        ConsoleColor? _cachedBackgroundColor;

        #endregion

        #region init

        public ConsoleTextWriterWrapper(IConsole console) : base() { Init(console); }

        public ConsoleTextWriterWrapper(IConsole console,TextWriter textWriter) : base(textWriter) { Init(console); }

        public ConsoleTextWriterWrapper(IConsole console,CSharpScriptEngine cSharpScriptEngine) : base() { Init(console,cSharpScriptEngine); }

        public ConsoleTextWriterWrapper(IConsole console,TextWriter textWriter, CSharpScriptEngine cSharpScriptEngine) : base(textWriter) { Init(console,cSharpScriptEngine); }

        /// <summary>
        /// shell init
        /// </summary>
        void Init(IConsole console,CSharpScriptEngine cSharpScriptEngine = null)
        {
            Console = console;
            console.CheckConsoleHasGeometry();
            CSharpScriptEngine = cSharpScriptEngine ?? new CSharpScriptEngine(console);

            // TIP: dot not affect background color throught System.Console.Background to preserve terminal console background transparency
            Console.DefaultForeground = sc.ForegroundColor;
            _cachedForegroundColor = Console.DefaultForeground;

            InitEchoDirectives();
        }

        void InitEchoDirectives()
        {
            // echo_directive => SimpleCommandDelegate, CommandDelegate, parameter
            var _drtvs = new Dictionary<string, (EchoDirectiveProcessor.SimpleCommandDelegate simpleCommand, EchoDirectiveProcessor.CommandDelegate command, object parameter)>() {
                { EchoDirectives.bkf+""   , (BackupForeground, null,null) },
                { EchoDirectives.bkb+""   , (BackupBackground, null,null) },
                { EchoDirectives.rsf+""   , (RestoreForeground, null,null) },
                { EchoDirectives.rsb+""   , (RestoreBackground, null,null) },

                { EchoDirectives.f+"="    , (null, _SetForegroundColor,null) },
                { EchoDirectives.f8+"="   , (null, _SetForegroundParse8BitColor,null) },
                { EchoDirectives.f24+"="  , (null, _SetForegroundParse24BitColor,null) },
                { EchoDirectives.b+"="    , (null, _SetBackgroundColor,null) },
                { EchoDirectives.b8+"="   , (null, _SetBackgroundParse8BitColor,null) },
                { EchoDirectives.b24+"="  , (null, _SetBackgroundParse24BitColor,null) },

                { EchoDirectives.df+"="   , (null, _SetDefaultForeground,null) },
                { EchoDirectives.db+"="   , (null, _SetDefaultBackground,null) },
                { EchoDirectives.rdc+""   , (RestoreDefaultColors, null,null) },

                { EchoDirectives.cls+""   , (ClearScreen, null,null) },

                { EchoDirectives.br+""    , (LineBreak, null,null) },
                { EchoDirectives.inf+""   , (Infos, null,null) },
                { EchoDirectives.bkcr+""  , (BackupCursorPos, null,null) },
                { EchoDirectives.rscr+""  , (RestoreCursorPos, null,null) },
                { EchoDirectives.crh+""   , (HideCur, null,null) },
                { EchoDirectives.crs+""   , (ShowCur, null,null) },
                { EchoDirectives.crx+"="  , (null, _SetCursorX,null) },
                { EchoDirectives.cry+"="  , (null, _SetCursorY,null) },
                { EchoDirectives.exit+""  , (null, _Exit,null) },
                { EchoDirectives.exec+"=" , (null, _ExecCSharp,null) },

                { EchoDirectives.invon+""   , (EnableInvert, null,null) },
                { EchoDirectives.lion+""    , (EnableLowIntensity, null,null) },
                { EchoDirectives.uon+""     , (EnableUnderline, null,null) },
                { EchoDirectives.bon+""     , (EnableBold, null,null) },
                { EchoDirectives.blon+""    , (EnableBlink, null,null) },
                { EchoDirectives.tdoff+""   , (DisableTextDecoration, null,null) },

                { EchoDirectives.cl+""          , (ClearLine, null,null) },
                { EchoDirectives.clright+""     , (ClearLineFromCursorRight, null,null) },
                { EchoDirectives.fillright+""   , (FillFromCursorRight, null,null) },
                { EchoDirectives.clleft+""      , (ClearLineFromCursorLeft, null,null) },

                { EchoDirectives.cup+""     , (_MoveCursorTop, null,null) },
                { EchoDirectives.cdown+""   , (_MoveCursorDown, null,null) },
                { EchoDirectives.cleft+""   , (_MoveCursorLeft, null,null) },
                { EchoDirectives.cright+""  , (_MoveCursorRight, null,null) },
                { EchoDirectives.chome+""   , (CursorHome, null,null) },

                { EchoDirectives.cnup+"="       , (null, _MoveCursorTop,null) },
                { EchoDirectives.cndown+"="     , (null, _MoveCursorDown,null) },
                { EchoDirectives.cnleft+"="     , (null, _MoveCursorLeft,null) },
                { EchoDirectives.cnright+"="    , (null, _MoveCursorRight,null) },

                // ANSI

                { EchoDirectives.ESC+"" , (null,_ANSI,ANSI.ESC) },
                { EchoDirectives.CRLF+"" , (null,_ANSI,ANSI.CRLF) },
                { EchoDirectives.CSI+"" , (null,_ANSI,ANSI.CSI) },
                { EchoDirectives.DECSC+"" , (null,_ANSI,ANSI.DECSC) },
                { EchoDirectives.DECRC+"" , (null,_ANSI,ANSI.DECRC) },
                { EchoDirectives.RIS+"" , (null,_ANSI,ANSI.RIS) },
                { EchoDirectives.RSTXTA+"" , (null,_ANSI,ANSI.RSTXTA) },
                { EchoDirectives.RSCOLDEF+"" , (null,_ANSI,ANSI.RSCOLDEF) },

                { EchoDirectives.CUU+"" , (null,_ANSI_Int,(EchoDirectiveProcessor.Command1pIntDelegate)ANSI.CUU) },
                { EchoDirectives.CUU+"=" , (null,_ANSI_Int,(EchoDirectiveProcessor.Command1pIntDelegate)ANSI.CUU) },
                { EchoDirectives.CUD+"" , (null,_ANSI_Int,(EchoDirectiveProcessor.Command1pIntDelegate)ANSI.CUD) },
                { EchoDirectives.CUD+"=" , (null,_ANSI_Int,(EchoDirectiveProcessor.Command1pIntDelegate)ANSI.CUD) },
                { EchoDirectives.CUF+"" , (null,_ANSI_Int,(EchoDirectiveProcessor.Command1pIntDelegate)ANSI.CUF) },
                { EchoDirectives.CUF+"=" , (null,_ANSI_Int,(EchoDirectiveProcessor.Command1pIntDelegate)ANSI.CUF) },
                { EchoDirectives.CUB+"" , (null,_ANSI_Int,(EchoDirectiveProcessor.Command1pIntDelegate)ANSI.CUB) },
                { EchoDirectives.CUB+"=" , (null,_ANSI_Int,(EchoDirectiveProcessor.Command1pIntDelegate)ANSI.CUB) },
                { EchoDirectives.CNL+"" , (null,_ANSI_Int,(EchoDirectiveProcessor.Command1pIntDelegate)ANSI.CNL) },
                { EchoDirectives.CNL+"=" , (null,_ANSI_Int,(EchoDirectiveProcessor.Command1pIntDelegate)ANSI.CNL) },
                { EchoDirectives.CPL+"" , (null,_ANSI_Int,(EchoDirectiveProcessor.Command1pIntDelegate)ANSI.CPL) },
                { EchoDirectives.CPL+"=" , (null,_ANSI_Int,(EchoDirectiveProcessor.Command1pIntDelegate)ANSI.CPL) },
                { EchoDirectives.CHA+"" , (null,_ANSI_Int,(EchoDirectiveProcessor.Command1pIntDelegate)ANSI.CHA) },
                { EchoDirectives.CHA+"=" , (null,_ANSI_Int,(EchoDirectiveProcessor.Command1pIntDelegate)ANSI.CHA) },
                { EchoDirectives.CUP+"" , (null,_ANSI_2Int,(EchoDirectiveProcessor.Command2pIntDelegate)ANSI.CUP) },
                { EchoDirectives.CUP+"=" , (null,_ANSI_2Int,(EchoDirectiveProcessor.Command2pIntDelegate)ANSI.CUP) },
                { EchoDirectives.SU+"" , (null,_ANSI_Int,(EchoDirectiveProcessor.Command1pIntDelegate)ANSI.SU) },
                { EchoDirectives.SU+"=" , (null,_ANSI_Int,(EchoDirectiveProcessor.Command1pIntDelegate)ANSI.SU) },
                { EchoDirectives.SD+"" , (null,_ANSI_Int,(EchoDirectiveProcessor.Command1pIntDelegate)ANSI.SD) },
                { EchoDirectives.SD+"=" , (null,_ANSI_Int,(EchoDirectiveProcessor.Command1pIntDelegate)ANSI.SD) },

                { EchoDirectives.DSR+"" , (null,_ANSI,ANSI.DSR) },

                { EchoDirectives.ED+"=" , (null,_ANSI_EDParameter,(Command1pEDParameterDelegate)ANSI.ED) },
                { EchoDirectives.EL+"=" , (null,_ANSI_ELParameter,(Command1pELParameterDelegate)ANSI.EL) },

                { EchoDirectives.SGR_Reset+"" , (null,_ANSI,ANSI.SGR_Reset) },
                { EchoDirectives.SGR_IncreasedIntensity+"" , (null,_ANSI,ANSI.SGR_IncreasedIntensity) },
                { EchoDirectives.SGR_DecreaseIntensity+"" , (null,_ANSI,ANSI.SGR_DecreaseIntensity) },
                { EchoDirectives.SGR_Italic+"" , (null,_ANSI,ANSI.SGR_Italic) },
                { EchoDirectives.SGR_Underline+"" , (null,_ANSI,ANSI.SGR_Underline) },
                { EchoDirectives.SGR_SlowBlink+"" , (null,_ANSI,ANSI.SGR_SlowBlink) },
                { EchoDirectives.SGR_RapidBlink+"" , (null,_ANSI,ANSI.SGR_RapidBlink) },
                { EchoDirectives.SGR_ReverseVideo+"" , (null,_ANSI,ANSI.SGR_ReverseVideo) },
                { EchoDirectives.SGR_ItalicOff+"" , (null,_ANSI,ANSI.SGR_ItalicOff) },
                { EchoDirectives.SGR_UnderlineOff+"" , (null,_ANSI,ANSI.SGR_UnderlineOff) },
                { EchoDirectives.SGR_BlinkOff+"" , (null,_ANSI,ANSI.SGR_BlinkOff) },
                { EchoDirectives.SGR_ReverseOff+"" , (null,_ANSI,ANSI.SGR_ReverseOff) },
                { EchoDirectives.SGR_NotCrossedOut+"" , (null,_ANSI,ANSI.SGR_NotCrossedOut) },
                { EchoDirectives.SGR_CrossedOut+"" , (null,_ANSI,ANSI.SGR_CrossedOut) },
                { EchoDirectives.SGR_DoubleUnderline+"" , (null,_ANSI,ANSI.SGR_DoubleUnderline) },
                { EchoDirectives.SGR_NormalIntensity+"" , (null,_ANSI,ANSI.SGR_NormalIntensity) },

                { EchoDirectives.SGRF+"=" , (null,ANSI.SGRF,null) },
                { EchoDirectives.SGRF8+"=" , (null,ANSI.SGRF8,null) },
                { EchoDirectives.SGRF24+"=" , (null,ANSI.SGRF24,null) },
                { EchoDirectives.SGRB+"=" , (null,ANSI.SGRB,null) },
                { EchoDirectives.SGRB8+"=" , (null,ANSI.SGRB8,null) },
                { EchoDirectives.SGRB24+"=" , (null,ANSI.SGRB24,null) },

                // Unicode characters

                { EchoDirectives.Lire+"" , (null,_Unicode,Unicode.Lire) },
                { EchoDirectives.Yen+"" , (null,_Unicode,Unicode.Yen) },
                { EchoDirectives.None+"" , (null,_Unicode,Unicode.None) },
                { EchoDirectives.ARet+"" , (null,_Unicode,Unicode.ARet) },
                { EchoDirectives.Demi+"" , (null,_Unicode,Unicode.Demi) },
                { EchoDirectives.Quar+"" , (null,_Unicode,Unicode.Quar) },
                { EchoDirectives.ThreeQuar+"" , (null,_Unicode,Unicode.ThreeQuar) },
                { EchoDirectives.DoubleExclam+"" , (null,_Unicode,Unicode.DoubleExclam) },
                { EchoDirectives.Exp1+"" , (null,_Unicode,Unicode.Exp1) },
                { EchoDirectives.Exp2+"" , (null,_Unicode,Unicode.Exp2) },
                { EchoDirectives.Exp3+"" , (null,_Unicode,Unicode.Exp3) },
                { EchoDirectives.ExpRelease+"" , (null,_Unicode,Unicode.ExpRelease) },
                { EchoDirectives.Copyright+"" , (null,_Unicode,Unicode.Copyright) },
                { EchoDirectives.AE+"" , (null,_Unicode,Unicode.AE) },
                { EchoDirectives.AESmall+"" , (null,_Unicode,Unicode.AESmall) },
                { EchoDirectives.Bull+"" , (null,_Unicode,Unicode.Bull) },
                { EchoDirectives.ArrowThickUp+"" , (null,_Unicode,Unicode.ArrowThickUp) },
                { EchoDirectives.ArrowThickDown+"" , (null,_Unicode,Unicode.ArrowThickDown) },
                { EchoDirectives.ArrowThickLeft+"" , (null,_Unicode,Unicode.ArrowThickLeft) },
                { EchoDirectives.ArrowThickRight+"" , (null,_Unicode,Unicode.ArrowThickRight) },
                { EchoDirectives.ArrowUp+"" , (null,_Unicode,Unicode.ArrowUp) },
                { EchoDirectives.ArrowRight+"" , (null,_Unicode,Unicode.ArrowRight) },
                { EchoDirectives.ArrowDown+"" , (null,_Unicode,Unicode.ArrowDown) },
                { EchoDirectives.ArrowLeftRight+"" , (null,_Unicode,Unicode.ArrowLeftRight) },
                { EchoDirectives.ArrowUpDown+"" , (null,_Unicode,Unicode.ArrowUpDown) },
                { EchoDirectives.ArrowUpDownUnderline+"" , (null,_Unicode,Unicode.ArrowUpDownUnderline) },
                { EchoDirectives.MoreOrLess+"" , (null,_Unicode,Unicode.MoreOrLess) },
                { EchoDirectives.CornerBottomLeft+"" , (null,_Unicode,Unicode.CornerBottomLeft) },
                { EchoDirectives.BarSmallDottedVertical+"" , (null,_Unicode,Unicode.BarSmallDottedVertical) },
                { EchoDirectives.LeftChevron+"" , (null,_Unicode,Unicode.LeftChevron) },
                { EchoDirectives.RightChevron+"" , (null,_Unicode,Unicode.RightChevron) },
                { EchoDirectives.EdgeFlatTopRight+"" , (null,_Unicode,Unicode.EdgeFlatTopRight) },
                { EchoDirectives.BarHorizontal+"" , (null,_Unicode,Unicode.BarHorizontal) },
                { EchoDirectives.BarVertical+"" , (null,_Unicode,Unicode.BarVertical) },
                { EchoDirectives.EdgeTopLeft+"" , (null,_Unicode,Unicode.EdgeTopLeft) },
                { EchoDirectives.EdgeTopRight+"" , (null,_Unicode,Unicode.EdgeTopRight) },
                { EchoDirectives.EdgeBottomLeft+"" , (null,_Unicode,Unicode.EdgeBottomLeft) },
                { EchoDirectives.EdgeBottomRight+"" , (null,_Unicode,Unicode.EdgeBottomRight) },
                { EchoDirectives.EdgeRowLeft+"" , (null,_Unicode,Unicode.EdgeRowLeft) },
                { EchoDirectives.EdgeRowRight+"" , (null,_Unicode,Unicode.EdgeRowRight) },
                { EchoDirectives.EdgeColTop+"" , (null,_Unicode,Unicode.EdgeColTop) },
                { EchoDirectives.EdgeColBottom+"" , (null,_Unicode,Unicode.EdgeColBottom) },
                { EchoDirectives.EdgeRowColCross+"" , (null,_Unicode,Unicode.EdgeRowColCross) },
                { EchoDirectives.BarDoubleThickHorizontal+"" , (null,_Unicode,Unicode.BarDoubleThickHorizontal) },
                { EchoDirectives.BarDoubleHorizontal+"" , (null,_Unicode,Unicode.BarDoubleHorizontal) },
                { EchoDirectives.BarDoubleVertical+"" , (null,_Unicode,Unicode.BarDoubleVertical) },
                { EchoDirectives.EdgeDoubleTopLeft+"" , (null,_Unicode,Unicode.EdgeDoubleTopLeft) },
                { EchoDirectives.EdgeDoubleTopRight+"" , (null,_Unicode,Unicode.EdgeDoubleTopRight) },
                { EchoDirectives.EdgeDoubleBottomLeft+"" , (null,_Unicode,Unicode.EdgeDoubleBottomLeft) },
                { EchoDirectives.EdgeDoubleBottomRight+"" , (null,_Unicode,Unicode.EdgeDoubleBottomRight) },
                { EchoDirectives.EdgeDoubleRowLeft+"" , (null,_Unicode,Unicode.EdgeDoubleRowLeft) },
                { EchoDirectives.EdgeDoubleRowRight+"" , (null,_Unicode,Unicode.EdgeDoubleRowRight) },
                { EchoDirectives.EdgeDoubleColTop+"" , (null,_Unicode,Unicode.EdgeDoubleColTop) },
                { EchoDirectives.EdgeDoubleColBottom+"" , (null,_Unicode,Unicode.EdgeDoubleColBottom) },
                { EchoDirectives.EdgeDoubleRowColCross+"" , (null,_Unicode,Unicode.EdgeDoubleRowColCross) },
                { EchoDirectives.BoxHalfBottom+"" , (null,_Unicode,Unicode.BoxHalfBottom) },
                { EchoDirectives.BoxHalfTop+"" , (null,_Unicode,Unicode.BoxHalfTop) },
                { EchoDirectives.Box+"" , (null,_Unicode,Unicode.Box) },
                { EchoDirectives.BoxQuarLight+"" , (null,_Unicode,Unicode.BoxQuarLight) },
                { EchoDirectives.BoxTierLight+"" , (null,_Unicode,Unicode.BoxTierLight) },
                { EchoDirectives.BoxHalfLight+"" , (null,_Unicode,Unicode.BoxHalfLight) },
                { EchoDirectives.CardPic+"" , (null,_Unicode,Unicode.CardPic) },
                { EchoDirectives.CardTrefl+"" , (null,_Unicode,Unicode.CardTrefl) },
                { EchoDirectives.CardArt+"" , (null,_Unicode,Unicode.CardArt) },
                { EchoDirectives.CardCarro+"" , (null,_Unicode,Unicode.CardCarro) },

            };

            EchoDirectiveProcessor = new EchoDirectiveProcessor(
                this,
                new CommandMap(_drtvs)
                );
        }

        #endregion

        #region commands impl. for echo directives map (avoiding lambdas in map)

        public delegate object Command1pEDParameterDelegate(EDParameter n);
        public delegate object Command1pELParameterDelegate(ELParameter n);

        object _ANSI(object p) => p as string;

        object _Unicode(object p) => ((char)p) + "";

        object _ANSI_2Int(object parameters)
        {
            if (parameters is EchoDirectiveProcessor.Command2pIntDelegate com)
                return com.Invoke();
            if (parameters is ValueTuple<object, string>)
            {
                var p = (ValueTuple<object, string>)parameters;
                try
                {
                    var command = (EchoDirectiveProcessor.Command2pIntDelegate)p.Item1;
                    var t = p.Item2.Split(":");
                    var x = Convert.ToInt32(t[0]);
                    var y = Convert.ToInt32(t[1]);
                    return command.Invoke(x, y);
                }
                catch (Exception)
                {
                    Error($"bad format and/or int value: {p.Item2} - attempted is {{Int}}:{{Int}}");
                }
            }
            return null;
        }

        object _ANSI_Int(object parameters)
        {
            if (parameters is EchoDirectiveProcessor.Command1pIntDelegate com)
                return com.Invoke();
            if (parameters is ValueTuple<object, string>)
            {
                var p = (ValueTuple<object, string>)parameters;
                try
                {
                    var command = (EchoDirectiveProcessor.Command1pIntDelegate)p.Item1;
                    return command.Invoke(Convert.ToInt32(p.Item2));
                }
                catch (Exception)
                {
                    Error($"bad Int value: {p.Item2}");
                }
            }
            return null;
        }

        object _ANSI_EDParameter(object parameters)
        {
            if (parameters is ValueTuple<object, string>)
            {
                var p = (ValueTuple<object, string>)parameters;
                try
                {
                    var command = (Command1pEDParameterDelegate)p.Item1;
                    return command.Invoke(Enum.Parse<EDParameter>(p.Item2));
                }
                catch (Exception)
                {
                    Error($"bad EDParameter value: {p.Item2}");
                }
            }
            return null;
        }

        object _ANSI_ELParameter(object parameters)
        {
            if (parameters is ValueTuple<object, string>)
            {
                var p = (ValueTuple<object, string>)parameters;
                try
                {
                    var command = (Command1pELParameterDelegate)p.Item1;
                    return command.Invoke(Enum.Parse<ELParameter>(p.Item2));
                }
                catch (Exception)
                {
                    Error($"bad ELParameter value: {p.Item2}");
                }
            }
            return null;
        }

        object _Exit(object x) { Console.Exit(); return null; }
        object _SetForegroundColor(object x) { SetForeground(TextColor.ParseColor(Console,x)); return null; }
        object _SetForegroundParse8BitColor(object x) { SetForeground(TextColor.Parse8BitColor(Console, x)); return null; }
        object _SetForegroundParse24BitColor(object x) { SetForeground(TextColor.Parse24BitColor(Console, x)); return null; }

        object _SetBackgroundColor(object x)
        {
            var c = TextColor.ParseColor(Console, x);
            if (c.HasValue) SetBackground(c.Value);
            return null;
        }

        object _SetBackgroundParse8BitColor(object x) { SetBackground(TextColor.Parse8BitColor(Console, x)); return null; }
        object _SetBackgroundParse24BitColor(object x) { SetBackground(TextColor.Parse24BitColor(Console, x)); return null; }
        object _SetDefaultForeground(object x)
        {
            var c = TextColor.ParseColor(Console, x);
            if (c.HasValue) SetDefaultForeground(c.Value);
            return null;
        }
        object _SetDefaultBackground(object x)
        {
            var c = TextColor.ParseColor(Console, x);
            if (c.HasValue) SetDefaultBackground(c.Value);
            return null;
        }
        object _SetCursorX(object x) { CursorLeft = Console.GetCursorX(x); return null; }
        object _SetCursorY(object x) { CursorTop = Console.GetCursorY(x); return null; }
        object _ExecCSharp(object x) { return CSharpScriptEngine.ExecCSharp((string)x,this); }
        void _MoveCursorTop() { MoveCursorTop(1); }
        void _MoveCursorDown() { MoveCursorDown(1); }
        void _MoveCursorLeft() { MoveCursorLeft(1); }
        void _MoveCursorRight() { MoveCursorRight(1); }
        object _MoveCursorTop(object x) { MoveCursorTop(Convert.ToInt32(x)); return null; }
        object _MoveCursorDown(object x) { MoveCursorDown(Convert.ToInt32(x)); return null; }
        object _MoveCursorLeft(object x) { MoveCursorLeft(Convert.ToInt32(x)); return null; }
        object _MoveCursorRight(object x) { MoveCursorRight(Convert.ToInt32(x)); return null; }

        #endregion

        #region buffering operations

        void BackupCursorInformation()
        {
            _cachedCursorPosition = CursorPos;
            _cachedBufferSize = new Size(sc.BufferWidth, sc.BufferHeight);
        }

        void ClearCursorInformation()
        {
            _cachedCursorPosition = Point.Empty;
            _cachedBufferSize = Size.Empty;
        }

        public override void EnableBuffer()
        {
            lock (Lock)
            {
                if (!IsBufferEnabled)
                {
                    base.EnableBuffer();
                    BackupCursorInformation();
                }
            }
        }

        public override void CloseBuffer()
        {
            lock (Lock)
            {
                if (IsBufferEnabled)
                {
                    base.CloseBuffer();
                    ClearCursorInformation();
                }
            }
        }

        #endregion

        #region console output operations

        public void Infos()
        {
            var @out = Console.Out;
            var colors = Console.Colors;
            lock (@out.Lock)
            {
                @out.Echoln($"OS={Environment.OSVersion} {(Environment.Is64BitOperatingSystem ? "64" : "32")}bits plateform={RuntimeEnvironment.OSType}");
                @out.Echoln($"{Bkf}{colors.HighlightIdentifier}window:{Rsf} left={colors.Numeric}{sc.WindowLeft}{Rsf},top={colors.Numeric}{sc.WindowTop}{Rsf},width={colors.Numeric}{sc.WindowWidth}{Rsf},height={colors.Numeric}{sc.WindowHeight}{Rsf},largest width={colors.Numeric}{sc.LargestWindowWidth}{Rsf},largest height={colors.Numeric}{sc.LargestWindowHeight}{Rsf}");
                @out.Echoln($"{colors.HighlightIdentifier}buffer:{Rsf} width={colors.Numeric}{sc.BufferWidth}{Rsf},height={colors.Numeric}{sc.BufferHeight}{Rsf} | input encoding={colors.Numeric}{sc.InputEncoding.EncodingName}{Rsf} | output encoding={colors.Numeric}{sc.OutputEncoding.EncodingName}{Rsf}");
                @out.Echoln($"default background color={Bkf}{colors.KeyWord}{Console.DefaultBackground}{Rsf} | default foreground color={colors.KeyWord}{Console.DefaultForeground}{Rsf}");
                if (RuntimeEnvironment.OSType == itpsrv.OSPlatform.Windows)
                {
#pragma warning disable CA1416 // Valider la compatibilité de la plateforme
                    @out.Echoln($"{Bkf}number lock={colors.Numeric}{sc.NumberLock}{Rsf} | capslock={colors.Numeric}{sc.CapsLock}{Rsf}");
#pragma warning restore CA1416 // Valider la compatibilité de la plateforme
#pragma warning disable CA1416 // Valider la compatibilité de la plateforme
                    @out.Echo($"{Bkf}cursor visible={colors.Numeric}{sc.CursorVisible}{Rsf} | cursor size={colors.Numeric}{sc.CursorSize}");
#pragma warning restore CA1416 // Valider la compatibilité de la plateforme
                }
            };
        }

        public void RSTXTA()
        {
            lock (Lock) { Write(ANSI.RSTXTA); }
        }

        public void CursorHome()
        {
            lock (Lock) { Write($"{(char)27}[H"); }
        }

        public void ClearLineFromCursorRight()
        {
            lock (Lock) { Write($"{(char)27}[K"); };
        }

        public void ClearLineFromCursorLeft()
        {
            lock (Lock) { Write($"{(char)27}[1K"); }
        }

        public void ClearLine()
        {
            lock (Lock) { Write($"{(char)27}[2K"); }
        }

        public void FillFromCursorRight()
        {
            lock (Lock)
            {
                FillLineFromCursor(' ', false, false);
            }
        }

        public void EnableInvert()
        {
            lock (Lock) { Write($"{(char)27}[7m"); }
        }

        public void EnableBlink()
        {
            lock (Lock) { Write($"{(char)27}[5m"); }           // not available on many consoles
        }

        public void EnableLowIntensity()
        {
            lock (Lock) { Write($"{(char)27}[2m"); }    // not available on many consoles
        }

        public void EnableUnderline()
        {
            lock (Lock) { Write($"{(char)27}[4m"); }
        }

        public void EnableBold()
        {
            lock (Lock) { Write($"{(char)27}[1m"); }            // not available on many consoles
        }

        public void DisableTextDecoration()
        {
            lock (Lock) { Write($"{(char)27}[0m"); /*RestoreDefaultColors();*/ }
        }

        public void MoveCursorDown(int n = 1)
        {
            lock (Lock) { Write($"{(char)27}[{n}B"); }
        }

        public void MoveCursorTop(int n = 1)
        {
            lock (Lock) { Write($"{(char)27}[{n}A"); }
        }

        public void MoveCursorLeft(int n = 1)
        {
            lock (Lock) { Write($"{(char)27}[{n}D"); }
        }

        public void MoveCursorRight(int n = 1)
        {
            lock (Lock) { Write($"{(char)27}[{n}C"); }
        }

        public void ScrollWindowDown(int n = 1) { Write(((char)27) + $"[{n}T"); }

        public void ScrollWindowUp(int n = 1) { Write(((char)27) + $"[{n}S"); }

        /// <summary>
        /// backup the current 3bit foreground color
        /// </summary>
        public void BackupForeground()
        {
            lock (Lock)
            {
                if (IsBufferEnabled) throw new BufferedOperationNotAvailableException();
                _foregroundBackup = sc.ForegroundColor;
            }
        }

        /// <summary>
        /// backup the current 3bit background color
        /// </summary>
        public void BackupBackground()
        {
            lock (Lock)
            {
                if (IsBufferEnabled) throw new BufferedOperationNotAvailableException();
                _backgroundBackup = sc.BackgroundColor;
            };
        }

        public void RestoreForeground()
        {
            lock (Lock) { SetForeground(Console.DefaultForeground); }
        }

        public void RestoreBackground()
        {
            lock (Lock) { SetBackground(Console.DefaultBackground); }
        }

        /// <summary>
        /// set foreground color from a 3 bit palette color (ConsoleColor to ansi)
        /// </summary>
        /// <param name="c"></param>
        public void SetForeground(ConsoleColor? c)
        {
            if (c == null) return;
            lock (Lock)
            {
                _cachedForegroundColor = c;
                var s = Set4BitsColorsForeground(To4BitColorNum(c.Value));
                Write(s);
            }
        }

        public void SetBackground(ConsoleColor? c)
        {
            if (c == null) return;
            lock (Lock)
            {
                _cachedBackgroundColor = c;
                var s = Set4BitsColorsBackground(To4BitColorNum(c.Value));
                Write(s);
            }
        }

        /// <summary>
        /// set foreground color from a 8 bit palette color (vt/ansi)
        /// </summary>
        /// <param name="c"></param>
        public void SetForeground(int c)
        {
            lock (Lock)
            {
                Write($"{(char)27}[38;5;{c}m");
            }
        }

        /// <summary>
        /// set background color from a 8 bit palette color (vt/ansi)
        /// </summary>
        /// <param name="c"></param>
        public void SetBackground(int c)
        {
            lock (Lock)
            {
                Write($"{(char)27}[48;5;{c}m");
            }
        }

        /// <summary>
        /// set foreground color from a 24 bit palette color (vt/ansi)
        /// </summary>
        /// <param name="r">red from 0 to 255</param>
        /// <param name="g">green from 0 to 255</param>
        /// <param name="b">blue from 0 to 255</param>
        public void SetForeground(int r, int g, int b)
        {
            lock (Lock)
            {
                Write($"{(char)27}[38;2;{r};{g};{b}m");
            }
        }

        /// <summary>
        /// set background color from a 24 bit palette color (vt/ansi)
        /// </summary>
        /// <param name="r">red from 0 to 255</param>
        /// <param name="g">green from 0 to 255</param>
        /// <param name="b">blue from 0 to 255</param>
        public void SetBackground(int r, int g, int b)
        {
            lock (Lock)
            {
                Write($"{(char)27}[48;2;{r};{g};{b}m");
            }
        }

        public void SetForeground((int r, int g, int b) color) => SetForeground(color.r, color.g, color.b);

        public void SetBackground((int r, int g, int b) color) => SetBackground(color.r, color.g, color.b);

        public void SetDefaultForeground(ConsoleColor c)
        {
            lock (Lock)
            {
                Console.DefaultForeground = c; sc.ForegroundColor = c;
            }
        }

        public void SetDefaultBackground(ConsoleColor c)
        {
            lock (Lock)
            {
                Console.DefaultBackground = c; sc.BackgroundColor = c;
            };
        }

        public void SetDefaultColors(ConsoleColor foregroundColor, ConsoleColor backgroundColor)
        {
            lock (Lock)
            {
                SetDefaultForeground(foregroundColor);
                SetDefaultBackground(backgroundColor);
            };
        }

        /// <summary>
        /// use RSTXTA to force colors set to defaults (avoid to reset to transparency colors)
        /// </summary>
        public void RestoreDefaultColors()
        {
            lock (Lock)
            {
                Write(ANSI.RSTXTA);
                SetForeground(Console.DefaultForeground);
                SetBackground(Console.DefaultBackground);
                if (Console.DefaultForeground.HasValue) sc.ForegroundColor = Console.DefaultForeground.Value;
            }
        }

        string GetRestoreDefaultColors
        {
            get
            {
                var r = ANSI.RSTXTA;
                if (Console.DefaultForeground.HasValue) r += Set4BitsColorsForeground(To4BitColorNum(Console.DefaultForeground.Value));
                if (Console.DefaultBackground.HasValue) r += Set4BitsColorsBackground(To4BitColorNum(Console.DefaultBackground.Value));
                if (Console.DefaultForeground.HasValue) sc.ForegroundColor = Console.DefaultForeground.Value;
                return r;
            }
        }

        public string DefaultColors
        {
            get
            {
                return Set4BitsColors(To4BitColorNum(Console.DefaultForeground), To4BitColorNum(Console.DefaultBackground));
            }
        }

        public void ClearScreen()
        {
            lock (Lock)
            {
                if (IsBufferEnabled) throw new BufferedOperationNotAvailableException();

                //RestoreDefaultColors();       // removed for the moment - can be restored in the future
                try
                {



                    WriteLine(ANSI.RSTXTA);         // reset text attr

                    System.Threading.Thread.Sleep(10);

                    //WriteLine(ANSI.RSTXTA+"     ");         // reset text attr
                    //Write(ANSI.ED(EDparameter.p0));
                    //Write(ANSI.RSTXTA+" ");

                    sc.Write(ANSI.RIS);
                    sc.Clear();
                    //sc.Write(ANSI.RIS);

                    //Write(ESC + "[0;0H");       // bug set arbitrary cursor pos on low-ansi terminals

                    //Write(Esc + "[0;0H");       // bug set arbitrary cursor pos on low-ansi terminals
                    //base._textWriter.Flush();
                    //Write(Esc + "[2J");         // bug set arbitrary cursor pos on low-ansi terminals

                }
                catch (System.IO.IOException)
                {

                }
                //Write(Esc+"[2J" + Esc + "[0;0H"); // bugged on windows
                //UpdateUI(true, false);
            };
        }

        public void LineBreak()
        {
            lock (Lock)
            {
                Write(LNBRK);
            };
        }

        public void ConsoleCursorPosBackup()
        {
            lock (Lock)
            {
                Write(ANSI.DECSC);
            }
        }

        public void ConsoleCursorPosBackupAndRestore()
        {
            ConsoleCursorPosBackup();
            ConsoleCursorPosRestore();
        }

        public void ConsoleCursorPosRestore()
        {
            lock (Lock)
            {
                Write(ANSI.DECRC);
            }
        }

        public Task ConsoleCursorPosRestoreAsync()
        {
            lock (Lock)
            {
                return WriteAsync(ANSI.DECRC);
            }
        }

        public void BackupCursorPos()
        {
            lock (Lock)
            {
                _cursorLeftBackup = CursorLeft;
                _cursorTopBackup = CursorTop;
            };
        }

        /// <summary>
        /// compat problem on low ansi
        /// </summary>
        public void RestoreCursorPos()
        {
            lock (Lock)
            {
                Write(ESC + "[2J" + ESC + $"[{_cursorTopBackup + 1};{_cursorLeftBackup + 1}H");
                //_textWriter.CursorLeft = _cursorLeftBackup;
                //_textWriter.CursorTop = _cursorTopBackup;
            };
        }

        //public void SetCursorLeft(int x) => Locked(() => _textWriter.CursorLeft = FixX(x));

        //public void SetCursorTop(int y) => Locked(() => _textWriter.CursorTop = FixY(y));

        /// <summary>
        /// get/set cursor column
        /// </summary>
        public int CursorLeft
        {
            get
            {
                if (!Console.IsConsoleGeometryEnabled) return 0;
                lock (Lock)
                {                    
                    return IsBufferEnabled ? _cachedCursorPosition.X : sc.CursorLeft;
                }
            }
            set
            {
                if (!Console.IsConsoleGeometryEnabled) return;
                lock (Lock)
                {
                    _cachedCursorPosition.X = value;
                    Write(ESC + "[" + (value + 1) + "G");
                }
            }
        }

        /// <summary>
        /// get/set cursor top
        /// </summary>
        public int CursorTop
        {
            get
            {
                if (!Console.IsConsoleGeometryEnabled) return 0;
                lock (Lock)
                {
                    return IsBufferEnabled ? _cachedCursorPosition.X : sc.CursorTop;
                }
            }
            set
            {
                if (!Console.IsConsoleGeometryEnabled) return;
                lock (Lock)
                {
                    _cachedCursorPosition.Y = value;
                    Write(/*ESC + "[2J" +*/ ESC + $"[{value + 1};{CursorLeft + 1}H");
                    //Write( ANSI.  );
                }
            }
        }

        public Point CursorPos
        {
            get
            {
                if (!Console.IsConsoleGeometryEnabled) return new Point(0,0);
                lock (Lock)
                {
                    return new Point(CursorLeft, CursorTop);
                }
            }
            set
            {
                if (!Console.IsConsoleGeometryEnabled) return;
                lock (Lock)
                {
                    Write(ESC + $"[{value.Y + 1};{value.X + 1}H");
                }
            }
        }

        public void SetCursorPos(Point p)
        {
            if (!Console.IsConsoleGeometryEnabled) return;
            lock (Lock)
            {
                var x = p.X;
                var y = p.Y;
                Console.FixCoords(ref x, ref y);
                if (IsBufferEnabled)
                {
                    _cachedCursorPosition.X = x;
                    _cachedCursorPosition.Y = y;
                }
                //_textWriter.CursorLeft = x;
                //_textWriter.CursorTop = y;
                Write(ESC + $"[{y + 1};{x + 1}H");
            }
        }

        /// <summary>
        /// set cursor pos - @[y+1;x+1H
        /// </summary>
        /// <param name="x">x (origine 0)</param>
        /// <param name="y">y (origine 0)</param>
        public void SetCursorPos(int x, int y)
        {
            if (!Console.IsConsoleGeometryEnabled) return;
            lock (Lock)
            {
                Console.FixCoords(ref x, ref y);
                if (IsBufferEnabled)
                {
                    _cachedCursorPosition.X = x;
                    _cachedCursorPosition.Y = y;
                }
                Write(ESC + $"[{(y + 1)};{(x + 1)}H");
            }
        }

#pragma warning disable CA1416 // Valider la compatibilité de la plateforme
        public static bool CursorVisible => sc.CursorVisible;
#pragma warning restore CA1416 // Valider la compatibilité de la plateforme

        public void HideCur()
        {
            if (!Console.IsConsoleGeometryEnabled) return;
            lock (Lock) { sc.CursorVisible = false; }
        }

        public void ShowCur()
        {
            if (!Console.IsConsoleGeometryEnabled) return;
            lock (Lock) { sc.CursorVisible = true; }
        }

        /// <summary>
        /// text only, no print directives, no ansi
        /// </summary>
        /// <param name="s">text to be filtered</param>
        /// <returns>text visible characters only</returns>
        public string GetText(string s)
        {
            var r = GetPrint(s, false, false, false);
            return r;
        }

        public string GetPrint(
            string s,
            bool lineBreak = false,
            bool doNotEvaluatePrintDirectives = false,      // TODO: remove this parameter
            bool ignorePrintDirectives = false,
            EchoSequenceList printSequences = null)
        {
            lock (Lock)
            {
                if (string.IsNullOrWhiteSpace(s)) return s;
                var ms = new MemoryStream(s.Length * 4);
                var sw = new StreamWriter(ms);
                Console.RedirectOut(sw);
                var e = Console.EnableConstraintConsolePrintInsideWorkArea;
                Console.EnableConstraintConsolePrintInsideWorkArea = false;
                if (!ignorePrintDirectives)
                {
                    // directives are removed
                    s = ANSI.GetText(s);    // also removed ansi sequences
                    Echo(s, lineBreak, false, true, true, printSequences);
                }
                else
                {
                    // directives are keeped
                    Echo(s, lineBreak, false, false, true, printSequences, false, false);
                }
                Console.EnableConstraintConsolePrintInsideWorkArea = e;
                sw.Flush();
                ms.Position = 0;
                var rw = new StreamReader(ms);
                var txt = rw.ReadToEnd();
                rw.Close();
                Console.RedirectOut((StreamWriter)null);
                return txt;
            }
        }

        public string GetPrintWithEscapeSequences(string s, bool lineBreak = false)
        {
            lock (Lock)
            {
                if (string.IsNullOrWhiteSpace(s)) return s;
                var ms = new MemoryStream(s.Length * 4);
                var sw = new StreamWriter(ms);
                Console.RedirectOut(sw);
                var e = Console.EnableConstraintConsolePrintInsideWorkArea;
                Console.EnableConstraintConsolePrintInsideWorkArea = false;
                Echo(s, lineBreak);
                Console.EnableConstraintConsolePrintInsideWorkArea = e;
                sw.Flush();
                ms.Position = 0;
                var rw = new StreamReader(ms);
                var txt = rw.ReadToEnd();
                rw.Close();
                Console.RedirectOut((StreamWriter)null);
                return txt;
            }
        }

        public void ConsolePrint(string s, bool lineBreak = false)
        {
            // any print goes here...
            lock (Lock)
            {
                if (CropX == -1)
                    ConsoleSubPrint(s, lineBreak);
                else
                {
                    var x = CursorLeft;
                    var mx = Math.Max(x, CropX);
                    if (mx > x)
                    {
                        var n = mx - x + 1;
                        if (s.Length <= n)
                            ConsoleSubPrint(s, lineBreak);
                        else
                            ConsoleSubPrint(s.Substring(0, n), lineBreak);
                    }
                }
            }
        }

        /// <summary>
        /// debug echo to file
        /// </summary>
        /// <param name="s"></param>
        /// <param name="lineBreak"></param>
        /// <param name="callerMemberName"></param>
        /// <param name="callerLineNumber"></param>
        public override void EchoDebug(
            string s,
            bool lineBreak = false,
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = -1)
        {
            if (!FileEchoDebugEnabled) return;
            if (FileEchoDebugDumpDebugInfo)
            {
                if (IsBufferEnabled)
                    _debugEchoStreamWriter?.Write($"x={CursorLeft},y={CursorTop},l={s.Length}, bw={_cachedBufferSize},bh={_cachedBufferSize},br={lineBreak} [{callerMemberName}:{callerLineNumber}] :");
                else
                    _debugEchoStreamWriter?.Write($"x={CursorLeft},y={CursorTop},l={s.Length},w={sc.WindowWidth},h={sc.WindowHeight},wtop={sc.WindowTop} bw={sc.BufferWidth},bh={sc.BufferHeight},br={lineBreak} [{callerMemberName}:{callerLineNumber}] :");
            }
            _debugEchoStreamWriter?.Write(s);
            if (lineBreak | FileEchoDebugAutoLineBreak) _debugEchoStreamWriter?.WriteLine(string.Empty);
            if (FileEchoDebugAutoFlush) _debugEchoStreamWriter?.Flush();
        }

        /// <summary>
        /// simplesystem.diagnostics.debug
        /// </summary>
        public void Debug(
            string s,
            bool lineBreak = false,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int callerLineNumber = -1)
        {
            System.Diagnostics.Debug.WriteLine($"{Path.GetFileName(callerFilePath)}:{callerLineNumber} | {callerMemberName} °°° {s}");
        }

        public override void Write(string s)
        {
            if (RedirecToErr)
            {
                if (IsReplicationEnabled)
                    _replicateStreamWriter.Write(s);
                Console.Err.Write(s);
            }
            else
                base.Write(s);
        }

        //public void Echoln(IEnumerable<string> ls, bool ignorePrintDirectives = false) { foreach (var s in ls) Echoln(s, ignorePrintDirectives); }

        //public void Echo(IEnumerable<string> ls, bool lineBreak = false, bool ignorePrintDirectives = false) { foreach (var s in ls) Echo(s, lineBreak, ignorePrintDirectives); }

        public void Echoln(string s = "", bool ignorePrintDirectives = false) => Echo(s, true, false, !ignorePrintDirectives);
        public void Echoln(object s, bool ignorePrintDirectives = false) => Echo(s, true, false, !ignorePrintDirectives);

        public void Echo(
            string s = "",
            bool lineBreak = false,
            bool ignorePrintDirectives = false) => Echo(s, lineBreak, false, !ignorePrintDirectives);

        public void Echoln(char s, bool ignorePrintDirectives = false) => Echo(s + "", true, false, !ignorePrintDirectives);

        public void Echo(char s, bool lineBreak = false, bool ignorePrintDirectives = false) => Echo(s + "", lineBreak, !ignorePrintDirectives);

        /// <summary>
        /// output to stream
        /// </summary>
        /// <param name="o">object to output - is transform to string with ToText</param>
        /// <param name="lineBreak">if true, append a line break to output (call LineBreak()), default is false</param>
        /// <param name="preserveColors">TODO: remove this parameter</param>
        /// <param name="parseCommands">if true, echo directives are parsed and evaluated, default is true</param>
        /// <param name="doNotEvalutatePrintDirectives">TODO: explain this parameter</param>
        /// <param name="printSequences">to store echo sequence objects when collected</param>
        /// <param name="avoidANSISequencesAndNonPrintableCharacters">if true and parseCommands=false, replace ansiseq and non printable chars by readable data</param>
        /// <param name="getNonPrintablesASCIICodesAsLabel">if true and parseCommands=false, replace ascii non printables chars by labels</param>
        public virtual void Echo(
            object o,
            bool lineBreak = false,
            bool preserveColors = false,        // TODO: remove this parameter + SaveColors property
            bool parseCommands = true,
            bool doNotEvalutatePrintDirectives = false,         // TODO: explain this
            EchoSequenceList printSequences = null,
            bool avoidANSISequencesAndNonPrintableCharacters = true,
            bool getNonPrintablesASCIICodesAsLabel = true
            )
        {
            lock (Lock)
            {
                if (o == null)
                {
                    if (DumpNullStringAsText != null)
                        ConsolePrint(DumpNullStringAsText, false);
                }
                else
                {
                    if (parseCommands)
                        // call the EchoDirective component
                        EchoDirectiveProcessor.ParseTextAndApplyCommands(
                            o.ToString(),
                            false,
                            "",
                            doNotEvalutatePrintDirectives,
                            printSequences);
                    else
                    {
                        var txt = o.ToString();
                        if (getNonPrintablesASCIICodesAsLabel) txt = ASCII.GetNonPrintablesCodesAsLabel(txt, false /* true: show all symbols */ );
                        if (avoidANSISequencesAndNonPrintableCharacters) txt = ANSI.AvoidANSISequencesAndNonPrintableCharacters(txt);
                        ConsolePrint(txt, false);
                    }
                }

                if (lineBreak) LineBreak();
            }
        }

        public void Warningln(string s) => Warning(s, true);

        public void Warning(string s, bool lineBreak = true) => Console.Out.Echo($"{Console.Colors.Warning}{s}{Console.Colors.Default}", lineBreak);

        public void Errorln(string s) => Error(s, true);

        public void Error(string s, bool lineBreak = true)
        {
            Console.Out.RedirecToErr = true;
            Console.Out.Echo($"{Console.Colors.Error}{s}{Console.Colors.Default}", lineBreak);
            Console.Out.RedirecToErr = false;
        }

        void ConsoleSubPrint(string s, bool lineBreak = false)
        {
            lock (Lock)
            {
                if (Console.EnableConstraintConsolePrintInsideWorkArea)
                {
                    var (id, x, y, w, h) = Console.ActualWorkArea();
                    var x0 = CursorLeft;
                    var y0 = CursorTop;

                    var croppedLines = new List<string>();
                    var xr = x0 + s.Length - 1;
                    var xm = x + w - 1;
                    if (xr > xm)
                    {
                        while (xr > xm && s.Length > 0)
                        {
                            var left = s.Substring(0, s.Length - (xr - xm));
                            s = s.Substring(s.Length - (xr - xm), xr - xm);
                            croppedLines.Add(left);
                            xr = x + s.Length - 1;
                        }
                        if (s.Length > 0)
                            croppedLines.Add(s);
                        var curx = x0;
                        foreach (var line in croppedLines)
                        {
                            Write(line);
                            x0 += line.Length;
                            SetCursorPosConstraintedInWorkArea(ref x0, ref y0);
                            EchoDebug(line);
                        }
                        if (lineBreak)
                        {
                            x0 = x;
                            y0++;
                            SetCursorPosConstraintedInWorkArea(ref x0, ref y0);
                        }
                    }
                    else
                    {
                        Write(s);
                        x0 += s.Length;
                        SetCursorPosConstraintedInWorkArea(ref x0, ref y0);
                        EchoDebug(s);
                        if (lineBreak)
                        {
                            x0 = x;
                            y0++;
                            SetCursorPosConstraintedInWorkArea(ref x0, ref y0);
                        }
                    }
                }
                else
                {
                    /*var dep = CursorLeft + s.Length - 1 > x + w - 1;
                    if (dep)
                    {
                        Write(s);
                        // removed: too slow & buggy (s.Length is wrong due to ansi codes)
                        //if (!IsRedirected) FillLineFromCursor(' ');   // this fix avoid background color to fill the full line on wsl/linux
                    }
                    else*/
                    Write(s);

                    EchoDebug(s);

                    if (lineBreak)
                    {
#if fix_colors_on_br
                        var f = _cachedForegroundColor;
                        var b = _cachedBackgroundColor;
                        if (!IsRedirected)
                        {
                            // needs ShellEnv
                            SetForeground( ColorSettings.Default.Foreground.Value );
                            SetBackground( ColorSettings.Default.Background.Value );
                            _textWriter.WriteLine(string.Empty);
                        }
#else
                        /*_textWriter*/
                        WriteLine(string.Empty);
#endif

                        EchoDebug(string.Empty, true);

#if fix_colors_on_br
                        if (!IsRedirected)
                        {
                            SetForeground( f );
                            SetBackground( b );
                        }
#endif
                    }
                }
            }
        }

        void FillLineFromCursor(char c = ' ', bool resetCursorLeft = true, bool useDefaultColors = true)
        {
            lock (Lock)
            {
                if (!EnableFillLineFromCursor) return;
                var f = _cachedForegroundColor;
                var b = _cachedForegroundColor;
                var aw = Console.ActualWorkArea();
                var nb = Math.Max(0, Math.Max(aw.Right, _cachedBufferSize.Width - 1) - CursorLeft - 1);
                var x = CursorLeft;
                var y = CursorTop;
                if (useDefaultColors)
                {
                    SetForeground(ColorSettings.Default.Foreground.Value);
                    SetBackground(ColorSettings.Default.Background.Value);
                }
                Write("".PadLeft(nb, c));   // TODO: BUG in WINDOWS: do not print the last character
                SetCursorPos(nb, y);
                Write(" ");
                if (useDefaultColors)
                {
                    SetForeground(f);
                    SetBackground(b);
                }
                if (resetCursorLeft)
                    CursorLeft = x;
            }
        }

        public int GetIndexInWorkAreaConstraintedString(
            string s,
            Point origin,
            Point cursorPos,
            bool forceEnableConstraintInWorkArea = false,
            bool fitToVisibleArea = true,
            bool doNotEvaluatePrintDirectives = true,
            bool ignorePrintDirectives = false
            )
            => GetIndexInWorkAreaConstraintedString(
                s,
                origin,
                cursorPos.X,
                cursorPos.Y,
                forceEnableConstraintInWorkArea,
                fitToVisibleArea,
                doNotEvaluatePrintDirectives,
                ignorePrintDirectives);

        public int GetIndexInWorkAreaConstraintedString(
            string s,
            Point origin,
            int cursorX,
            int cursorY,
            bool forceEnableConstraintInWorkArea = false,
            bool fitToVisibleArea = true,
            bool doNotEvaluatePrintDirectives = true,
            bool ignorePrintDirectives = false)
        {
            var r = GetWorkAreaStringSplits(
                s,
                origin,
                forceEnableConstraintInWorkArea,
                fitToVisibleArea,
                doNotEvaluatePrintDirectives,
                ignorePrintDirectives,
                cursorX,
                cursorY
                );
            return r.CursorIndex;
        }

        public LineSplitList GetIndexLineSplitsInWorkAreaConstraintedString(
            string s,
            Point origin,
            int cursorX,
            int cursorY,
            bool forceEnableConstraintInWorkArea = false,
            bool fitToVisibleArea = true,
            bool doNotEvaluatePrintDirectives = false,
            bool ignorePrintDirectives = false)
        {
            var r = GetWorkAreaStringSplits(
                s,
                origin,
                forceEnableConstraintInWorkArea,
                fitToVisibleArea,
                doNotEvaluatePrintDirectives,
                ignorePrintDirectives,
                cursorX,
                cursorY
                );
            return r;
        }

        /// <summary>
        /// TODO: check for buffered mode
        /// </summary>
        /// <param name="s"></param>
        /// <param name="origin"></param>
        /// <param name="forceEnableConstraintInWorkArea"></param>
        /// <param name="fitToVisibleArea"></param>
        /// <param name="doNotEvaluatePrintDirectives"></param>
        /// <param name="ignorePrintDirectives"></param>
        /// <param name="cursorX"></param>
        /// <param name="cursorY"></param>
        /// <returns></returns>
        public LineSplitList GetWorkAreaStringSplits(
            string s,
            Point origin,
            bool forceEnableConstraintInWorkArea = false,
            bool fitToVisibleArea = true,
            bool doNotEvaluatePrintDirectives = false,
            bool ignorePrintDirectives = false,
            int cursorX = -1,
            int cursorY = -1)
        {
            if (!Console.IsConsoleGeometryEnabled) 
                return new LineSplitList(
                    new List<StringSegment>() { new StringSegment(s, 0, s.Length - 1) },null,0,0                    
                );

            var originalString = s;
            var r = new List<StringSegment>();
            EchoSequenceList printSequences = null;
            if (cursorX == -1) cursorX = origin.X;
            if (cursorY == -1) cursorY = origin.Y;
            int cursorLineIndex = -1;
            int cursorIndex = -1;

            lock (Lock)
            {
                int index = -1;
                var (id, x, y, w, h) = Console.ActualWorkArea(fitToVisibleArea);
                var x0 = origin.X;
                var y0 = origin.Y;

                var croppedLines = new List<StringSegment>();
                string pds = null;
                var length = s.Length;
                if (doNotEvaluatePrintDirectives)
                {
                    pds = s;
                    printSequences = new EchoSequenceList();
                    s = GetPrint(s, false, doNotEvaluatePrintDirectives, ignorePrintDirectives, printSequences);
                }
                var xr = x0 + s.Length - 1;
                var xm = x + w - 1;

                if (xr >= xm)
                {
                    if (pds != null)
                    {
                        var lineSegments = new List<string>();
                        var currentLine = string.Empty;
                        int lastIndex = 0;

                        foreach (var ps in printSequences)
                        {
                            if (!ps.IsText)
                                lineSegments.Add(ps.ToText());
                            else
                            {
                                currentLine += ps.Text;
                                xr = x0 + currentLine.Length - 1;
                                if (xr > xm && currentLine.Length > 0)
                                {
                                    while (xr > xm && currentLine.Length > 0)
                                    {
                                        var left = currentLine.Substring(0, currentLine.Length - (xr - xm));
                                        currentLine = currentLine.Substring(currentLine.Length - (xr - xm), xr - xm);

                                        var truncLeft = left[lastIndex..];
                                        lineSegments.Add(truncLeft);
                                        croppedLines.Add(new StringSegment(string.Join("", lineSegments), 0, 0, lastIndex + truncLeft.Length));
                                        lineSegments.Clear();
                                        lastIndex = 0;

                                        xr = x + currentLine.Length - 1;
                                    }
                                    if (currentLine.Length > 0)
                                    {
                                        lineSegments.Add(currentLine);
                                        lastIndex = currentLine.Length;
                                    }
                                }
                                else
                                {
                                    lineSegments.Add(currentLine[lastIndex..]);
                                    lastIndex = currentLine.Length;
                                }
                            }
                        }

                        if (lineSegments.Count > 0)
                        {
                            var truncLeft = currentLine[lastIndex..];
                            lineSegments.Add(truncLeft);
                            croppedLines.Add(new StringSegment(string.Join("", lineSegments), 0, 0, lastIndex + truncLeft.Length));
                            lineSegments.Clear();
                            lastIndex = 0;
                        }
                    }
                    else
                    {
                        while (xr > xm && s.Length > 0)
                        {
                            var left = s.Substring(0, s.Length - (xr - xm));
                            s = s.Substring(s.Length - (xr - xm), xr - xm);
                            croppedLines.Add(new StringSegment(left, 0, 0, left.Length));
                            xr = x + s.Length - 1;
                        }
                        if (s.Length > 0)
                            croppedLines.Add(new StringSegment(s, 0, 0, s.Length));
                    }

                    var curx = x0;
                    int lineIndex = 0;
                    index = 0;
                    bool indexFounds = false;
                    foreach (var line in croppedLines)
                    {
                        r.Add(new StringSegment(line.Text, x0, y0, line.Length));
                        if (!indexFounds && cursorY == y0)
                        {
                            index += cursorX - x0;
                            cursorIndex = index;
                            cursorLineIndex = lineIndex;
                            indexFounds = true;
                        }
                        x0 += line.Length;
                        index += line.Length;
                        SetCursorPosConstraintedInWorkArea(ref x0, ref y0, false, forceEnableConstraintInWorkArea, fitToVisibleArea);
                        lineIndex++;
                    }
                    if (!indexFounds)
                    {
                        cursorIndex = index;
                        cursorLineIndex = lineIndex;
                    }
                }
                else
                {
                    cursorIndex = cursorX - x0;
                    cursorLineIndex = 0;
                    if (pds != null)
                        r.Add(new StringSegment(pds, x0, y0, pds.Length));
                    else
                        r.Add(new StringSegment(s, x0, y0, s.Length));
                }
            }

            if (!doNotEvaluatePrintDirectives)
            {
                printSequences = new EchoSequenceList
                {
                    new EchoSequence(Console, (string)null, 0, originalString.Length - 1, null, originalString)
                };
            }

            return new LineSplitList(r, printSequences, cursorIndex, cursorLineIndex);
        }

        public void SetCursorPosConstraintedInWorkArea(Point pos, bool enableOutput = true, bool forceEnableConstraintInWorkArea = false, bool fitToVisibleArea = true)
        {
            var x = pos.X;
            var y = pos.Y;
            SetCursorPosConstraintedInWorkArea(ref x, ref y, enableOutput, forceEnableConstraintInWorkArea, fitToVisibleArea);
        }

        public void SetCursorPosConstraintedInWorkArea(int cx, int cy, bool enableOutput = true, bool forceEnableConstraintInWorkArea = false, bool fitToVisibleArea = true)
            => SetCursorPosConstraintedInWorkArea(ref cx, ref cy, enableOutput, forceEnableConstraintInWorkArea, fitToVisibleArea);

        /// <summary>
        /// TODO: check for buffered mode
        /// </summary>
        /// <param name="cx"></param>
        /// <param name="cy"></param>
        /// <param name="enableOutput"></param>
        /// <param name="forceEnableConstraintInWorkArea"></param>
        /// <param name="fitToVisibleArea"></param>
        public void SetCursorPosConstraintedInWorkArea(ref int cx, ref int cy, bool enableOutput = true, bool forceEnableConstraintInWorkArea = false, bool fitToVisibleArea = true)
        {
            lock (Lock)
            {
                int dx = 0;
                int dy = 0;

                if (Console.EnableConstraintConsolePrintInsideWorkArea || forceEnableConstraintInWorkArea)
                {
                    var (id, left, top, right, bottom) = Console.ActualWorkArea(fitToVisibleArea);
                    if (cx < left)
                    {
                        cx = right - 1;
                        cy--;
                    }
                    if (cx >= right)
                    {
                        cx = left;
                        cy++;
                    }

                    if (enableOutput && cy < top)
                    {
                        dy = top - cy;
                        cy += dy;
                        if (top + 1 <= bottom)
#pragma warning disable CA1416 // Valider la compatibilité de la plateforme
                            sc.MoveBufferArea(      // TODO: not supported on linux (ubuntu 18.04 wsl)
                                left, top, right, bottom - top,
                                left, top + 1,
                                ' ',
                                Console.DefaultForeground ?? ConsoleColor.White, Console.DefaultBackground ?? ConsoleColor.Black);
#pragma warning restore CA1416 // Valider la compatibilité de la plateforme
                    }

                    if (enableOutput && cy > bottom /*- 1*/)
                    {
                        dy = bottom /*- 1*/ - cy;
                        cy += dy;
                        var nh = bottom - top + dy + 1;
                        if (nh > 0)
                        {
#pragma warning disable CA1416 // Valider la compatibilité de la plateforme
                            sc.MoveBufferArea(      // TODO: not supported on linux (ubuntu 18.04 wsl)
                                left, top - dy, right, nh,
                                left, top,
                                ' ',
                                Console.DefaultForeground ?? ConsoleColor.White, Console.DefaultBackground ?? ConsoleColor.Black);
#pragma warning restore CA1416 // Valider la compatibilité de la plateforme
                        }
                    }
                }

                if (enableOutput)
                {
                    SetCursorPos(cx, cy);
                    if (dx != 0 || dy != 0)
                        Console.WorkAreaScrolled?.Invoke(null, new WorkAreaScrollEventArgs(0, dy));
                }
            }
        }

        #endregion

    }
}
