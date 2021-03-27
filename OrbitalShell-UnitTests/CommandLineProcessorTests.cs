using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using OrbitalShell.Component.CommandLine.Processor;

namespace OrbitalShell_UnitTests
{
    [TestClass]
    [TestCategory("command line processor")]
    public class CommandLineBehaviolarTests
    {
        [TestMethod("crash of a command")]        
        public void CommandCrashTest()
        {
            var shell = BaseTests.GetInitializedShell(BaseTests.DefaultShellInitArgs);
            var clr = shell
                .GetCommandLineProcessor()
                .CommandLineReader;

            var (asyncResult, evalResult) = clr.SendInput("com-crash-test");

            Assert.IsNotNull(evalResult.EvalError);
            Assert.AreEqual(evalResult.EvalResultCode, (int)ReturnCode.Error);
        }
    }
}
