using System;

namespace DotNetConsoleAppToolkit.Console
{
    public static class ANSI
    {
        public static readonly string ESC = ((char)27) + "";
        public static readonly string CSI = $"{ESC}[";
        public static readonly string CRLF = (char)13 + ((char)10 + "");

        public static readonly string DECSC = ESC+"7";
        public static readonly string DECRC = ESC+"8";

        public enum EDparameter {

            /// <summary>
            /// clear from cursor the end of screen
            /// </summary>
            p0 = 0,

            /// <summary>
            /// 
            /// </summary>
            p1 = 1,

            /// <summary>
            /// 
            /// </summary>
            p2 = 2,

            /// <summary>
            /// 
            /// </summary>
            p3 = 3

        }

        public static string ED(EDparameter n) => $"{CSI}{(int)n}J";

        // notice: removed from start of output: {CSI}0m
        public static string Set3BitsColors(int foregroundNum,int backgroundNum) =>
            $"{CSI}0m{CSI}{(((backgroundNum & 0b1000) != 0) ? "4" : "10")}{backgroundNum & 0b111}m{CSI}{(((foregroundNum & 0b1000) != 0)?"3":"9")}{foregroundNum & 0b111}m";

        public static (int colorNum, bool isDark) To3BitColorIndex(ConsoleColor c)
        {
            if (Enum.TryParse<Color3BitMap>((c + "").ToLower(), out var colbit))
            {
                var num = (int)colbit & 0b111;
                var isDark = ((int)colbit & 0b1000) != 0;
                return (num, isDark);
            }
            else
                return ((int)Color3BitMap.gray, false);
        }

        public static int To3BitColorNum(ConsoleColor c)
        {
            if (Enum.TryParse<Color3BitMap>((c + "").ToLower(), out var colbit))
            {
                var num = (int)colbit;
                return num;
            }
            else
                return (int)Color3BitMap.gray;
        }
    }
}
