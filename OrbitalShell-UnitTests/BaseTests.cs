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
        }

        [TestMethod("starts an interactive shell")]
        public void TestShellInteractiveStartup()
        {
            var shellInitializer = GetInitializedShell(DefaultShellInitArgs);
            var returnCode =
                shellInitializer
                    .GetCommandLineProcessor()
                    .CommandLineReader
                    .ReadCommandLine();

            if (returnCode != (int)ReturnCode.OK)
                Assert.Fail();
        }

        /// <summary>
        /// returns a shell that is initialized (user profile is loaded)
        /// </summary>
        /// <param name="args">shell command line arguments</param>
        /// <returns>a shell initializer</returns>
        public static ShellInitializer GetInitializedShell(string[] args)
        {
            var st = Program.InitializeShell(args);
            if (st == null) Assert.Fail();
            var si = st.GetShellInitializer(args).Run();
            if (si == null) Assert.Fail();
            return si;
        }
    }
}
