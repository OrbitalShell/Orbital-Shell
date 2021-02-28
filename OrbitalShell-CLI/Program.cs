using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using OrbitalShell.Component.Shell;

namespace OrbitalShell
{
    class Program
    {
        static int Main(string[] args)
        {
            App.InitializeServices(System.Array.Empty<string>());

            var si = new ServicesInitializer();
            si.InitializeServices(App.HostBuilder);

            App.EndInitializeServices();

            var scope = App.Host.Services.CreateScope();
            si.ScopedServiceProvider = scope.ServiceProvider;

            var shellStartup = scope.ServiceProvider.GetRequiredService<IShellStartup>();
            var returnCode = shellStartup.Startup(args);

            App.Host.RunAsync().Wait();

            return returnCode;
        }
    }
}

