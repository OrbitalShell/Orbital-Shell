using OrbitalShell.Component.CommandLine.CommandModel;
using OrbitalShell.Component.Console;
using OrbitalShell.Lib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace OrbitalShell.Component.CommandLine.Parsing
{
    public class ParameterSyntax
    {
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
                if (position > 0)
                {
                    var leftSegments = segments[0..(position - 1)];
                    var idx = leftSegments.ToList().IndexOf(segment);
                    if (idx > -1)
                        return (new ParseError($"parameter at position {position} is already defined at position {idx}: {csp}", position, index, CommandSpecification), this);
                }
                return (new ParseError(null, position, index, CommandSpecification), this);
            }

            // mandatory criteria to be an option 
            if (csp.OptionName != null || csp.OptionLongName != null)
            {
                // option

                var optsyntax = $"{CommandLineSyntax.OptionPrefix}{csp.OptionName}";
                var optlngsyntax = $"{CommandLineSyntax.OptionLongPrefix}{csp.OptionLongName}";

                // option name presentation in error message
                var poptsyntax = ((csp.OptionName != null) ? optsyntax : "")
                + ((csp.OptionName != null && csp.OptionLongName != null) ? " or " : "")
                + (csp.OptionLongName != null ? $"{optlngsyntax}" : "");

                if (optsyntax.Equals(segment.Text, syntaxMatchingRule)
                    || (csp.OptionLongName != null && optlngsyntax.Equals(segment.Text, syntaxMatchingRule))
                )
                {
                    if ((csp.SegmentsCount == 2 || (csp.SegmentsCount == 1 && csp.RequiredParameterName != null
                        && !cs.ParametersSpecifications[csp.RequiredParameterName].IsOptional))
                        && rightSegments.Length == 0)
                    {
                        return (new ParseError($"missing value at position {position + 1} for option {poptsyntax}", position + 1, index, CommandSpecification), this);
                    }
                    else
                        return (null, this);
                }
                else
                {
                    return (new ParseError($"parameter mismatch. attempted: {poptsyntax} at position {position}, found: '{segment.Text}'", position, index, CommandSpecification), this);
                }
            }
            else
            {
                var psyntax = $"{csp.ParameterInfo.ParameterType.UnmangledName()}";
                // fixed parameter
                if (csp.Index == position)
                {
                    if (csp.SegmentsCount == 2 && rightSegments.Length == 0)
                        return (new ParseError($"missing value at position {position + 1} for parameter {psyntax}", position + 1, index, CommandSpecification), this);
                    else
                        return (null, this);
                }
                else
                {
                    var found = csp.Index < segments.Length ? $", found: '{segments[csp.Index].Text}'" : "";
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

        public bool TryGetValue(
            object ovalue,
            out object convertedValue,
            out List<object> possibleValues,
            Type fallBackType = null,
            bool defaultReturnIdentityOk = true,
            bool allowPrecisionLost = true
        ) => ValueTextParser.ToTypedValue(
                ovalue,
                CommandParameterSpecification.ParameterInfo.ParameterType,
                CommandParameterSpecification.ParameterInfo.DefaultValue,
                out convertedValue,
                out possibleValues,
                fallBackType,
                defaultReturnIdentityOk,
                allowPrecisionLost);

        public IMatchingParameter BuildMatchingParameter()
        {
            IMatchingParameter mparam = null;
            var comspec = CommandParameterSpecification;
            var ptype = comspec.ParameterInfo.ParameterType;

            var customAttrType = ptype.GetCustomAttribute<CustomParameterType>();
            if (customAttrType != null)
            {
                mparam = new MatchingParameter<object>(comspec, true);
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

                if (mparam == null)
                    // unknown type, not a CustomParameter
                    mparam = new MatchingParameter<object>(comspec);
            }

            if (mparam == null)
                throw new NotSupportedException($"command parameter type not supported: {ptype.FullName} in command specification: {CommandParameterSpecification}");

            return mparam;
        }

        public override string ToString()
        {
            return CommandParameterSpecification.ToString();
        }

    }
}
