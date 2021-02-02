using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace OrbitalShell.Lib
{
    /// <summary>
    /// 'types extension methods
    /// </summary>
    public static partial class TypesExt
    {
        #region type Type extensions

        /// <summary>
        /// surface clone
        /// </summary>
        /// <typeparam name="T">object type</typeparam>
        /// <param name="obj">object to clone</param>
        /// <returns>the clone</returns>
        public static T Clone<T>(this T obj) where T : new()
        {
            var cloneObj = new T();
            foreach (var member in typeof(T).GetMembers())
            {
                if (member is FieldInfo field && !field.IsInitOnly)
                    field.SetValue(cloneObj, field.GetValue(obj));
                if (member is PropertyInfo prop && prop.CanWrite)
                    prop.SetValue(cloneObj, prop.GetValue(obj));
            }
            return cloneObj;
        }

        // TODO: implements parameter fullName
        public static string UnmangledName(this Type type, bool fullName = false)
        {
            return TypesManglingExt.FriendlyName(type);
        }

        public static bool InheritsFrom(this Type type, Type ancestorType)
        {
            while (type != null)
            {
                if (type.BaseType == ancestorType) return true;
                type = type.BaseType;
            }
            return false;
        }

        public static List<Type> GetInheritanceChain(this Type type, bool includeRoot = true)
        {
            var r = new List<Type>();
            if (includeRoot) r.Add(type);
            type = type.BaseType;
            // walk inheritance chain
            while (type != null)
            {
                if (type != typeof(object))
                    r.Add(type);
                type = type.BaseType;
            }
            return r;
        }

        public static bool HasInterface(this Type type, Type interfaceType)
            => type.GetInterface(interfaceType.FullName) != null;

        public static List<MemberInfo> GetFieldsAndProperties(this object o)
        {
            var t = o.GetType();
            var r = new List<MemberInfo>();
            foreach (var f in t.GetFields())
            {
                r.Add(f);
            }
            foreach (var p in t.GetProperties())
            {
                r.Add(p);
            }
            return r;
        }

        public static object GetMemberValue(this MemberInfo mi, object obj, bool throwException = true)
        {
            if (mi is FieldInfo f) return f.GetValue(obj);
            if (mi is PropertyInfo p)
            {
                if (p.GetGetMethod().GetParameters().Length == 0)
                    return p.GetValue(obj);
                else
                {
                    if (throwException)
                        // indexed property
                        throw new ArgumentException($"can't get value of indexed property: '{p.Name}'");
                }
            }
            return null;
        }

        public static List<(string name, object value, MemberInfo memberInfo)> GetMemberValues(this object o)
        {
            var t = o.GetType();
            var r = new List<(string, object, MemberInfo)>();
            foreach (var f in t.GetFields())
            {
                //r.Add((f.Name, f.GetValue(o),f));
                r.Add((f.Name, f.GetMemberValue(o), f));
            }
            foreach (var p in t.GetProperties())
            {
                /*if (p.GetGetMethod().GetParameters().Length==0)
                    r.Add((p.Name, p.GetValue(o),p));
                else
                    // indexed property
                    r.Add((p.Name, "indexed property" , p));*/
                try
                {
                    var val = p.GetMemberValue(o, true);
                    r.Add((p.Name, p.GetValue(o), p));
                }
                catch (ArgumentException)
                {
                    r.Add((p.Name, "indexed property", p));
                }
            }
            r.Sort(new Comparison<(string, object, MemberInfo)>(
                (a, b) => a.Item1.CompareTo(b.Item1)));
            return r;
        }

        public static Type GetMemberValueType(this MemberInfo memberInfo)
        {
            if (memberInfo is FieldInfo field) return field.FieldType;
            if (memberInfo is PropertyInfo prop) return prop.PropertyType;
            return null;
        }

        /*public static object GetValue(this MemberInfo memberInfo,object target)
        {
            if (memberInfo is FieldInfo fi)
            {
                return fi.GetValue(target);
            } else
            {
                if (memberInfo is PropertyInfo pi)
                {
                    return pi.GetValue(target);
                }
            }
            return null;
        }*/

        #endregion

        #region collections ext

        public static List<T> Clone<T>(this List<T> o)
        {
            var r = new List<T>();
            r.AddRange(o);
            return r;
        }

        public static bool AddUnique<T>(this List<T> o, T value)
        {
            var r = o.Contains(value);
            if (!r) o.Add(value);
            return r;
        }

        public static void Merge<K, V>(this Dictionary<K, V> dic, Dictionary<K, V> mergeTo)
        {
            foreach (var kv in mergeTo)
                dic.AddOrReplace(kv.Key, kv.Value);
        }

        public static void Add<K, V>(this Dictionary<K, V> dic, Dictionary<K, V> addTo)
        {
            foreach (var kv in addTo)
                dic.Add(kv.Key, kv.Value);
        }

        public static void AddOrReplace<TK, TV>(this Dictionary<TK, TV> dic, TK key, TV value)
        {
            if (dic.ContainsKey(key))
                dic[key] = value;
            else
                dic.Add(key, value);
        }

        public static void AddOrReplace<TK, TV>(this SortedDictionary<TK, TV> dic, TK key, TV value)
        {
            if (dic.ContainsKey(key))
                dic[key] = value;
            else
                dic.Add(key, value);
        }

        public static void Merge<T>(this List<T> mergeInto, List<T> merged)
        {
            foreach (var o in merged)
                if (!mergeInto.Contains(o))
                    mergeInto.Add(o);
        }

        public static void AddColumns(
            this DataTable table,
            params string[] columnNames)
        {
            foreach (var colName in columnNames)
            {
                table.Columns.Add(colName);
            }
        }

        #endregion

        // echo
        /* OBSOLETE?
                public static void Echo(this string x, ConsoleTextWriterWrapper @out) => @out.Echo(x);
                public static void Echo(this int x, ConsoleTextWriterWrapper @out) => @out.Echo(x);
                public static void Echo(this double x, ConsoleTextWriterWrapper @out) => @out.Echo(x);
                public static void Echo(this float x, ConsoleTextWriterWrapper @out) => @out.Echo(x);
                public static void Echo(this bool x, ConsoleTextWriterWrapper @out) => @out.Echo(x);

                public static void Echoln(this string x, ConsoleTextWriterWrapper @out) => @out.Echoln(x);
                public static void Echoln(this int x, ConsoleTextWriterWrapper @out) => @out.Echoln(x + "");
                public static void Echoln(this double x, ConsoleTextWriterWrapper @out) => @out.Echoln(x + "");
                public static void Echoln(this float x, ConsoleTextWriterWrapper @out) => @out.Echoln(x + "");
                public static void Echoln(this bool x, ConsoleTextWriterWrapper @out) => @out.Echoln(x + "");
        */
    }
}
