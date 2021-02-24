using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


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

            var returnCode = Shell.Startup(scope.ServiceProvider,args);

            App.Host.RunAsync().Wait();

            return returnCode;
        }
    }
}

