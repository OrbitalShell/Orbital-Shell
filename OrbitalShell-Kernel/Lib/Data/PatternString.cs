using OrbitalShell.Component.CommandLine.CommandModel;
using static OrbitalShell.Lib.Str;
using System.Text.RegularExpressions;

namespace OrbitalShell.Lib.Data
{
    /// <summary>
    /// normal string or wildcard / regex string<br/>
    /// - has wildcard symbols ? or * -&gt; wildcard match
    /// - starts with \ -&gt; RegEx<br/>
    /// - else normal string (string compare)
    /// </summary>
    [CustomParameterType]
    public class PatternString
    {
        /// <summary>
        /// wrapped string
        /// </summary>
        public string Str;

        public PatternString(string s)
        {
            Str = s;
        }

        public bool HasWildcard()
        {
            return Str == null ?
                false :
                (Str.Contains('*') || Str.Contains('?'));
        }

        public bool IsRegEx() => (Str == null) ? false : Str.StartsWith("\\");

        public bool Match(string s, bool ignoreCase = false)
        {
            if (s == null) return true;
            if (Str == null) return false;
            if (IsRegEx())
            {
                var rgex = Str.Substring(1);
                // TODO: add case option to regex match
                return Regex.IsMatch(s, rgex);
            }
            if (HasWildcard())
                return MatchWildcard(Str, s, ignoreCase);
            return s == Str;
        }

        public override string ToString()
        {
            return Str?.ToString();
        }

        public static implicit operator string(PatternString ps)
        {
            return ps?.Str;
        }
    }
}