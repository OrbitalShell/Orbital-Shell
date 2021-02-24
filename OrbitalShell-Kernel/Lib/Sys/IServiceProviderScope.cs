using System;

namespace OrbitalShell.Lib.Sys
{
    public interface IServiceProviderScope
    {
        IServiceProvider ServiceProvider { get; set; }
    }
}