using DotNetConsoleAppToolkit.Component.CommandLine.Processor;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetConsoleAppToolkit.Console
{
    public class EchoPrimitiveMap
    {
        public bool MappedCall(
            object obj,
            EchoEvaluationContext context
            )
        {
            obj = obj ?? throw new NullReferenceException();
            return false;
        }

    }
}
