using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using OrbitalShell.Component.Shell;

namespace OrbitalShell
{
    public static class Program
    {
        static int Main(string[] args)
        {
            var returnCode =
                GetShellServiceHost(args)
                .InitializeShellServiceHost(args);

            App.Host.Run();

            return returnCode;
        }

        public static IShellServiceHost GetShellServiceHost(string[] args)
        {
            App.InitializeServices(System.Array.Empty<string>());

            var si = new ServicesInitializer();
            si.InitializeServices(App.HostBuilder);

            App.EndInitializeServices();

            var scope = App.Host.Services.CreateScope();
            si.ScopedServiceProvider = scope.ServiceProvider;

            return scope.ServiceProvider.GetRequiredService<IShellServiceHost>();
        }
    }
}

