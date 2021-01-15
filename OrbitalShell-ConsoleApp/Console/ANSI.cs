using System;

namespace DotNetConsoleAppToolkit.Console
{
    /// <summary>
    /// all ANSI codes that we take into account for the targetted consoles referential
    /// from https://en.wikipedia.org/wiki/ANSI_escape_code
    /// </summary>
    public static class ANSI
    {
        #region char codes and prefixs

        public static readonly string ESC = ((char)27) + "";

        public static readonly string CRLF = Environment.NewLine; //(char)13 + ((char)10 + ""); // @TODO: validate on linux

        public static readonly string CSI = $"{ESC}[";

        public static string SGR(int n) => $"{ESC}{n}m";

        #endregion

        #region VT100 type Fp or 3Fp        

        /// <summary>
        /// backup cursor position
        /// </summary>
        public static readonly string DECSC = ESC+"7";

        /// <summary>
        /// restore cursor position
        /// </summary>
        public static readonly string DECRC = ESC+"8";

        #endregion

        #region ANSI type Fs

        /// <summary>
        /// Triggers a full reset of the terminal to its original state. This may include (if applicable): reset graphic rendition, clear tabulation stops, reset to default font, and more.
        /// </summary>
        public static readonly string RIS = ESC+"c";

        #endregion

        #region combos

        /// <summary>
        /// RESET TEXT ATTRIBUTES : console background (if transparency preserved), text attributes (uon,tdoff)
        /// this tip allow to force the background color restoration without filling it
        /// this tip properly and completely clean-up the text attributes
        /// you should wait after lanuch this seq about about 25ms before it is efficient (vscode debug console)
        /// </summary>        
        public static readonly string RSTXTA = CSI + "4m" + CSI + "0m";

        #endregion

        #region CSI (Control Sequence Introducer)

        /// <summary>
        /// Cursor Up - Moves the cursor n (default 1) cells in the given direction. If the cursor is already at the edge of the screen, this has no effect.
        /// </summary>
        /// <param name="n">number of cells</param>
        /// <returns>ansi seq</returns>
        public static string CUU(int n=1) => $"{CSI}{n}A";

        /// <summary>
        /// Cursor Down - Moves the cursor n (default 1) cells in the given direction. If the cursor is already at the edge of the screen, this has no effect.
        /// </summary>
        /// <param name="n">number of cells</param>
        /// <returns>ansi seq</returns>
        public static string CUD(int n=1) => $"{CSI}{n}B";

        /// <summary>
        /// Cursor Forward - Moves the cursor n (default 1) cells in the given direction. If the cursor is already at the edge of the screen, this has no effect.
        /// </summary>
        /// <param name="n">number of cells</param>
        /// <returns>ansi seq</returns>
        public static string CUF(int n=1) => $"{CSI}{n}C";

        /// <summary>
        /// Cursor Back - Moves the cursor n (default 1) cells in the given direction. If the cursor is already at the edge of the screen, this has no effect.
        /// </summary>
        /// <param name="n">number of cells</param>
        /// <returns>ansi seq</returns>
        public static string CUB(int n=1) => $"{CSI}{n}D";

        /// <summary>
        /// Cursor Next Line - Moves cursor to beginning of the line n (default 1) lines down. (not ANSI.SYS)
        /// </summary>
        /// <param name="n">line downs count</param>
        /// <returns>ansi seq</returns>
        public static string CNL(int n=1) => $"{CSI}{n}E";

        /// <summary>
        /// Cursor Previous Line - Moves cursor to beginning of the line n (default 1) lines up. (not ANSI.SYS)
        /// </summary>
        /// <param name="n">line up count</param>
        /// <returns>ansi seq</returns>
        public static string CPL(int n=1) => $"{CSI}{n}F";

        /// <summary>
        /// Cursor Horizontal Absolute - Moves the cursor to column n (default 1). (not ANSI.SYS)
        /// </summary>
        /// <param name="n">column index</param>
        /// <returns>ansi seq</returns>
        public static string CHA(int n=1) => $"{CSI}{n}G";

        /// <summary>
        /// Cursor Position - Moves the cursor to row n, column m. The values are 1-based, and default to 1 (top left corner) if omitted. A sequence such as CSI ;5H is a synonym for CSI 1;5H as well as CSI 17;H is the same as CSI 17H and CSI 17;1H
        /// </summary>
        /// <param name="row">row index</param>
        /// <param name="column">column index</param>
        /// <returns>ansi seq</returns>
        public static string CUP(int column=1,int row=1) => $"{CSI}{row};{column}H";

        /// <summary>
        /// Scroll Up - Scroll whole page up by n (default 1) lines. New lines are added at the bottom. (not ANSI.SYS)
        /// </summary>
        /// <param name="n">lines count</param>
        /// <returns>ansi seq</returns>
        public static string SU(int n=1) => $"{CSI}{n}S";

        /// <summary>
        /// Scroll Down - Scroll whole page down by n (default 1) lines. New lines are added at the top. (not ANSI.SYS)
        /// </summary>
        /// <param name="n">lines count</param>
        /// <returns>ansi seq</returns>
        public static string SD(int n=1) => $"{CSI}{n}T";

        /// <summary>
        /// Device Status Report - Reports the cursor position (CPR) to the application as (as though typed at the keyboard) ESC[n;mR, where n is the row and m is the column.)
        /// </summary>
        /// <returns>ansi seq</returns>
        public static string DSR = $"{CSI}6n";

        public enum EDparameter {

            /// <summary>
            /// If n is 0 (or missing), clear from cursor to end of screen
            /// </summary>
            p0 = 0,

            /// <summary>
            /// If n is 1, clear from cursor to beginning of the screen
            /// </summary>
            p1 = 1,

            /// <summary>
            ///  If n is 2, clear entire screen (and moves cursor to upper left on DOS ANSI.SYS).
            /// (partial support)
            /// </summary>
            p2 = 2,

            /// <summary>
            ///  If n is 3, clear entire screen and delete all lines saved in the scrollback buffer (this feature was added for xterm and is supported by other terminal applications).
            /// (low support)
            /// </summary>
            p3 = 3      

        }

        /// <summary>
        /// Erases part of the screen
        /// </summary>
        /// <param name="n">EDparameter</param>
        /// <returns>ansi seq</returns>
        public static string ED(EDparameter n) => $"{CSI}{(int)n}J";

        public enum ELParameter {

            /// <summary>
            /// if is 0 (or missing), clear from cursor to the end of the line
            /// </summary>
            p0 = 0,

            /// <summary>
            /// If n is 1, clear from cursor to beginning of the line.
            /// </summary>
            p1 = 1,

            /// <summary>
            /// If n is 2, clear entire line. Cursor position does not change.
            /// </summary>
            p2 = 2,
        }

        /// <summary>
        /// Erases part of the line
        /// </summary>
        /// <param name="n">ELparameter</param>
        /// <returns>ansi seq</returns> 
        public static string EL(ELParameter n) => $"{CSI}{(int)n}K";

        #endregion

        #region SGR (Select Graphic Rendition) 

        /// <summary>
        /// Reset / Normal - All attributes off (not always efficient on many terminals. @see combo RSTXTA)
        /// </summary>
        /// <returns>ansi seq</returns>
        public static string SGR_Reset => $"{SGR(0)}";

        /// <summary>
        /// Bold or increased intensity - As with faint, the color change is a PC (SCO / CGA) invention.
        /// </summary>
        /// <returns>ansi seq</returns>
        public static string SGR_IncreasedIntensity => $"{SGR(1)}";

        /// <summary>
        /// Faint or decreased intensity - aka Dim (with a saturated color). May be implemented as a light font weight like bold.
        /// </summary>
        /// <returns>ansi seq</returns>
        public static string SGR_DecreaseIntensity => $"{SGR(2)}";

        /// <summary>
        /// Italic - Not widely supported. Sometimes treated as inverse or blink. (not windows terminal, probably not ANSY.SYS, but ok on vscode debuger)
        /// </summary>
        /// <returns>ansi seq</returns>
        public static string SGR_Italic => $"{SGR(3)}";

        /// <summary>
        /// Underline - Style extensions exist for Kitty, VTE, mintty and iTerm2.
        /// </summary>
        /// <returns>ansi seq</returns>
        public static string SGR_Underline => $"{SGR(4)}";

        /// <summary>
        /// Slow Blink - less than 150 per minute (not vscode debuger, ok with windows terminal )
        /// </summary>
        /// <returns>ansi seq</returns>
        public static string SGR_SlowBlink => $"{SGR(5)}";

        /// <summary>
        /// Rapid Blink - MS-DOS ANSI.SYS, 150+ per minute; not widely supported (not vscode debuger, ok with windows terminal. Same blink speed than SlowBlink)
        /// </summary>
        /// <returns>ansi seq</returns>
        public static string SGR_RapidBlink => $"{SGR(6)}";

        /// <summary>
        /// Reverse video - swap foreground and background colors, aka invert; inconsistent emulation (swap console default colors, not assignable by shell in full ansi)
        /// </summary>
        /// <returns>ansi seq</returns>
        public static string SGR_ReverseVideo => $"{SGR(7)}";

        /*
        /// <summary>
        /// 
        /// </summary>
        /// <returns>ansi seq</returns>
        public static string SGR_ => $"{SGR()}";
        */

        #endregion

        #region color support

        // notice: removed from start of output: {CSI}0m
        /// <summary>
        /// set colors from 4 bit index ( To3BitColorIndex(ConsoleColor) ). (@TODO: check {CSI}0m removed from bg begin)
        /// </summary>
        /// <param name="foregroundNum"></param>
        /// <param name="backgroundNum"></param>
        /// <returns></returns>
        public static string Set4BitsColors(int foregroundNum,int backgroundNum) {
            var r = "";
            if (backgroundNum>-1) r += $"{CSI}{(((backgroundNum & 0b1000) != 0) ? "4" : "10")}{backgroundNum & 0b111}m";
            if (foregroundNum>-1) r += $"{CSI}{(((foregroundNum & 0b1000) != 0)?"3":"9")}{foregroundNum & 0b111}m";
            return r;
        }

        public static string Set4BitsColorsForeground(int foregroundNum)
            => foregroundNum>-1? $"{CSI}{(((foregroundNum & 0b1000) != 0)?"3":"9")}{foregroundNum & 0b111}m" : "";

        public static string Set4BitsColorsBackground(int backgroundNum)
            => backgroundNum>-1? $"{CSI}{(((backgroundNum & 0b1000) != 0) ? "4" : "10")}{backgroundNum & 0b111}m" : "";

        public static (int colorNum, bool isDark) To4BitColorIndex(ConsoleColor c)
        {
            if (Enum.TryParse<Color4BitMap>((c + "").ToLower(), out var colbit))
            {
                var num = (int)colbit & 0b111;
                var isDark = ((int)colbit & 0b1000) != 0;
                return (num, isDark);
            }
            else
                return ((int)Color4BitMap.gray, false);
        }

        public static int To4BitColorNum(ConsoleColor? c)
        {
            if (c==null) return -1;
            if (Enum.TryParse<Color4BitMap>((c + "").ToLower(), out var colbit))
            {
                var num = (int)colbit;
                return num;
            }
            else
                return (int)Color4BitMap.gray;
        }

        #endregion
    }
}
