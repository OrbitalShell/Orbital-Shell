using DotNetConsoleAppToolkit.Component.CommandLine.CommandModel;
using DotNetConsoleAppToolkit.Console;
using System;
using System.Data;
using System.Linq;
using System.Reflection;

namespace DotNetConsoleAppToolkit.Component.CommandLine.Parsing
{
    public class ParameterSyntax
    {
        public static string OptionPrefix = "-";

        public readonly CommandSpecification CommandSpecification;
        public readonly CommandParameterSpecification CommandParameterSpecification;

        public ParameterSyntax(
            CommandSpecification commandSpecification,
            CommandParameterSpecification commandParameterSpecification
            )
        {
            CommandSpecification = commandSpecification;
            CommandParameterSpecification = commandParameterSpecification;
        }

        public (ParseError parseError, ParameterSyntax parameterSyntax) MatchSegment(
            StringComparison syntaxMatchingRule,
            MatchingParameters matchingParameters,
            StringSegment segment, 
            int position, 
            int index,
            StringSegment[] rightSegments,
            StringSegment[] segments)
        {
            var cs = CommandSpecification;
            var csp = CommandParameterSpecification;
            if (matchingParameters.Contains(csp.ParameterName))
            {
                if (position>0)
                {
                    var leftSegments = segments[0..(position - 1)];
                    var idx = leftSegments.ToList().IndexOf(segment);
                    if (idx>-1)
                        return (new ParseError($"parameter at position {position} is already defined at position {idx}: {csp}", position, index, CommandSpecification), this);
                }
                return (new ParseError(null, position, index, CommandSpecification), this);
            }

            if (csp.OptionName != null)
            {
                var optsyntax = $"{ParameterSyntax.OptionPrefix}{csp.OptionName}";
                // option
                if (optsyntax.Equals(segment.Text, syntaxMatchingRule))
                {
                    if ((csp.SegmentsCount == 2 || (csp.SegmentsCount==1 && csp.RequiredParameterName!=null && !cs.ParametersSpecifications[csp.RequiredParameterName].IsOptional) ) && rightSegments.Length == 0)
                        return (new ParseError($"missing value at position {position+1} for parameter {optsyntax}", position+1, index, CommandSpecification), this);
                    else
                        return (null, this);
                }
                else
                    return (new ParseError($"parameter mismatch. attempted: {optsyntax} at position {position}, found: '{segment}'", position, index, CommandSpecification), this);
            }
            else
            {
                var psyntax = $"{csp.ParameterInfo.ParameterType.Name}";
                // fixed parameter
                if (csp.Index == position)
                {
                    if (csp.SegmentsCount==2 && rightSegments.Length==0)
                        return (new ParseError($"missing value at position {position + 1} for parameter {psyntax}", position + 1, index, CommandSpecification), this);
                    else
                        return (null, this);
                }
                else
                {
                    var found = csp.Index < segments.Length ? $", found: '{segments[csp.Index]}'" : "";
                    return (new ParseError($"missing parameter. attempted: {csp.ParameterName} of type {psyntax} at position {csp.Index}{found}", csp.Index, index, CommandSpecification), this);
                }
            }
            throw new ConstraintException();
        }

        public IMatchingParameter BuildMatchingParameter(object value)
        {
            var mparam = BuildMatchingParameter();
            mparam.SetValue(value);
            return mparam;
        }

        public bool TryGetValue(object ovalue,out object convertedValue)
        {
            var comspec = CommandParameterSpecification;
            var ptype = comspec.ParameterInfo.ParameterType;
            convertedValue = null;
            bool result = false;
            bool found = false;

            var customAttrType = ptype.GetCustomAttribute<CustomParameterType>();
            if (customAttrType != null)
            {
                try
                {
                    convertedValue = Activator.CreateInstance(ptype, new object[] { ovalue });
                    return true;
                } catch (Exception)
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

                // unknown type, not CustomParameter
                if (!found)
                {
                    result = true;
                    convertedValue = ovalue;
                }
            }

            return result;
        }

        public IMatchingParameter BuildMatchingParameter()
        {
            IMatchingParameter mparam = null;
            var comspec = CommandParameterSpecification;
            var ptype = comspec.ParameterInfo.ParameterType;

            var customAttrType = ptype.GetCustomAttribute<CustomParameterType>();
            if (customAttrType != null)
            {
                mparam = new MatchingParameter<object>(comspec,true);
            }
            else
            {
                if (ptype == typeof(int))
                    mparam = new MatchingParameter<int>(comspec);
                if (ptype == typeof(Int16))
                    mparam = new MatchingParameter<Int16>(comspec);
                if (ptype == typeof(UInt16))
                    mparam = new MatchingParameter<UInt16>(comspec);
                if (ptype == typeof(Int32))
                    mparam = new MatchingParameter<Int32>(comspec);
                if (ptype == typeof(UInt32))
                    mparam = new MatchingParameter<UInt32>(comspec);
                if (ptype == typeof(Int64))
                    mparam = new MatchingParameter<Int64>(comspec);
                if (ptype == typeof(UInt64))
                    mparam = new MatchingParameter<UInt64>(comspec);
                if (ptype == typeof(short))
                    mparam = new MatchingParameter<short>(comspec);
                if (ptype == typeof(long))
                    mparam = new MatchingParameter<long>(comspec);
                if (ptype == typeof(double))
                    mparam = new MatchingParameter<double>(comspec);
                if (ptype == typeof(float))
                    mparam = new MatchingParameter<float>(comspec);
                if (ptype == typeof(decimal))
                    mparam = new MatchingParameter<decimal>(comspec);
                if (ptype == typeof(string))
                    mparam = new MatchingParameter<string>(comspec);
                if (ptype == typeof(bool))
                    mparam = new MatchingParameter<bool>(comspec);
                if (ptype == typeof(sbyte))
                    mparam = new MatchingParameter<sbyte>(comspec);
                if (ptype == typeof(byte))
                    mparam = new MatchingParameter<byte>(comspec);
                if (ptype == typeof(char))
                    mparam = new MatchingParameter<char>(comspec);
                if (ptype == typeof(Single))
                    mparam = new MatchingParameter<Single>(comspec);
                if (ptype == typeof(DateTime))
                    mparam = new MatchingParameter<DateTime>(comspec);

                if (mparam==null)
                    // unknown type, not CustomParameter
                    mparam = new MatchingParameter<object>(comspec);
            }

            if (mparam==null)
                throw new NotSupportedException($"command parameter type not supported: {ptype.FullName} in command specification: {CommandParameterSpecification}");

            return mparam;
        }

        public override string ToString()
        {
            return CommandParameterSpecification.ToString();
        }

    }
}
