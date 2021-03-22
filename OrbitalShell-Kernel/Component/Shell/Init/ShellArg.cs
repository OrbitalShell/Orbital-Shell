using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbitalShell.Component.Shell.Init
{
    public class ShellArg
    {
        public readonly string ShortName;

        public readonly string LongName;

        public ShellArg(string shortName, string longName)
        {
            ShortName = shortName;
            LongName = longName;
        }
    }
}
