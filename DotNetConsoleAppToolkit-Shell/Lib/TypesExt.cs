using System;
using System.Reflection;

namespace DotNetConsoleAppToolkit.Lib
{
    public static partial class TypesExt
    {
        public static MethodInfo GetEchoMethod( this object o )
        {
            if (o == null) throw new ArgumentNullException(nameof(o));
            var t = o.GetType();
            try
            {
                var mi = t.GetMethod(
                    "Echo", 
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static
                    );
                return mi;
            } catch (Exception)
            {
                return null;
            }
        }

        public static bool HasEchoMethod(this object o) => GetEchoMethod(o) != null;
    }
}
