using System.Reflection;

namespace OrbitalShell.Component.Shell.Hook
{
    /// <summary>
    /// hook specification
    /// </summary>
    public class HookSpecification
    {
        public readonly object Owner;

        public readonly MethodInfo Method;

        public string Name;

        public HookSpecification(string name, object owner, MethodInfo method)
        {
            Owner = owner;
            Method = method;
            Name = name;
        }
    }
}