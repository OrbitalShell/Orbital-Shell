using System;
using OrbitalShell.Component.CommandLine;

namespace OrbitalShell.Component.Shell.Hook
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class HookAttribute : Attribute
    {
        public readonly string HookName;

        public HookAttribute(
            Hooks hookName
        )
        {
            HookName = hookName + "";
        }

    }
}
