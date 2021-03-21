using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrbitalShell;
using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Shell;

namespace OrbitalShell_UnitTests
{
    [TestClass]
    [TestCategory("shell init")]
    public class BaseTests
    {
        public static string[] DefaultShellInitArgs => new string[] { "" };

        [TestMethod("starts a non interactive shell")]
        public void TestShellNoneInteractiveStartup()
        {
            var shellInitializer = GetInitializedShell(DefaultShellInitArgs);
            // no exception -> test ok
        }

        /*[TestMethod("starts an interactive shell")]
        public void TestShellInteractiveStartup()
        {
            var shellInitializer = GetInitializedShell(DefaultShellInitArgs);
            var returnCode =
                shellInitializer
                    .GetCommandLineProcessor()
                    .CommandLineReader
                    .ReadCommandLine();     // never ends -> test never ok/fail

            Assert.AreEqual((int)ReturnCode.OK,returnCode);
        }*/

        /// <summary>
        /// returns a shell that is initialized (user profile is loaded)
        /// </summary>
        /// <param name="args">shell command line arguments</param>
        /// <returns>a shell initializer</returns>
        public static ShellBootstrap GetInitializedShell(string[] args)
        {
            var st = Program.GetShellServiceHost(args);
            Assert.IsNotNull(st);
            var si = st.GetShellBootstrap(args).Run();
            Assert.IsNotNull(si);
            return si;
        }
    }
}
