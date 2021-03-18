using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OrbitalShell_UnitTests
{
    [TestClass]
    [TestCategory("command line syntax parser")]
    public class CommandLineSyntaxParserTests
    {
        [TestMethod("test syntaxes for standard arguments types")]        
        public void ArgumentSyntaxTest()
        {
            var shellInit = BaseTests.GetInitializedShell(BaseTests.DefaultShellInitArgs);            
        }
    }
}
