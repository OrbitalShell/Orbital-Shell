using System;

namespace OrbitalShell.Component.CommandLine.Parsing
{
    public static class ValueTextParser
    {
        public static object ToTypedValue(object ovalue,Type toType)
        {
            if (ovalue==null) return null;
            bool result = false;
            object convertedValue = ovalue;
            if (ovalue is string value)
            {
                if (toType == typeof(int))
                {
                    result |= int.TryParse(value, out var intv);
                    convertedValue = intv;
                }
                if (toType == typeof(Int16))
                {
                    result |= Int16.TryParse(value, out var intv);
                    convertedValue = intv;
                }
                if (toType == typeof(Int32))
                {
                    result |= Int32.TryParse(value, out var intv);
                    convertedValue = intv;
                }
                if (toType == typeof(Int64))
                {
                    result |= Int64.TryParse(value, out var intv);
                    convertedValue = intv;
                }
                if (toType == typeof(UInt16))
                {
                    result |= UInt16.TryParse(value, out var intv);
                    convertedValue = intv;
                }
                if (toType == typeof(UInt32))
                {
                    result |= UInt32.TryParse(value, out var intv);
                    convertedValue = intv;
                }
                if (toType == typeof(UInt64))
                {
                    result |= UInt64.TryParse(value, out var intv);
                    convertedValue = intv;
                }
                if (toType == typeof(short))
                {
                    result |= short.TryParse(value, out var intv);
                    convertedValue = intv;
                }
                if (toType == typeof(long))
                {
                    result |= long.TryParse(value, out var intv);
                    convertedValue = intv;
                }
                if (toType == typeof(double))
                {
                    result |= double.TryParse(value, out var intv);
                    convertedValue = intv;
                }
                if (toType == typeof(float))
                {
                    result |= float.TryParse(value, out var intv);
                    convertedValue = intv;
                }
                if (toType == typeof(decimal))
                {
                    result |= decimal.TryParse(value, out var intv);
                    convertedValue = intv;
                }
                if (toType == typeof(string))
                {
                    result |= true;
                    convertedValue = value;
                }
                if (toType == typeof(bool))
                {
                    result |= bool.TryParse(value, out var intv);
                    convertedValue = intv;
                }
                if (toType == typeof(sbyte))
                {
                    result |= sbyte.TryParse(value, out var intv);
                    convertedValue = intv;
                }
                if (toType == typeof(byte))
                {
                    result |= byte.TryParse(value, out var intv);
                    convertedValue = intv;
                }
                if (toType == typeof(char))
                {
                    result |= char.TryParse(value, out var intv);
                    convertedValue = intv;
                }
                if (toType == typeof(Single))
                {
                    result |= Single.TryParse(value, out var intv);
                    convertedValue = intv;
                }
                if (toType == typeof(DateTime))
                {
                    result |= DateTime.TryParse(value, out var intv);
                    convertedValue = intv;
                };
                if (!result) throw new Exception($"type not supported: {toType.FullName}");
            }
            return convertedValue;
        }
    }
}