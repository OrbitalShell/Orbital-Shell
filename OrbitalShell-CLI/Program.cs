using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using OrbitalShell.Component.Shell;

namespace OrbitalShell
{
    public static class Program
    {
        static async Task<int> Main(string[] args)
            => await RunShell(args);

        public static async Task<int> RunShell(string[] args)
        {
            var shellStartup = InitializeShell(args);

            var returnCode = shellStartup.Startup(args);

            await App.Host.RunAsync();

            return returnCode;
        }

        public static IShellStartup InitializeShell(string[] args)
        {
            App.InitializeServices(System.Array.Empty<string>());

            var si = new ServicesInitializer();
            si.InitializeServices(App.HostBuilder);

            App.EndInitializeServices();

            var scope = App.Host.Services.CreateScope();
            si.ScopedServiceProvider = scope.ServiceProvider;

            return scope.ServiceProvider.GetRequiredService<IShellStartup>();
        }
    }
}

