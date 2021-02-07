using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrbitalShell.Lib
{
    /// <summary>
    /// string util extensions methods
    /// </summary>
    public static class StrExt
    {
        public static List<string> SplitNotUnslashed(this string s, char c)
        {
            var r = new List<string>();
            var j = 0;
            bool matchsep;
            for (int i = 0; i < s.Length; i++)
                if ((matchsep = s[i] == c) && i > 0 && s[i] - 1 != '\\')
                {
                    r.Add(new string(s.Substring(j, i - j)));
                    j = i + 1;
                }
                else if ((matchsep = s[i] == c) && i == 0)
                {
                    r.Add("");
                    j = i + 1;
                }
            if (j < s.Length) r.Add(new string(s.Substring(j)));
            return r;
        }

        public static List<string> SplitByPrefixsNotUnslashed(this string s, List<char> chars)
        {
            var r = new List<string>();
            var j = 0;
            bool matchsep;
            s = new string(s.Reverse().ToArray());
            for (int i = 0; i < s.Length; i++)
                if ((matchsep = chars.Contains(s[i])) && i > 0 && s[i] - 1 != '\\')
                {
                    r.Add(new string(s.Substring(j, i - j + 1)));
                    j = i + 1;
                }
                else if ((matchsep = chars.Contains(s[i])) && i == 0)
                {
                    r.Add("");
                    j = i + 1;
                }
            if (j < s.Length) r.Add(new string(s.Substring(j)));
            return r.Select(x => new string(x.Reverse().ToArray())).ToList();
        }

        /// <summary>
        /// indicates if the string contains at least one of the characters
        /// </summary>
        /// <param name="s">string</param>
        /// <param name="chars">chars list</param>
        /// <returns></returns>
        public static bool Contains(this string s, List<char> chars)
        {
            foreach (var c in chars)
                if (s.Contains(c)) return true;
            return false;
        }

        public static bool IsUpperCase(this string s)
        {
            foreach (var c in s) if (!char.IsUpper(c)) return false;
            return true;
        }

        public static bool EndsWith(this string s, List<string> postfixs)
        {
            foreach (var p in postfixs) if (s.EndsWith(p)) return true;
            return false;
        }

        public static bool ContainsDigit(this string s)
        {
            foreach (var c in s) if (char.IsDigit(c)) return true;
            return false;
        }

        public static string RemoveDigits(this string s)
        {
            var sb = new StringBuilder();
            foreach (var c in s) if (!char.IsDigit(c)) sb.Append(c);
            return sb.ToString();
        }
    }
}