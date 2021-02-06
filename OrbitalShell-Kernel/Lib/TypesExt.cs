using System;
using System.Reflection;

namespace OrbitalShell.Lib
{
    /// <summary>
    /// types extensions
    /// </summary>
    public static class TypesExt
    {
        public static object GetParameterDefaultValue(this MethodInfo mi, int pindex)
            => Activator.CreateInstance(mi.GetParameters()[pindex].ParameterType);
    }
}