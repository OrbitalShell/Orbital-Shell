using System.Collections.Generic;
using System;
using OrbitalShell.Lib;
using System.Reflection;
using System.Linq;

namespace OrbitalShell.Lib.Sys
{
    /// <summary>
    /// type helper
    /// </summary>
    public class TypeBuilder
    {
        /// <summary>
        /// list of supported type labels
        /// </summary>
        /// <returns>list of labels</returns>
        public static Dictionary<string, Type> GetTypeLabels()
        {
            var r = new List<string>();
            var sysTypes = Assembly.GetAssembly(typeof(Int32)).DefinedTypes;
            var excludeChars = new List<char> { '_', '>', '<' };
            var excludeEnds = new List<string> { "Ex", "Attribute" };

            var dict = new Dictionary<string, Type>();

            void Add(string label, Type type)
            {
                r.Add(label);
                dict.TryAdd(label, type);
            }

            sysTypes = sysTypes.Reverse();  // default to max size numbers
            // primitives types
            foreach (var t in sysTypes)
            {
                if (t.IsPrimitive
#if no
                    /* !t.IsInterface
                    && !t.IsClass
                    && t.IsPublic
                    && t.IsValueType
                    && !t.IsAbstract
                    && !t.IsGenericType
                    && !t.IsNested */
                    // && !t.Name.Contains(excludeChars)
                    // && !t.Name.IsUpperCase()
#endif
                    )
                {
                    var n = t.Name;
                    if (n.ContainsDigit())
                        Add(n.RemoveDigits().ToLower(), t);
                    Add(n.ToLower(), t);
                }
            }

            // custom
            Add("bool", typeof(bool));
            Add("float", typeof(float));
            Add("short", typeof(short));
            Add("long", typeof(long));
            Add("string", typeof(string));
            Add("timespan", typeof(TimeSpan));
            Add("datetime", typeof(DateTime));

#if all_system_types_filtered_activated
            // any not primitive and accessible and related
            // Actually NOT - maybe in the future (keep in place)
            /*foreach (var t in sysTypes)
            {
                if (!t.IsPrimitive
                    && !t.IsInterface
                    && !t.IsClass
                    && t.IsPublic
                    && t.IsValueType
                    && !t.IsAbstract
                    && !t.IsGenericType
                    && !t.IsNested
                    && !t.Name.Contains(excludeChars)
                    && !t.Name.IsUpperCase()
                    )
                {
                    Add(t.Name, t);
                }
            }*/
#endif

            r = r.Distinct().ToList();
            var res = new Dictionary<string, Type>();
            r.Sort();
            foreach (var tn in r) res.Add(tn, dict[tn]);
            return res;
        }

        /// <summary>
        /// get a type from a label, or an actual type full name case sensitive<br/>
        /// supported labels are any System.{type} containing the label wathever case it is
        /// </summary>
        /// <param name="typeLabel">string</param>
        /// <returns>founded type or</returns>
        public static Type GetType(string typeLabel)
        {
            var td = GetTypeLabels();
            if (td.TryGetValue(typeLabel.ToLower(), out var dtype))
                return dtype;
            var t = Type.GetType(typeLabel);
            return t;
        }
    }
}