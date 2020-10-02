using DotNetConsoleAppToolkit.Component.CommandLine.Processor;
using DotNetConsoleAppToolkit.Console;
using System;
using System.Linq;
using System.Reflection;

namespace DotNetConsoleAppToolkit.Lib
{
    public static partial class TypesExt
    {
        public static void InvokeEcho(
            this MethodInfo echoMethodInfo,
            object obj,
            ConsoleTextWriterWrapper @out,
            CommandEvaluationContext context,
            FormattingOptions options = null
            )
        {
            @out.Echo(context.ShellEnv.Colors.Default);           

            if (echoMethodInfo.GetParameters().Length == 3)
                echoMethodInfo.Invoke(obj, new object[] { @out, context,
                    (options != null) ? options : GetParameterDefaultValue(echoMethodInfo, 2)
                });
            else
                echoMethodInfo.Invoke(obj, new object[] { obj, @out, context, 
                    (options != null) ? options : GetParameterDefaultValue(echoMethodInfo, 3)
                });
        }

        public static string InvokeAsText()
        {
            return null;
        }

        static object GetParameterDefaultValue(MethodInfo mi,int pindex) 
            => Activator.CreateInstance(mi.GetParameters()[pindex].ParameterType);

        public static MethodInfo GetAsTextMethod(this object o)
        {
            if (o == null) throw new ArgumentNullException(nameof(o));
            var t = o.GetType();
            try
            {
                var mi = t.GetMethod(
                    "AsText",
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static
                    );

                // extension of a type that is not in this assembly will not be found by GetMethod
                // search in extension class to fix it
                if (mi == null)
                {
                    var mis = typeof(EchoPrimitives).GetMethods();
                    mi = mis.Where(x => x.Name == "AsText" && x.GetParameters()[0].ParameterType == o.GetType()).FirstOrDefault();
                }
                return mi;
            }
            catch (Exception)
            {
                return null;
            }
        }

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
                
                // extension of a type not in this assembly is not found
                // search in extension class to fix it
                if (mi==null)
                {
                    var mis = typeof(EchoPrimitives).GetMethods();
                    mi = mis.Where(x => x.Name == "Echo" && x.GetParameters()[0].ParameterType == o.GetType()).FirstOrDefault();
                }

                return mi;

            } catch (Exception)
            {
                return null;
            }
        }

        public static bool HasEchoMethod(this object o) => GetEchoMethod(o) != null;

        public static bool HasAsTextMethod(this object o) => GetAsTextMethod(o) != null;
    }
}
