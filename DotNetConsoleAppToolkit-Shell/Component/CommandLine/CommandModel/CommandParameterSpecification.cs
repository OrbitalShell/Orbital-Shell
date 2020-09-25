//#define printDefaultValueInSyntax

using DotNetConsoleAppToolkit.Component.CommandLine.Parsing;
using DotNetConsoleAppToolkit.Console;
using System.Reflection;
using static DotNetConsoleAppToolkit.DotNetConsole;
#if printDefaultValueInSyntax
using static DotNetConsoleAppToolkit.Lib.Str;
#endif

namespace DotNetConsoleAppToolkit.Component.CommandLine.CommandModel
{
    public class CommandParameterSpecification
    {
        public readonly string ParameterName;
        public readonly ParameterInfo ParameterInfo;
        public readonly bool IsOptional = false;
        public readonly int Index = -1;
        public readonly string Description;
        public readonly string OptionName = null;
        public readonly object DefaultValue = null;
        public readonly bool HasDefaultValue = false;
        public readonly bool HasValue = true;

        public readonly string RequiredParameterName = null;

        public string ActualName => OptionName ?? ParameterName;
        public bool IsOption => OptionName != null;

        public CommandParameterSpecification(
            string parameterName,
            string description, 
            bool isOptional, 
            int index, 
            string optionName,
            bool hasValue,
            bool hasDefaultValue,
            object defaultValue,
            ParameterInfo parameterInfo,
            string requiredParameterName=null)
        {
            RequiredParameterName = requiredParameterName;
            ParameterName = parameterName;
            ParameterInfo = parameterInfo;
            Description = description;
            IsOptional = isOptional;
            Index = index;
            OptionName = optionName;
            HasValue = hasValue;
            HasDefaultValue = hasDefaultValue;
            DefaultValue = defaultValue;

            if (HasValue && requiredParameterName != null)
                throw new AmbiguousParameterSpecificationException($"parameter '{ParameterName}' can't both having a value and requiring a parameter (name '{requiredParameterName}')"); 
        }

        public string ParameterValueTypeName => ParameterInfo.ParameterType.Name;

        public int SegmentsCount => IsOption && HasValue ? 2 : 1;

        public override string ToString() => Dump();

        public string Dump(bool grammarSymbolsVisible = true)
        {
            var r = $"{ParameterName}";
            if (IsOption)
            {
                var optVal = (HasValue) ? $" {ParameterValueTypeName}" : "";
                r = $"{ParameterSyntax.OptionPrefix}{OptionName}{optVal}";
            }
            if (IsOptional && grammarSymbolsVisible) r = $"[{r}]";
#if printDefaultValueInSyntax
            if (HasDefaultValue && grammarSymbolsVisible) r += $"{{={($"{DumpAsText(DefaultValue)}")}}}";
#endif
            return r;
        }

        public string ToColorizedString(bool grammarSymbolsVisible=true)
        {
            var f = GetCmd(EchoDirectives.f + "", DefaultForeground.ToString().ToLower());
            var r = $"{ColorSettings.ParameterName}{ParameterName}{f}";
            if (IsOption)
            {
                var optVal = (HasValue) ? $" {ColorSettings.ParameterValueType}{ParameterValueTypeName}" : "";
                r = $"{ColorSettings.OptionPrefix}{ParameterSyntax.OptionPrefix}{ColorSettings.OptionName}{OptionName}{optVal}{f}";
            }
            if (IsOptional && grammarSymbolsVisible) r = $"{ColorSettings.SyntaxSymbol}[{r}{ColorSettings.SyntaxSymbol}]{f}";
#if printDefaultValueInSyntax
            if (HasDefaultValue && grammarSymbolsVisible) r += $"{Cyan}{{={($"{Darkyellow}{DumpAsText(DefaultValue)}{Cyan}}}{f}")}";
#endif
            return r;
        }
    }
}
