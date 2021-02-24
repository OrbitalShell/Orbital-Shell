using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OrbitalShell.Component.CommandLine.Processor;

namespace OrbitalShell.Component.CommandLine.Reader
{
    public class Delegates
    {
        public delegate ExpressionEvaluationResult 
            ExpressionEvaluationCommandDelegate(
                CommandEvaluationContext context, 
                string com, 
                int outputX, 
                string postAnalysisPreExecOutput = null);

    }
}
