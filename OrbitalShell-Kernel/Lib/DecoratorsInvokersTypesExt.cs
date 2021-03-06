﻿using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Console;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace OrbitalShell.Lib
{
    /// <summary>
    /// extensions methods for: output filtering
    /// </summary>
    public static partial class DecoratorsInvokersTypesExt
    {
        public static void InvokeEcho(
            this MethodInfo echoMethodInfo,
            object obj,
            EchoEvaluationContext ctx
            )
        {
            if (echoMethodInfo.GetParameters().Length == 1)
            {
                echoMethodInfo.Invoke(obj, new object[] { ctx });
            }
            else
            {
                echoMethodInfo.Invoke(obj, new object[] { obj, ctx });
            }
        }

        public static string InvokeAsText(this MethodInfo asTextMethodInfo, object obj, CommandEvaluationContext context)
        {
            if (asTextMethodInfo.GetParameters().Length == 1)
                return (string)asTextMethodInfo.Invoke(obj, new object[] { context });
            else
                return (string)asTextMethodInfo.Invoke(obj, new object[] { obj, context });
        }

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

        /// <summary>
        /// search an 'Echo' method for the given object
        /// </summary>
        /// <param name="o">object</param>
        /// <returns>method info or null</returns>
        public static MethodInfo GetEchoMethod(this object o)
        {
            if (o == null) throw new ArgumentNullException(nameof(o));
            var t = o.GetType();

            try
            {
                var inheritanceChain = t.GetInheritanceChain();

                MethodInfo mi = null;
                foreach (var it in inheritanceChain)
                {
                    mi = it.GetMethod(
                        "Echo",
                        BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static
                        );
                    if (mi != null) break;
                }

                // extension of a type not in this assembly is not found
                // search in extension class to fix it

                MethodInfo[] mis = null;

                if (mi == null)
                {
                    mis = typeof(EchoPrimitives).GetMethods();
                    foreach (var it in inheritanceChain)
                    {
                        mi = mis.Where(x => x.Name == "Echo" && x.GetParameters()[0].ParameterType == it).FirstOrDefault();
                        if (mi != null) break;
                    }
                }

                // search extension method with interfaces types

                if (mi == null) {
                    foreach (var it in t.GetInterfaces())
                    {
                        mi = mis.Where(x => x.Name == "Echo" && x.GetParameters()[0].ParameterType == it).FirstOrDefault();
                        if (mi != null) break;
                    }
                }

                return mi;

            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool HasEchoMethod(this object o) => GetEchoMethod(o) != null;

        public static bool HasAsTextMethod(this object o) => GetAsTextMethod(o) != null;
    }
}
