using OrbitalShell.Component.CommandLine.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrbitalShell.Component.CommandLine.Parsing
{
    internal class CommandLineSyntax
    {
        // -- top level parser

        public static char SingleQuote = '\'';
        public static char DoubleQuote = '"';
        public static char NeutralizerSymbol = '\\';
        public static char SpaceSeparator = ' ';
        public static char BatchCommentBegin = '#';

        public static char VariablePrefix = '$';
        public static char VariableNamePathSeparator = '.';

        public static string Pipeline = "|";
        public static string ConditionalPipelineSuccess = "&&";
        public static string ConditionalPipelineFail = "||";

        public static string RedirectInput = "<";
        public static string RedirectOutput = ">";
        public static string RedirectUnion = "&>";
        public static string HereScript = "<<";

        public static string StdErr = "2" + RedirectOutput;
        public static List<string> StdOut = new List<string> { "1" + RedirectOutput, RedirectOutput };
        public static List<string> StdIn = new List<string> { "0" + RedirectInput, RedirectInput };

        public static char CommandGroupSeparator = ';';
        public static char CommandGroupBegin = '(';
        public static char CommandGroupEnd = ')';

        public static char CommandNamespaceSeparator = '-';

        /// <summary>
        /// prefix of a command option (short name)
        /// </summary>
        public static string OptionPrefix = "-";

        /// <summary>
        /// prefix of a command option (long name)
        /// </summary>
        public static string OptionLongPrefix = "--";

        /// <summary>
        /// values separarator in parameter of type list/collection
        /// </summary>
        public static char ParameterTypeListValuesSeparator = ',';

        /// <summary>
        /// add a flag value (value prefix) in parameter of type enum flag
        /// </summary>
        public static char ParameterTypeFlagEnumValuePrefixEnabled = '+';

        /// <summary>
        /// remove a flag value (value prefix) in parameter of type enum flag
        /// </summary>
        public static char ParameterTypeFlagEnumValuePrefixDisabled = '-';

        /// <summary>
        /// any prefix for flag value (value prefix) in parameter of type enum flag
        /// </summary>
        public static List<char> ParameterTypeFlagEnumValuePrefixs = new List<char> { ParameterTypeFlagEnumValuePrefixEnabled, ParameterTypeFlagEnumValuePrefixDisabled };

        public static bool IsNotAnOperator(string s) =>
            !IsRedirectOutput(s) &&
            !IsRedirectInput(s) &&
            !IsRedirectError(s) &&
            !IsPipeOperator(s);

        public static bool IsRedirectOutput(string s) =>
            StdOut.Contains(s);

        public static bool IsRedirectInput(string s) =>
            StdIn.Contains(s) || IsHereScript(s);

        public static bool IsHereScript(string s) => HereScript == s;

        public static bool IsRedirectionUnion(string s) => false;

        public static bool IsRedirectError(string s) => false;

        public static bool IsPipeOperator(string s) => s == Pipeline || s == ConditionalPipelineFail || s == ConditionalPipelineSuccess;

        public static string PipelineConditionToStr(PipelineCondition pipelineCondition)
        {
            switch (pipelineCondition)
            {
                case PipelineCondition.NotAppliable: return "";
                case PipelineCondition.Always: return Pipeline;
                case PipelineCondition.Success: return ConditionalPipelineSuccess;
                case PipelineCondition.Error: return ConditionalPipelineFail;
                default: return null;
            }
        }

        // -----

        public static char[] TopLevelSeparators = {
            SpaceSeparator,
            DoubleQuote,
            SingleQuote,
            NeutralizerSymbol,
            '&' };


        public static string InputRedirect = "1>";

        public static char[] CommonOperators =
        {
            '-',
            '+',
            '/',
            '*',
            '^',
            ':'
        };

        public static string[] StreamSeparators =
        {
            ">",
            "<"
        };

        public static string[] PipelineSeparators =
        {
            "|"
        };
    }
}
