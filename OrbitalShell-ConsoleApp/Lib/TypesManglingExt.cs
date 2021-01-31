using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace OrbitalShell.Lib
{
    /// <summary>
    /// types names mangling extensions<br/>
    /// from: https://www.codeproject.com/Tips/621812/User-friendly-names-for-Types<br/>
    /// author: Dennis E https://www.codeproject.com/Members/User-5049496<br/>
    /// under Licence CPOL <br/>   
    /// released 16/7/2013<br/>
    /// </summary>
    public static class TypesManglingExt
    {
        private static readonly Dictionary<Type, string> keywordTypes = new Dictionary<Type, string>() {
            { typeof(void), "void" },
            { typeof(bool), "bool" },
            { typeof(char), "char" },
            { typeof(byte), "byte" },
            { typeof(sbyte), "sbyte" },
            { typeof(short), "short" },
            { typeof(ushort), "ushort" },
            { typeof(int), "int" },
            { typeof(uint), "uint" },
            { typeof(long), "long" },
            { typeof(ulong), "ulong" },
            { typeof(float), "float" },
            { typeof(double), "double" },
            { typeof(decimal), "decimal" },
            { typeof(string), "string" },
            { typeof(object), "object" }
        };

        public static string GetKeyword(this Type type)
        {
            if (type == null) return null;
            string result;
            keywordTypes.TryGetValue(type, out result);
            return result;
        }

        // TODO: implements parameter short or fullName
        public static string FriendlyName(this Type type, bool useKeywords = true, bool showGenericArguments = true, bool showDeclaringType = true, bool compactNullable = true)
        {
            if (type == null) return string.Empty;
            StringBuilder b = new StringBuilder();
            BuildFriendlyName(b, type, useKeywords, showGenericArguments, showDeclaringType, compactNullable);
            return b.ToString();
        }

        private static void BuildFriendlyName(StringBuilder builder, Type type, bool useKeywords, bool showGenericArguments, bool showDeclaringType, bool compactNullable)
        {
            bool isBasic = true;
            if (showDeclaringType && type.IsNested && !type.IsGenericParameter)
            {
                BuildFriendlyName(builder, type.DeclaringType, useKeywords, showGenericArguments, showDeclaringType, compactNullable);
                builder.Append('.');
            }
            if (type.IsArray)
            {
                isBasic = false;
                BuildFriendlyName(builder, type.GetElementType(), useKeywords, showGenericArguments, showDeclaringType, compactNullable);
                builder.Append('[');
                for (int rank = type.GetArrayRank(); rank > 1; --rank) builder.Append(',');
                builder.Append(']');
            }
            if (type.IsPointer)
            {
                isBasic = false;
                BuildFriendlyName(builder, type.GetElementType(), useKeywords, showGenericArguments, showDeclaringType, compactNullable);
                builder.Append('*');
            }
            if (type.IsGenericParameter)
            {
                isBasic = false;
                GenericParameterAttributes gpAttributes = type.GenericParameterAttributes;
                if ((gpAttributes & GenericParameterAttributes.Covariant) == GenericParameterAttributes.Covariant)
                {
                    builder.Append("out ");
                }
                else if ((gpAttributes & GenericParameterAttributes.Contravariant) == GenericParameterAttributes.Contravariant)
                {
                    builder.Append("in ");
                }
                builder.Append(type.Name);
            }
            if (type.IsGenericType)
            {
                isBasic = false;
                string name = type.Name;
                int index = name.IndexOf('`');
                if (index > 0) name = name.Substring(0, name.IndexOf('`'));
                if (type.IsGenericTypeDefinition)
                {
                    if (compactNullable && type == typeof(Nullable<>))
                    {
                        builder.Append(showGenericArguments ? "T?" : "Nullable<>");
                    }
                    else
                    {
                        builder.Append(name).Append('<');
                        Type[] args = type.GetGenericArguments();
                        if (showGenericArguments)
                        {
                            for (int i = 0; i < args.Length; ++i)
                            {
                                if (i > 0) builder.Append(", ");
                                BuildFriendlyName(builder, args[i], useKeywords, showGenericArguments, showDeclaringType, compactNullable);
                            }
                        }
                        else
                        {
                            for (int i = 0; i < args.Length - 1; ++i) builder.Append(',');
                        }
                        builder.Append('>');
                    }
                }
                else
                {
                    if (compactNullable && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        BuildFriendlyName(builder, Nullable.GetUnderlyingType(type), useKeywords, showGenericArguments, showDeclaringType, compactNullable);
                        builder.Append('?');
                    }
                    else
                    {
                        builder.Append(name).Append('<');
                        Type[] args = type.GetGenericArguments();
                        for (int i = 0; i < args.Length; ++i)
                        {
                            if (i > 0) builder.Append(", ");
                            BuildFriendlyName(builder, args[i], useKeywords, showGenericArguments, showDeclaringType, compactNullable);
                        }
                        builder.Append('>');
                    }
                }
            }
            if (isBasic)
            {
                string keyword = useKeywords ? GetKeyword(type) : null;
                builder.Append(keyword ?? type.Name);
            }
        }

    }
}