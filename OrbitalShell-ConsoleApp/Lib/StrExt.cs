using System.Collections.Generic;
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
            if (j < s.Length) r.Add(new string(s.Substring(j)));
            return r;
        }
    }
}