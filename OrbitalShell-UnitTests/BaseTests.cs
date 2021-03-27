using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrbitalShell;
using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Shell;
using OrbitalShell.Component.Shell.Init;

using System.Linq;

namespace OrbitalShell_UnitTests
{
    [TestClass]
    [TestCategory("shell init")]
    public class BaseTests
    {
        public static string[] DefaultShellInitArgs = "--quiet --no-console --no-interact".Split(' ');

        [TestMethod("starts a non interactive shell")]
        public void TestShellNoneInteractiveStartup()
        {
            var shellInitializer = GetInitializedShell(DefaultShellInitArgs);
            // no exception -> test ok
        }

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
