using System;
using System.Linq;

namespace OrbitalShell.Commands.NuGetServerApi
{
    public class QueryResultRoot
    {
        public int TotalHits;

        public Package[] Data;

        public override string ToString()
        {
            var r = Environment.NewLine;
            string sep = "".PadLeft(30, '-') + r;
            return ($"TotalHits={TotalHits}{r}" +
                ((Data != null && Data.Length > 0) ?
                    $"Packages list:{r}" + $"{string.Join("", Data.Select(x => sep + x.ToString()))}"
                    : "")).Trim();
        }
    }
}
