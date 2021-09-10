using System;

namespace OrbitalShell.Lib
{
    public static class ObjectExt
    {
        public static T Apply<T>(this T o, Action<T> apply)
        {
            apply(o);
            return o;
        }
    }
}
