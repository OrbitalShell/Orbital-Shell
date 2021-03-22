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

        /// <summary>
        /// text of the argument
        /// </summary>
        public readonly string ArgText;

        /// <summary>
        /// argument value (option parameter is applyable, else next arg in args sequence)
        /// </summary>
        public readonly string ArgValue;

        /// <summary>
        /// eventually arg index in args list
        /// </summary>
        public int ArgIndex;

        public ShellArgValue(
            ShellArg shellArg, 
            string argText,
            string argValue = null
            )
        {
            ShellArg = shellArg;
            ArgValue = argValue;
            ArgText = argText;
        }
    }
}
