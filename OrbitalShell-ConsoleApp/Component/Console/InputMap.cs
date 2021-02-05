using System;

namespace OrbitalShell.Component.Console
{
    public class InputMap
    {
        public const int ExactMatch = 0;
        public const int NoMatch = -1;
        public const int PartialMatch = 1;

        public readonly string Text;
        public readonly object Code;
        /// <summary>
        /// -1 : no match, 0 : exact match , 1: partial match
        /// </summary>
        public readonly Func<string, ConsoleKeyInfo, int> MatchInput;
        public readonly bool CaseSensitiveMatch;

        public InputMap(string text, bool caseSensitiveMatch = false)
        {
            Text = text;
            CaseSensitiveMatch = caseSensitiveMatch;
        }

        public InputMap(string text, object code,bool caseSensitiveMatch=false)
        {
            Text = text;
            Code = code;
            CaseSensitiveMatch = caseSensitiveMatch;
        }

        public InputMap(Func<string, ConsoleKeyInfo, int> matchInput, object code)
        {
            MatchInput = matchInput;
            Code = code;
        }

        public int Match(string input,ConsoleKeyInfo key)
        {
            if (Text != null) {
                if (Text.Equals(input, CaseSensitiveMatch ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase)) return ExactMatch;
                if (Text.StartsWith(input, CaseSensitiveMatch ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase)) return PartialMatch;
                return NoMatch;
            }
            else return (MatchInput==null)?-1 : MatchInput(input,key);
        }
    }
}
