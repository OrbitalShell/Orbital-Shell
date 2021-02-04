using System;

namespace OrbitalShell.Component.Shell.Module
{
    /// <summary>
    /// indicates that a class own shell hooks
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class HooksAttribute : Attribute
    {
        public HooksAttribute(
        )
        {
        }
    }
}
