using System;

namespace DotNetConsoleAppToolkit.Console
{
    /// <summary>
    /// all ANSI codes that we take into account for the targetted terminals referential
    /// from https://en.wikipedia.org/wiki/ANSI_escape_code
    /// </summary>
    public static class ANSI
    {
        #region char codes and prefixs

        public static readonly string ESC = ((char)27) + "";

        public static readonly string CRLF = Environment.NewLine; //(char)13 + ((char)10 + ""); // @TODO: validate on linux

        public static readonly string CSI = $"{ESC}[";

        public static string SGR(int n,string seq=null) => $"{ESC}[{n}{(string.IsNullOrWhiteSpace(seq)?"":$";{seq}")}m";
        public static string SGR(string seq=null) => $"{ESC}[{seq}m";

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
        /// you should wait after launch this seq about about 25ms before it is efficient (vscode debug console)
        /// </summary>        
        public static readonly string RSTXTA = CSI + "4m" + CSI + "0m";

        /// <summary>
        /// reset terminal colors (foreground and background) to their default (not supported on some terminals)
        /// </summary>
        public static readonly string RSCOLDEF = CSI + "[39;49m";

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

        public enum EDParameter {

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
        public static string ED(EDParameter n) => $"{CSI}{(int)n}J";

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

        /// <summary>
        /// Italic off
        /// </summary>
        /// <returns>ansi seq</returns>
        public static string SGR_ItalicOff => $"{SGR(23)}";

        /// <summary>
        /// Underline off - Not singly or doubly underlined
        /// </summary>
        /// <returns>ansi seq</returns>
        public static string SGR_UnderlineOff => $"{SGR(24)}";

        /// <summary>
        /// Blink off
        /// </summary>
        /// <returns>ansi seq</returns>
        public static string SGR_BlinkOff => $"{SGR(25)}";

        /// <summary>
        /// Reverse/invert off
        /// </summary>
        /// <returns>ansi seq</returns>
        public static string SGR_ReverseOff => $"{SGR(27)}";

        /// <summary>
        /// Not crossed out
        /// </summary>
        /// <returns>ansi seq</returns>
        public static string SGR_NotCrossedOut => $"{SGR(29)}";

        /// <summary>
        /// Crossed-out - aka Strike, characters legible but marked as if for deletion. (do nothing on vscode debuger, works on windows terminal)
        /// </summary>
        /// <returns>ansi seq</returns>
        public static string SGR_CrossedOut => $"{SGR(9)}";

        /// <summary>
        /// Doubly underline or Bold off - Double-underline per ECMA-48.
        /// </summary>
        /// <returns>ansi seq</returns>
        public static string SGR_DoubleUnderline => $"{SGR(21)}";

        /// <summary>
        /// Normal color or intensity - Neither bold nor faint
        /// </summary>
        /// <returns>ansi seq</returns>
        public static string SGR_NormalIntensity => $"{SGR(22)}";

        /// <summary>
        /// SGR_4BitsColors parameter values for SGR_SetForegroundColor4bits and SGR_SetBackgroundColor4bits
        /// </summary>
        public enum SGR_4BitsColors {
            Black = 0,
            Red = 1,
            Green = 2,
            Yellow = 3,
            Blue = 4,
            Magenta = 5,
            Cyan = 6,
            White = 7
        }

        /// <summary>
        /// Set foreground color - 3/4 bits palette mode (8 normal colors + 8 bright colors)
        /// <para>bright black is gray or darkgray, white is gray or light gray, bright white is white. That depends on console palette</para>
        /// </summary>
        /// <param name="color">SGR_4BitsColors</param>
        /// <param name="bright">enable bright 8 additional color set (bright colors)</param>
        /// <returns>ansi seq</returns>
        public static string SGR_SetForegroundColor4bits(SGR_4BitsColors color,bool bright) => $"{SGR(""+((bright?90:30)+(int)color))}";

        /// <summary>
        /// Set foreground color - 3/4 bits palette mode (8 normal colors + 8 bright colors)
        /// <para>bright black is gray or darkgray, white is gray or light gray, bright white is white. That depends on console palette</para>
        /// <para>format is {n}[,bright] with n from any name in SGR_4BitsColors (both args not in sensitive case). if ,bright is added, the color refers to the 8 colors additional palette - for example Black,bright or red</para>
        /// <param name="s">{n}[,bright]</param>
        /// </summary>
        public static string SGRF(object o) {
            if (!(o is string s)) return null;
            var p = ParseSGR_4BitsColor(s);
            var r = SGR_SetForegroundColor4bits(p.color,p.bright);
            return r;
        }

        /// <summary>
        /// format is {n}[,bright] with n from any name in SGR_4BitsColors (both args not in sensitive case). if ,bright is added, the color refers to the 8 colors additional palette - for example Black,bright or red
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static (SGR_4BitsColors color,bool bright) ParseSGR_4BitsColor(string s) {
            try {
                var t = s.Split(":");
                var label = t[0];
                var bright = t.Length==2 && t[1].ToLower()=="bright";
                if (Enum.TryParse<SGR_4BitsColors>(label,true,out var sgr4bc)) {
                    return (sgr4bc,bright);
                } else
                    throw new Exception("label not in SGR_4BitsColors");
            } catch (Exception ex) {
                throw new Exception("invalid SGR_4BitsColor definition syntax. attempted is: {n}[:bright] with n from any name in SGR_4BitsColors in lower case",ex);
            }
        }

        /// <summary>
        /// set foreground color - 8 bits palette (256 colors)
        /// <para>0-  7:  standard colors (as in ESC [ 30–37 m, see SGR_4BitsColors)<br/>
        /// 8- 15:  high intensity colors (as in ESC [ 90–97 m, see SGR_4BitsColors bright)<br/>
        /// 16-231:  6 × 6 × 6 cube (216 colors): 16 + 36 × r + 6 × g + b (0 ≤ r, g, b ≤ 5)<br/>
        /// 232-255:  grayscale from black to white in 24 steps</para>
        /// </summary>
        /// <param name="n">palette color index</param>
        /// <returns>ansi seq</returns>
        public static string SGR_SetForegroundColor8bits(int n) => $"{SGR(38,$"5;{n}")}";

        /// <summary>
        /// set foreground color - 8 bits palette (256 colors)
        /// <para>0-  7:  standard colors (as in ESC [ 30–37 m, see SGR_4BitsColors)<br/>
        /// 8- 15:  high intensity colors (as in ESC [ 90–97 m, see SGR_4BitsColors bright)<br/>
        /// 16-231:  6 × 6 × 6 cube (216 colors): 16 + 36 × r + 6 × g + b (0 ≤ r, g, b ≤ 5)<br/>
        /// 232-255:  grayscale from black to white in 24 steps</para>
        /// <para>format is {n},0<=n<=255</para>
        /// </summary>
        /// <param name="s">{n},0<=n<=255</param>
        /// <returns>ansi seq</returns>
        public static string SGRF8(object o) {
            if (!(o is string s)) return null;
            try {
                var n = Convert.ToInt16(s);
                if (n<0 || n>255) throw new Exception("value out of bounds (8its max)");
                return SGR_SetForegroundColor8bits(n);
            } catch (Exception ex) {
                throw new Exception("invalid SGR ;5;n definition syntax. attempted is: {n},0<=n<=255",ex);
            }
        }

        /// <summary>
        /// set foreground color - 24 bits 'true color' (for 16 or 24 bits palette graphic cards)
        /// <para>parameters are red,green,blue luminosity from 0 to 255</para>
        /// </summary>
        /// <param name="r">red: 0 to 255</param>
        /// <param name="g">green: 0 to 255</param>
        /// <param name="b">blue: 0 to 255</param>
        /// <returns>ansi seq</returns>
        public static string SGR_SetForegroundColor24bits(int r,int g,int b) => $"{SGR(38,$"2;{r};{g};{b}")}";

        /// <summary>
        /// set foreground color - 24 bits 'true color' (for 16 or 24 bits palette graphic cards)
        /// <para>parameters are red,green,blue luminosity from 0 to 255</para>
        /// <para>format is: {r}:{g}:{b} 0<=r<=255 0<=g<=255 0<=b<=255</para>
        /// </summary>
        /// <param name="s">{r}:{g}:{b} 0<=r<=255 0<=g<=255 0<=b<=255</param>
        /// <returns>ansi seq</returns>
        public static string SGRF24(object o) {
            if (!(o is string s)) return null;
            try {
                int convert(string s) {
                    var n = Convert.ToInt16(s);
                    if (n<0 || n>255) throw new Exception("value out of bounds (8bits max)");
                    return n;
                }
                var t = s.Split(":");
                if (t.Length!=3) throw new Exception("wrong numbers of parameters.. attempted exactly two comma");
                var r = convert(t[0]);
                var g = convert(t[1]);
                var b = convert(t[2]); 
                return SGR_SetForegroundColor24bits(r,g,b);
            } catch (Exception ex) {
                throw new Exception("invalid SGR ;2;r;g;b definition syntax. attempted is: {r}:{g}:{b} 0<=r<=255 0<=g<=255 0<=b<=255",ex);
            }
        }

        /// <summary>
        /// Set background color - 3/4 bits palette mode (8 normal colors + 8 bright colors)
        /// <para>bright black is gray or darkgray, white is gray or light gray, bright white is white. That depends on console palette</para>
        /// </summary>
        /// <param name="color">SGR_4BitsColors</param>
        /// <param name="bright">enable bright 8 additional color set (sames colors in bright)</param>
        /// <returns>ansi seq</returns>
        public static string SGR_SetBackgroundColor4bits(SGR_4BitsColors color,bool bright) => $"{SGR(""+((bright?100:40)+(int)color))}";

        /// <summary>
        /// Set background color - 3/4 bits palette mode (8 normal colors + 8 bright colors)
        /// <para>bright black is gray or darkgray, white is gray or light gray, bright white is white. That depends on console palette</para>
        /// <para>format is {n}[,bright] with n from any name in SGR_4BitsColors (both args not in sensitive case). if ,bright is added, the color refers to the 8 colors additional palette - for example Black,bright or red</para>
        /// </summary>
        public static string SGRB(object o) {
            if (!(o is string s)) return null;
            var p = ParseSGR_4BitsColor(s);
            return SGR_SetBackgroundColor4bits(p.color,p.bright);
        }

        /// <summary>
        /// set background color - 8 bits palette (256 colors)
        /// <para>0-  7:  standard colors (as in ESC [ 30–37 m, see SGR_4BitsColors)<br/>
        /// 8- 15:  high intensity colors (as in ESC [ 90–97 m, see SGR_4BitsColors bright)<br/>
        /// 16-231:  6 × 6 × 6 cube (216 colors): 16 + 36 × r + 6 × g + b (0 ≤ r, g, b ≤ 5)<br/>
        /// 232-255:  grayscale from black to white in 24 steps</para>
        /// </summary>
        /// <param name="n">palette color index</param>
        /// <returns>ansi seq</returns>
        public static string SGR_SetBackgroundColor8bits(int n) => $"{SGR(48,$"5;{n}")}";

        /// <summary>
        /// set background color - 8 bits palette (256 colors)
        /// <para>0-  7:  standard colors (as in ESC [ 30–37 m, see SGR_4BitsColors)<br/>
        /// 8- 15:  high intensity colors (as in ESC [ 90–97 m, see SGR_4BitsColors bright)<br/>
        /// 16-231:  6 × 6 × 6 cube (216 colors): 16 + 36 × r + 6 × g + b (0 ≤ r, g, b ≤ 5)<br/>
        /// 232-255:  grayscale from black to white in 24 steps</para>
        /// <para>format is {n},0<=n<=255</para>
        /// </summary>
        /// <param name="s">{n},0<=n<=255</param>
        /// <returns>ansi seq</returns>
        public static string SGRB8(object o) {
            if (!(o is string s)) return null;
            try {
                var n = Convert.ToInt16(s);
                if (n<0 || n>255) throw new Exception("value out of bounds for 8bits color palette");
                return SGR_SetBackgroundColor8bits(n);
            } catch (Exception ex) {
                throw new Exception("invalid SGR ;5;n definition syntax. attempted is: {n},0<=n<=255",ex);
            }
        }

        /// <summary>
        /// set background color - 24 bits 'true color' (for 16 or 24 bits palette graphic cards)
        /// <para>parameters are red,green,blue luminosity from 0 to 255</para>
        /// </summary>
        /// <param name="r">red: 0 to 255</param>
        /// <param name="g">green: 0 to 255</param>
        /// <param name="b">blue: 0 to 255</param>
        /// <returns>ansi seq</returns>
        public static string SGR_SetBackgroundColor24bits(int r,int g,int b) => $"{SGR(48,$"2;{r};{g};{b}")}";

        /// <summary>
        /// set background color - 24 bits 'true color' (for 16 or 24 bits palette graphic cards)
        /// <para>parameters are red,green,blue luminosity from 0 to 255</para>
        /// <para>format is: {r}:{g}:{b} 0<=r<=255 0<=g<=255 0<=b<=255</para>
        /// </summary>
        /// <param name="s">{r}:{g}:{b} 0<=r<=255 0<=g<=255 0<=b<=255</param>
        /// <returns>ansi seq</returns>
        public static string SGRB24(object o) {
            if (!(o is string s)) return null;
            try {
                int convert(string s) {
                    var n = Convert.ToInt16(s);
                    if (n<0 || n>255) throw new Exception("value out of bounds (8bits max)");
                    return n;
                }
                var t = s.Split(":");
                if (t.Length!=3) throw new Exception("wrong numbers of parameters.. attempted exactly two ,");
                var r = convert(t[0]);
                var g = convert(t[1]);
                var b = convert(t[2]); 
                return SGR_SetBackgroundColor24bits(r,g,b);
            } catch (Exception ex) {
                throw new Exception("invalid SGR ;2;r;g;b definition syntax. attempted is: {r}:{g}:{b} 0<=r<=255 0<=g<=255 0<=b<=255",ex);
            }
        }

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

        #region ANSI string util

        public static string AvoidANSISequencesAndNonPrintableCharacters(string s) {
            s = s.Replace(""+(char)27,"\\x1b");
            var r = "";
            foreach (var c in s)
                r += (c<32)?ToHexStr(c):""+c;
            return r;
        }

        static string ToHexStr(int n) {
            var s = string.Format("\\x{0:X}",n);
            return s;
        }

        /// <summary>
        /// TODO: returns true if the sequence starts by a ANSI sequence
        /// <para>from https://en.wikipedia.org/wiki/ANSI_escape_code</para>
        /// </summary>
        /// <param name="s">string to be analyzed</param>
        /// <returns>several information about any ansi sequence or not</returns>
        public static (bool startsWithANSISequence,string ansiSequence,int nextIndex) StartsWithANSISequence(string s) 
        {

            return (false,null,1);
        }

        #endregion
    }
}
