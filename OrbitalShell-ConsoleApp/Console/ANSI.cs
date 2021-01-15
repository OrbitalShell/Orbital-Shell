using System;

namespace DotNetConsoleAppToolkit.Console
{
    /// <summary>
    /// all ansi valid codes, even for 'low ansi terminals'.
    /// from https://en.wikipedia.org/wiki/ANSI_escape_code
    /// </summary>
    public static class ANSI
    {
        public static readonly string ESC = ((char)27) + "";
        public static readonly string CSI = $"{ESC}[";
        public static readonly string CRLF = (char)13 + ((char)10 + "");

        /// <summary>
        /// backup cursor position
        /// </summary>
        public static readonly string DECSC = ESC+"7";

        /// <summary>
        /// restore cursor position
        /// </summary>
        public static readonly string DECRC = ESC+"8";

        /// <summary>
        /// RESET TEXT ATTRIBUTES : console background (if transparency preserved), text attributes (uon,tdoff)
        /// </summary>        
        public static readonly string RSTXTA = ESC+"[4m" + ESC+"[0m";
        public static readonly string RSTXTA2 = ESC+"[4m" + " " + ESC+"[0m";

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
            /// </summary>
            p2 = 2,

            /// <summary>
            ///  If n is 3, clear entire screen and delete all lines saved in the scrollback buffer (this feature was added for xterm and is supported by other terminal applications).
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

        #region color support

        // notice: removed from start of output: {CSI}0m
        /// <summary>
        /// set colors from 4 bit index ( To3BitColorIndex(ConsoleColor) )
        /// </summary>
        /// <param name="foregroundNum"></param>
        /// <param name="backgroundNum"></param>
        /// <returns></returns>
        public static string Set4BitsColors(int foregroundNum,int backgroundNum) {
            var r = "";
            if (backgroundNum>-1) r += $"{CSI}0m{CSI}{(((backgroundNum & 0b1000) != 0) ? "4" : "10")}{backgroundNum & 0b111}m";
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
