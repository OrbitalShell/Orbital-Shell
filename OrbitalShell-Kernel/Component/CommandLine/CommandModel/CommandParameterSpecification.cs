//#define printDefaultValueInSyntax

using OrbitalShell.Component.CommandLine.Parsing.Sentence;
using OrbitalShell.Component.Console;

using System.Reflection;

#if printDefaultValueInSyntax
using static OrbitalShell.Lib.Str;
#endif
using static OrbitalShell.Component.EchoDirective.Shortcuts;

using OrbitalShell.Component.EchoDirective;
using OrbitalShell.Lib;

using Microsoft.Extensions.DependencyInjection;

namespace OrbitalShell.Component.CommandLine.CommandModel
{
    public class CommandParameterSpecification
    {
        public readonly string ParameterName;
        public readonly ParameterInfo ParameterInfo;
        public readonly bool IsOptional = false;
        public readonly int Index = -1;
        public readonly string Description;
        public readonly string OptionName = null;
        public readonly string OptionLongName = null;
        public readonly object DefaultValue = null;
        public readonly bool HasDefaultValue = false;
        public readonly bool HasValue = true;

        public readonly string RequiredParameterName = null;

        public string ActualName => OptionName ?? OptionLongName ?? ParameterName;
        public bool IsOption => OptionName != null || OptionLongName != null;

        public CommandParameterSpecification(
            string parameterName,
            string description,
            bool isOptional,
            int index,
            string optionName,
            string optionLongName,
            bool hasValue,
            bool hasDefaultValue,
            object defaultValue,
            ParameterInfo parameterInfo,
            string requiredParameterName = null)
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
            OptionLongName = optionLongName;

            if (HasValue && requiredParameterName != null)
                throw new AmbiguousParameterSpecificationException($"parameter specification error: the parameter '{ParameterName}' can't have both a required parameter ('{requiredParameterName}')");
        }

        public string ParameterValueTypeName => ParameterInfo.ParameterType.Name;

        public int SegmentsCount => IsOption && HasValue ? 2 : 1;

        public override string ToString() => Dump();

        public string Dump(bool grammarSymbolsVisible = true)
        {
            var r = $"{ParameterName}";
            if (IsOption)
            {
                var optVal = (HasValue) ? $" {ParameterInfo.ParameterType.UnmangledName()}" : "";
                string sepcar = grammarSymbolsVisible ? "|" : ", ";
                string longopt = (OptionName != null && OptionLongName != null ? $"{sepcar}" : "")
                                + (OptionLongName != null ? $"{CommandLineSyntax.OptionLongPrefix}{OptionLongName}" : "");
                r = "";
                if (OptionName != null)
                    r += $"{CommandLineSyntax.OptionPrefix}{OptionName}";
                r += $"{longopt}{optVal}";
            }
            if (IsOptional && grammarSymbolsVisible) r = $"[{r}]";
#if printDefaultValueInSyntax
            if (HasDefaultValue && grammarSymbolsVisible) r += $"{{={($"{DumpAsText(DefaultValue)}")}}}";
#endif
            return r;
        }

        public string ToColorizedString(ColorSettings colors, bool grammarSymbolsVisible = true)
        {
            var console = App.Host.Services.GetRequiredService<IConsole>();

            var f = GetCmd(EchoDirectives.f + "", console.DefaultForeground.ToString().ToLower());
            var r = $"{colors.ParameterName}{ParameterName}{f}";
            if (IsOption)
            {
                var optVal = (HasValue) ? $" {colors.ParameterValueType}{ParameterInfo.ParameterType.UnmangledName()}" : "";
                string sepcar = grammarSymbolsVisible ? $"{colors.SyntaxSymbol}|" : ", ";
                //string longopt = OptionLongName != null ? $"{sepcar}{colors.OptionPrefix}{CommandLineSyntax.OptionLongPrefix}{colors.OptionName}{OptionLongName}" : "";

                string longopt = (OptionName != null && OptionLongName != null ? $"{sepcar}" : "")
                                + (OptionLongName != null ? $"{colors.OptionPrefix}{CommandLineSyntax.OptionLongPrefix}{colors.OptionName}{OptionLongName}" : "");
                r = "";
                if (OptionName != null)
                    r += $"{colors.OptionPrefix}{CommandLineSyntax.OptionPrefix}{colors.OptionName}{OptionName}";
                r += $"{longopt}{optVal}{f}";
            }
            if (IsOptional && grammarSymbolsVisible) r = $"{colors.SyntaxSymbol}[{r}{colors.SyntaxSymbol}]{f}";
#if printDefaultValueInSyntax
            if (HasDefaultValue && grammarSymbolsVisible) r += $"{Cyan}{{={($"{colors.Value}{DumpAsText(DefaultValue)}{Cyan}}}{f}")}";
#endif
            return r;
        }
    }
}
