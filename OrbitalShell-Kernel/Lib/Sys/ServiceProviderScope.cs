using System;

namespace OrbitalShell.Lib.Sys
{
    public class ServiceProviderScope : IServiceProviderScope
    {
        public ServiceProviderScope(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public IServiceProvider ServiceProvider { get; set; }
    }
}
