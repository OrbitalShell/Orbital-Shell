using System;
using System.Linq;

using OrbitalShell.Component.Console;

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

        /// <summary>
        /// Echo method
        /// </summary>
        /// <param name="context">echo context</param>
        public void Echo(EchoEvaluationContext context)
        {
            var cols = context.CommandEvaluationContext.ShellEnv.Colors;
            var r = cols.Default + Environment.NewLine;
            var s = $"{cols.HighlightSymbol}TotalHits";
            s += $"{cols.Default}=";
            s += $"{cols.Numeric}{TotalHits}{r}{cols.Default}";
            context.Out.Echoln(s);
            if (Data != null && Data.Length > 0)
            {
                context.Out.Echoln( $"Packages list:{r}" );
                int i = 1;
                foreach (var item in Data)
                {
                    item.Echo(context);
                    if (i < Data.Length)
                        context.Out.Echoln();
                    i++;
                }
            }            
        }
    }
}
