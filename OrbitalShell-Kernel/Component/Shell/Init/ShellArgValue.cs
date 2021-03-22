using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbitalShell.Component.Shell.Init
{
    public class ShellArgValue
    {
        public readonly ShellArg ShellArg;

        public readonly string ArgValue;

        public ShellArgValue(ShellArg shellArg, string argValue)
        {
            ShellArg = shellArg;
            ArgValue = argValue;
        }
    }
}
