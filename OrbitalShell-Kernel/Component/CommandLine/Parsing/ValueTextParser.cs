using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using OrbitalShell.Component.CommandLine.CommandModel;
using System.Collections;
using OrbitalShell.Lib;
using System.Runtime;

namespace OrbitalShell.Component.CommandLine.Parsing
{
    public static class ValueTextParser
    {
        /// <summary>
        /// try to convert a text representation to a typed valued according to a type specification
        /// </summary>
        /// <param name="ovalue">object (null,string) to translate to object real type</param>
        /// <param name="ptype">real type expected</param>
        /// <param name="defaultValue">default value used on type value instantiation default value. not used if null</param>
        /// <param name="convertedValue">value converted to real type expected</param>
        /// <param name="possibleValues">in case of fail, message indicating possible values for the expected type</param>
        /// <returns>true if success, false otherwise</returns>
        public static bool ToTypedValue(
            object ovalue,
            Type ptype,
            object defaultValue,
            out object convertedValue,
            out List<object> possibleValues
            )
        {
            convertedValue = null;
            possibleValues = null;
            if (ovalue == null) return false;

            bool result = false;
            bool found = false;
            possibleValues = null;
            var customAttrType = ptype.GetCustomAttribute<CustomParameterType>();
            var interfaces = ptype.GetInterfaces();

            var h = ptype.GetInheritanceChain();

            if (ptype.HasInterface(typeof(ICollection)) && ovalue is string s)
            {
                var genArgs = ptype.GenericTypeArguments;
                if (genArgs.Length > 1) throw new Exception("generic type with more then 1 type argument is not supported: " + ptype.UnmangledName());
                var argType = genArgs[0];
                var lst = Activator.CreateInstance(ptype);
                var met = ptype.GetMethod("Add");
                if (met == null) throw new Exception($"the type {ptype.UnmangledName()} has no method 'Add' that would allow to use it as a collection parameter type");

                var values = s.SplitNotUnslashed(CommandLineSyntax.ParameterTypeListValuesSeparator);
                foreach (var val in values)
                {
                    if (ToTypedValue(val, argType, null, out var convertedVal, out var valPossibleValues))
                    {
                        met.Invoke(lst, new object[] { convertedVal });
                    }
                    else
                    {
                        possibleValues = valPossibleValues;
                        return false;
                    }
                }

                convertedValue = lst;
                return true;

            }
            else if (ptype.IsEnum && ovalue is string str)
            {
                if (ptype.GetCustomAttribute<FlagsAttribute>() != null && str.Contains(CommandLineSyntax.ParameterTypeFlagEnumValuePrefixs))
                {
                    // flag enum Name
                    var fvalues = str.SplitByPrefixsNotUnslashed(CommandLineSyntax.ParameterTypeFlagEnumValuePrefixs);
                    object flag = (defaultValue == null) ? Activator.CreateInstance(ptype) : defaultValue;

                    foreach (var fval in fvalues)
                    {
                        var val = fval.Substring(1);
                        var flagEnabling = fval[0] == CommandLineSyntax.ParameterTypeFlagEnumValuePrefixEnabled;
                        if (ToTypedValue(val, ptype, null, out var convertedVal, out var valPossibleValues))
                        {
                            if (flagEnabling)
                                flag = (int)flag + (int)convertedVal;
                            else
                                flag = (int)flag & ~((int)convertedVal);
                        }
                        else
                        {
                            possibleValues = valPossibleValues;
                            return false;
                        }
                    }

                    convertedValue = flag;
                    return true;
                }
                else
                {
                    // support for any Enum type (expr = single val)
                    // support for flag Enum type and expr = val1,...,valn (result is: Enum val1 | .. | valn) - test with: val.HasFlag(flagval)
                    // support for Enum type and expr = val1,...,valn (result is: int val1 + .. + valn) - test with: val.HasFlag(flagval)
                    if (Enum.TryParse(ptype, str, false, out convertedValue))
                    {
                        return true;
                    }
                    else
                    {
                        possibleValues = Enum.GetNames(ptype).ToList<object>();
                        return false;
                    }
                }
            }
            else if (customAttrType != null)
            {
                try
                {
                    convertedValue = Activator.CreateInstance(ptype, new object[] { ovalue });
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else
            {
                if (ovalue is string value)
                {
                    if (ptype == typeof(int))
                    {
                        result = int.TryParse(value, out var intv);
                        convertedValue = intv;
                        found = true;
                    }
                    if (ptype == typeof(Int16))
                    {
                        result = Int16.TryParse(value, out var intv);
                        convertedValue = intv;
                        found = true;
                    }
                    if (ptype == typeof(Int32))
                    {
                        result = Int32.TryParse(value, out var intv);
                        convertedValue = intv;
                        found = true;
                    }
                    if (ptype == typeof(Int64))
                    {
                        result = Int64.TryParse(value, out var intv);
                        convertedValue = intv;
                        found = true;
                    }
                    if (ptype == typeof(UInt16))
                    {
                        result = UInt16.TryParse(value, out var intv);
                        convertedValue = intv;
                        found = true;
                    }
                    if (ptype == typeof(UInt32))
                    {
                        result = UInt32.TryParse(value, out var intv);
                        convertedValue = intv;
                        found = true;
                    }
                    if (ptype == typeof(UInt64))
                    {
                        result = UInt64.TryParse(value, out var intv);
                        convertedValue = intv;
                        found = true;
                    }
                    if (ptype == typeof(short))
                    {
                        result = short.TryParse(value, out var intv);
                        convertedValue = intv;
                        found = true;
                    }
                    if (ptype == typeof(long))
                    {
                        result = long.TryParse(value, out var intv);
                        convertedValue = intv;
                        found = true;
                    }
                    if (ptype == typeof(double))
                    {
                        result = double.TryParse(value, out var intv);
                        convertedValue = intv;
                        found = true;
                    }
                    if (ptype == typeof(float))
                    {
                        result = float.TryParse(value, out var intv);
                        convertedValue = intv;
                        found = true;
                    }
                    if (ptype == typeof(decimal))
                    {
                        result = decimal.TryParse(value, out var intv);
                        convertedValue = intv;
                        found = true;
                    }
                    if (ptype == typeof(string))
                    {
                        result = true;
                        convertedValue = value;
                        found = true;
                    }
                    if (ptype == typeof(bool))
                    {
                        result = bool.TryParse(value, out var intv);
                        convertedValue = intv;
                        found = true;
                    }
                    if (ptype == typeof(sbyte))
                    {
                        result = sbyte.TryParse(value, out var intv);
                        convertedValue = intv;
                        found = true;
                    }
                    if (ptype == typeof(byte))
                    {
                        result = byte.TryParse(value, out var intv);
                        convertedValue = intv;
                        found = true;
                    }
                    if (ptype == typeof(char))
                    {
                        result = char.TryParse(value, out var intv);
                        convertedValue = intv;
                        found = true;
                    }
                    if (ptype == typeof(Single))
                    {
                        result = Single.TryParse(value, out var intv);
                        convertedValue = intv;
                        found = true;
                    }
                    if (ptype == typeof(DateTime))
                    {
                        result = DateTime.TryParse(value, out var intv);
                        convertedValue = intv;
                        found = true;
                    }
                }

                // unknown type, not CustomParameter: converted value = original value
                if (!found)
                {
                    result = true;
                    convertedValue = ovalue;
                }
            }

            return result;
        }
    }
}