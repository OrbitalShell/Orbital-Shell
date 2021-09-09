using System;
using System.Linq;

using OrbitalShell.Component.CommandLine.Parsing;

using static OrbitalShell.Component.CommandLine.Parsing.CommandLineSyntax;

namespace OrbitalShell.Component.Shell.Variable
{
    public static class VariableSyntax
    {
        public static int FindEndOfVariableName(char[] text, int beginPos)
        {
            int i = beginPos;
            while (i < text.Length)
            {
                char? previousChar = (i > 0) ? text[i - 1] : null;
                if (!IsVariableNameValidCharacter(
                    text[i],
                    previousChar
                    ))
                {
                    break;
                }
                i++;
            }
            return i - 1;
        }

        /// <summary>
        /// predicates that indicates if a character is valid in a variable name, eventually according to the previous character
        /// <para>// ⚡️⚡️⚡️ TODO: disallow in name declaration ⚡️⚡️⚡️</para>
        /// </summary>
        /// <param name="c">character to check</param>
        /// <param name="previousChar">eventually the character that is immediately before the character to be checked</param>
        /// <returns></returns>
        public static bool IsVariableNameValidCharacter(char c, char? previousChar = null)
        {
            return
                !(
                // exclude non printable caracters & flow control caracters
                c <= 31
                // excluded in names
                || ExcludeFromVariableName.Contains(c)
                // exclude variable delimiter
                || (c == VariablePrefix && (!previousChar.HasValue || previousChar.Value != NeutralizerSymbol))
                // exclude top level separators
                || TopLevelSeparators.Contains(c)
                // exclude common operators
                || CommonOperators.Contains(c)
                );
        }

        public static string[] SplitPath(string path)
        {
            return path?.Split(VariableNamePathSeparator);
        }

        public static string GetVariableName(string path)
        {
            if (path == null) return null;
            var p = SplitPath(path);
            if (p.Length == 0) return null;
            return p.Last();
        }

        public static string GetVariableName(ArraySegment<string> path)
        {
            if (path == null) return null;
            if (path.Count == 0) return null;
            return path.Last();
        }

        public static bool HasValidRootNamespace(string name)
        {
            var ns = Enum.GetNames(typeof(VariableNamespace)).Select(x => x + CommandLineSyntax.VariableNamePathSeparator);
            foreach (var n in ns)
                if (name.StartsWith(n)) return true;
            return false;
        }
    }
}
