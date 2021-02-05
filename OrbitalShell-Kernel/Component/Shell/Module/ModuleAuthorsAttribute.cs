using System;

namespace OrbitalShell.Component.Shell.Module
{
    /// <summary>
    /// declare authors of a shell module
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class ModuleAuthorsAttribute : Attribute
    {
        public readonly string[] Auhors;

        public ModuleAuthorsAttribute(params string[] authors)
        {
            Auhors = authors;
        }
    }
}