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

            clr.BeginReadln(null, waitForReaderExited: true);
            clr.SendInput("com-crash-test");
        }
    }
}
