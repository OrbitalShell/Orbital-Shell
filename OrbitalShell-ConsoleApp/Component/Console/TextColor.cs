using System;
using static OrbitalShell.DotNetConsole;
using static OrbitalShell.Component.EchoDirective.Shortcuts;
using OrbitalShell.Component.EchoDirective;

namespace OrbitalShell.Component.Console
{
    public class TextColor
    {
        ConsoleColor? _foreground;
        public ConsoleColor? Foreground
        {
            get { return _foreground.HasValue?_foreground.Value:DefaultForeground; }
            set { _foreground = value; }
        }

        ConsoleColor? _background;
        public ConsoleColor? Background
        {
            get { return _background.HasValue?_background.Value:DefaultBackground;  }
            set { _background = value; }
        }

        public readonly string ToStrPost = null;
        public readonly string ToStrPre = null;

        public TextColor(ConsoleColor? foreground, ConsoleColor? background=null)
        {
            Foreground = foreground;
            Background = background;
        }

        public TextColor(ConsoleColor? foreground, ConsoleColor? background=null,string toStrPost="",string toStrPre="")
        {
            Foreground = foreground;
            Background = background;
            ToStrPost = toStrPost;
            ToStrPre = toStrPre;
        }

        public TextColor Invert() => new TextColor(_background,_foreground);

        public override string ToString() =>
             ToStrPre + (!_foreground.HasValue ? "" : GetCmd(EchoDirectives.f + "", _foreground.Value.ToString().ToLower()))
             + (!_background.HasValue ? "" : GetCmd(EchoDirectives.b + "", _background.Value.ToString().ToLower())) + ToStrPost;

        public string ToStringForegroundOnly() => (!_foreground.HasValue ? "" : GetCmd(EchoDirectives.f + "", _foreground.Value.ToString().ToLower()));

        public string ToString(bool foregroundOnly) => 
             ToStrPre + (!_foreground.HasValue ? "" : GetCmd(EchoDirectives.f + "", _foreground.Value.ToString().ToLower()))
             + ((foregroundOnly)? "": (!_background.HasValue ? "" : GetCmd(EchoDirectives.b + "", _background.Value.ToString().ToLower()))) + ToStrPost;


        #region build and convert colors operations

        public static ConsoleColor GetColor(string colorName)
        {
            return (ConsoleColor)Enum.Parse(typeof(ConsoleColor), colorName);
        }

        /// <summary>
        /// parse a 4 bit color
        /// </summary>
        /// <param name="c">text of color name</param>
        public static ConsoleColor? ParseColor(object c)
        {
            if (c==null) return null;
            var s = (string)c;
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (Enum.TryParse(s, true, out ConsoleColor r))
                return r;
            if (TraceCommandErrors) Error($"invalid color name: {c}");
            return ConsoleColor.Black;
        }

        /// <summary>
        /// parse a 8 bit color
        /// </summary>
        /// <param name="c">string representing an integer in range 0..255 (included)</param>
        /// <returns></returns>
        public static int Parse8BitColor(object c)
        {
            if (int.TryParse((string)c, out int r) && r>=0 && r<=255)            
                return r;
            if (TraceCommandErrors) Error($"invalid 8 bit color number: {c}");
            return 255;
        }

        /// <summary>
        /// parse a 24 bit color
        /// </summary>
        /// <param name="c">string of format: r:g:b where 0<=r,g,b<=255</param>
        /// <returns></returns>
        public static (int r,int g,int b) Parse24BitColor(object c)
        {
            var s = (string)c;
            var t = s.Split(':');
            if (t.Length==3)
            {
                if (int.TryParse(t[0], out int r) && r >= 0 && r <= 255
                    && int.TryParse(t[1], out int g) && g >= 0 && g <= 255
                    && int.TryParse(t[2], out int b) && b >= 0 && b <= 255)
                    return (r, g, b);
            }
            if (TraceCommandErrors) Error($"invalid 24 bit color: {c}");
            return (255, 255, 255);
        }

        #endregion
    }
}
