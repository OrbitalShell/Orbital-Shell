using DotNetConsoleAppToolkit.Component.CommandLine.Data;
using DotNetConsoleAppToolkit.Component.CommandLine.Processor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace DotNetConsoleAppToolkit.Component.CommandLine.Variable
{
    /// <summary>
    /// variables of the shell
    /// </summary>
    public class ShellEnvironment : DataObject
    {
        public ShellEnvironment(string name) : base(name, false)
        {

        }

        public void Initialize(CommandEvaluationContext context)
        {
            
        }
    }
}
