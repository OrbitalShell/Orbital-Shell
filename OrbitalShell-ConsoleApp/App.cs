using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using hosting = Microsoft.Extensions.Hosting;

using OrbitalShell.Component.Console;
using System;

namespace OrbitalShell
{

    /// <summary>
    /// console app bootstrap
    /// </summary>
    /// <remarks>
    /// begin DI init
    /// </remarks>
    public static class App
    {
        public static IHostBuilder HostBuilder { get; private set; }

        public static IHost Host { get; private set; }

        public static IServiceProvider ServiceProvider => Host.Services ?? throw new Exception("host is not initialized");

        public static IHostBuilder InitializeServices(string[] args = null)
        {
            args ??= Array.Empty<string>();
            HostBuilder = 
                hosting.Host
                    .CreateDefaultBuilder(args)
                    .ConfigureServices((_, services) =>
                        services.AddSingleton<IConsole,Console>());                        
            return HostBuilder;
        }

        public static void EndInitializeServices()
        {
            Host = HostBuilder.Build();
        }
    }
}
