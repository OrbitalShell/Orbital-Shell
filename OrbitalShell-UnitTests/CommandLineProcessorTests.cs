using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

            //clr.BeginReadln(null, waitForReaderExited: false);    // this sentence and next mismatch on Loop parameter
            //clr.SendInput("com-crash-test",waitEndOfInput:true);    // change the function responsability: must assume begin readln
        }
    }
}
