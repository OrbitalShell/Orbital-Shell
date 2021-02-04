using System;

namespace OrbitalShell.Component.Shell.Module
{
    /// <summary>
    /// module utilitary methods
    /// </summary>
    public static class ModuleUtil
    {
        /// <summary>
        /// format and returns command declaring type from given type class name
        /// </summary>
        /// <param name="type">a command declaring type</param>
        /// <returns>camel case name , ends 'Commands' removed if present</returns>
        public static string DeclaringTypeShortName(Type type)
        {
            var r = type.Name;
            var i = r.LastIndexOf("Commands");
            if (i > 0)
                r = r.Substring(0, i);
            return r;
        }
    }
}