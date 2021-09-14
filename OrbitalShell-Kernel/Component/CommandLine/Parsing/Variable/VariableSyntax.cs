using System;
using System.Linq;

using OrbitalShell.Component.CommandLine.Parsing.Sentence;
using OrbitalShell.Component.Shell.Variable;

using static OrbitalShell.Component.CommandLine.Parsing.Sentence.CommandLineSyntax;

namespace OrbitalShell.Component.CommandLine.Parsing.Variable
{
    public static class VariableSyntax
    {
        public static (
            string varName,
            int beginIndex,
            int lastIndex,
            bool isNameCaptured,
            int remaining,
            bool previousNameDefined)
            ReadVariableName(
                ref char[] text,
                int beginPos,
                int lastPos
            )
        {
            int i = beginPos;
            var isNameCaptured = false;
            var isCapturingName = false;
            var capturesRecursionCount = 0;
            var recursionCount = 0;
            bool nameDefined = false;
            bool previousNameDefined = true;

            while (i < lastPos)
            {
                var c = text[i];
                char? previousChar = (i > 0) ? text[i - 1] : null;
                var isNeutralized = previousChar == NeutralizerSymbol;
                var skipSymbol = false;

                if (c == VariablePrefix
                    && !isNeutralized
                    )
                {
                    if (!isCapturingName && nameDefined)
                    {
                        //i--;
                        break;
                    }
                    else
                    {
                        // variable in variable name
                        // reset start def at this point
                        skipSymbol = true;
                        isCapturingName = false;
                        isNameCaptured = false;
                        beginPos = i + 1;
                        recursionCount++;
                        previousNameDefined = nameDefined;
                        nameDefined = false;
                    }
                }

                if (c == VariableNameOpenCapture)
                {
                    capturesRecursionCount++;
                }

                if (c == VariableNameOpenCapture)
                {
                    // TODO: ❓❓ choose if yes or no ❓❓
                    /*if (isCapturingName)
                        throw new Exception($"( not allowed in: {new string(text)} at pos {i}");*/

                    skipSymbol = !isCapturingName;
                    isCapturingName = true;
                    isNameCaptured = true;
                }

                if (c == VariableNameEndCapture)
                {
                    if (isCapturingName)
                        i++;
                    break;
                }

                if (!skipSymbol)
                {
                    nameDefined = true;
                }

                if (!skipSymbol
                    && !isCapturingName
                    && !IsVariableNameValidCharacter(
                        c,
                        previousChar
                        ))
                {
                    break;
                }
                i++;
            }

            var endPos = i - 1;

            return isNameCaptured ?

                (new string(text[(beginPos + 1)..endPos])
                    , beginPos - 1
                    , endPos
                    , isNameCaptured
                    , recursionCount + 1
                    , previousNameDefined)

                : (new string(text[beginPos..(endPos + 1)])
                    , beginPos - 1
                    , endPos
                    , isNameCaptured
                    , recursionCount + 1
                    , previousNameDefined);
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
                // exclude non printable characters & flow control caracters
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

        public static string GetPath(string name)
        {
            if (name == null)
                return null;
            var t = SplitPath(name);
            if (t.Length == 0)
                return "";
            return string.Join(VariableNamePathSeparator, t[0..^1]);
        }
    }
}
