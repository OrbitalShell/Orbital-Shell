using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbitalShell.Component.CommandLine.Processor
{
    /// <summary>
    /// command operation context settings
    /// </summary>
    public class CommandEvaluationContextSettings
    {
        public bool IsQuiet { get; set; }

        public bool HasConsole { get; set; }

        public bool IsInteractive { get; set; }
    }
}
