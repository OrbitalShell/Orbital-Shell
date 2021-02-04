using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrbitalShell.Lib
{
    public static class Str
    {
        #region data to text operations

        public static bool IsQuotedString(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            return s[0] == s[s.Length - 1] && (s[0] == '\'' || s[0] == '"');
        }

        public static string AssureIsQuoted(string s) => IsQuotedString(s) ? s : DoubleQuoted(s);

        public static string DoubleQuoted(string s)
        {
            return $"\"{s}\"";
        }

        public static string Quoted(string s)
        {
            return $"'{s}'";
        }

        public static string DoubleQuoteIfString(object o)
        {
            if (o == null) return null;
            if (o is string str
                && !str.StartsWith("\"")
                && !str.EndsWith("\"")
                )
            {
                str = "\"" + str + "\"";
                return str;
            }
            return o.ToString();
        }

        /// <summary>
        /// null output fallback
        /// </summary>
        public static string DumpNullStringAsText = "{null}";

        public static string DumpAsText(object o, bool quoteStrings = true)
        {
            if (o == null) return DumpNullStringAsText ?? null;
            if (o is string s && quoteStrings) return $"\"{s}\"";
            return o.ToString();
        }

        public static string TimeSpanDescription(TimeSpan ts, string prefix = "", string postfix = "")
        {
            var d = ts.Days;
            var h = ts.Hours;
            var m = ts.Minutes;
            var s = ts.Seconds;
            var ms = ts.Milliseconds;
            var lst = new List<String>();
            if (d > 0) lst.Add($"{prefix}{d}{postfix} days");
            if (h > 0) lst.Add($"{prefix}{h}{postfix} hours");
            if (m > 0) lst.Add($"{prefix}{m}{postfix} minutes");
            if (s > 0) lst.Add($"{prefix}{s}{postfix} seconds");
            if (ms >= 0) lst.Add($"{prefix}{ms}{postfix} milliseconds");
            return string.Join(' ', lst);
        }

        public static string Plur(string s, int n, string postfix = "") => $"{n}{postfix} {s}" + ((n > 1) ? "s" : "");

        public static string Dump(object[] t)
        {
            return string.Join(',', t.Select(x => DumpAsText(x)));
        }

        /// <summary>
        /// indicates if a string match one with wildcards<br/>
        /// author: PrzemekBenz https://www.codeproject.com/script/Membership/View.aspx?mid=629898<br/>
        /// from: https://www.codeproject.com/tips/57304/use-wildcard-characters-and-to-compare-strings<br/> 
        /// licence: CPOL 27/7/2017<br/>
        /// </summary>        
        /// <param name="pattern">wildcard pattern string</param>
        /// <param name="input">string to check if match</param>
        /// <param name="ignoreCase">ignore case if true</param>
        /// <returns>true if string match, false otherwise</returns>
        public static bool MatchWildcard(
            String pattern,
            String input, bool ignoreCase)
        {
            if (String.Compare(pattern, input, ignoreCase) == 0)
            {
                return true;
            }
            else if (String.IsNullOrEmpty(input))
            {
                if (String.IsNullOrEmpty(pattern.Trim(new Char[1] { '*' })))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (pattern.Length == 0)
            {
                return false;
            }
            else if (pattern[0] == '?')
            {
                return MatchWildcard(pattern.Substring(1), input.Substring(1), ignoreCase);
            }
            else if (pattern[pattern.Length - 1] == '?')
            {
                return MatchWildcard(pattern.Substring(0, pattern.Length - 1),
                                           input.Substring(0, input.Length - 1),
                                           ignoreCase);
            }
            else if (pattern[0] == '*')
            {
                if (MatchWildcard(pattern.Substring(1), input, ignoreCase))
                {
                    return true;
                }
                else
                {
                    return MatchWildcard(pattern, input.Substring(1), ignoreCase);
                }
            }
            else if (pattern[pattern.Length - 1] == '*')
            {
                if (MatchWildcard(pattern.Substring(0, pattern.Length - 1), input, ignoreCase))
                {
                    return true;
                }
                else
                {
                    return MatchWildcard(pattern, input.Substring(0, input.Length - 1), ignoreCase);
                }
            }
            else if (pattern[0] == input[0])
            {
                return MatchWildcard(pattern.Substring(1), input.Substring(1), ignoreCase);
            }
            return false;
        }

        public static string HumanFormatOfSize(long bytes, int digits = 1, string sep = " ", string prefix = "", string postfix = "", string bigPostFix = "")
        {
            var byteChar = 'b';
            long absB = bytes == long.MinValue ? long.MaxValue : Math.Abs(bytes);
            if (absB < 1024)
            {
                return prefix + bytes + postfix + sep + byteChar;
            }
            if (absB < 1024 * 1024)
            {
                return String.Format(prefix + "{0:F" + digits + "}" + postfix + sep + "{1}" + bigPostFix + byteChar, absB / 1024d, "K");
            }
            long value = absB;
            Stack<long> values = new Stack<long>();
            char ci = '?';
            var t = new char[] { 'K', 'M', 'G', 'T', 'P', 'E' };
            int n = 0;
            while (n < t.Length && value > 0)
            {
                for (int i = 40; i >= 0 && absB > 0xfffccccccccccccL >> i; i -= 10)
                {
                    value >>= 10;
                    if (value > 0)
                    {
                        ci = t[n++];
                        values.Push(value);
                    }
                }
            }
            value = values.Pop();
            if (values.Count > 0) value = values.Pop();
            value *= Math.Sign(bytes);
            return String.Format(prefix + "{0:F" + digits + "}" + postfix + sep + "{1}" + bigPostFix + byteChar, value / 1024d, ci);
        }

        #endregion
    }
}
