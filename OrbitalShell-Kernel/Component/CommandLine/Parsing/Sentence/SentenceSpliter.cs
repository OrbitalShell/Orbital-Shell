using System;
using System.Collections.Generic;
using System.Linq;

using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Console;

using static OrbitalShell.Component.CommandLine.Parsing.Sentence.CommandLineSyntax;

namespace OrbitalShell.Component.CommandLine.Parsing.Sentence
{
    public class SentenceSpliter : ISentenceSpliter
    {
        /// <summary>
        /// indicates if the char is a separator at top input stream level
        /// <para>// TODO: ⚠️⚠️⚠️ handle here more separators: eol,cr,... ⚠️⚠️⚠️</para>
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public bool IsTopLevelSeparator(char c)
        {
            return c == SpaceSeparator;
        }

        public char FindMetaChar(ref string expr)
        {
            char metachar = (char)0;
            var badmetachar = expr.Contains(metachar);
            if (badmetachar)
            {
                var r = new Random();
                while (badmetachar)
                {
                    metachar = (char)r.Next(10000, 20000);
                    badmetachar = expr.Contains(metachar);
                }
            }

            return metachar;
        }

        /// <summary>
        /// split on SPACE " ' 
        /// parse top level syntax of a command line
        /// </summary>
        /// <param name="context"></param>
        /// <param name="expr"></param>
        /// <returns></returns>
        public StringSegment[] SplitExpr(
            CommandEvaluationContext _,
            string expr
            )
        {
            if (expr == null) return Array.Empty<StringSegment>();

            var splits = new List<StringSegment>();
            var t = expr.Trim().ToCharArray();
            char prevc = SpaceSeparator;
            int i = 0;
            var inString = false;
            var inSingleQuoteString = false;
            var inDoubleQuoteString = false;
            int beginCurStrIndex = 0;
            var prevStr = "";
            var k = t.Length;

            // find a meta char for private coding that is not in the string - try char 0 or a random in 10000-20000
            var metachar = FindMetaChar(ref expr);

            while (i < k)
            {
                var c = t[i];
                var isNeutralizer = c == NeutralizerSymbol;
                var neutralizedBySymbol = prevc == NeutralizerSymbol;
                bool isSeparator = IsTopLevelSeparator(c);

                var isStringSeparator = !neutralizedBySymbol && (c == SingleQuote || c == DoubleQuote);
                var isSingleQuoteStringSeparator = c == SingleQuote && !neutralizedBySymbol;
                var isDoubleQuoteStringSeparator = c == DoubleQuote && !neutralizedBySymbol;

                if (!inString)
                {
                    if (isStringSeparator)
                    {
                        prevStr = ((i - 1) >= beginCurStrIndex) ? new string(t[beginCurStrIndex..i]) : "";
                        inSingleQuoteString = isSingleQuoteStringSeparator;
                        inDoubleQuoteString = isDoubleQuoteStringSeparator;
                        inString = true;
                        beginCurStrIndex = i + 1;
                    }

                    if (isSeparator)
                    {
                        // end of non separator pattern + begin of separator pattern

                        splits.Add(new StringSegment(prevStr + new string(t[beginCurStrIndex..i]), beginCurStrIndex, i - 1));

                        // skip * separators
                        beginCurStrIndex = i;
                        while (IsTopLevelSeparator(t[++i])) ;

                        splits.Add(new StringSegment(prevStr + new string(t[beginCurStrIndex..i]), beginCurStrIndex, i - 1));

                        prevStr = "";
                        beginCurStrIndex = i;
                        i--;
                    }
                }
                else
                {
                    if ((inSingleQuoteString && isSingleQuoteStringSeparator)
                        || (inDoubleQuoteString && isDoubleQuoteStringSeparator))
                    {
                        if (i == t.Length - 1)
                        {
                            splits.Add(new StringSegment(prevStr + new string(t[beginCurStrIndex..i]), beginCurStrIndex, i - 1));
                            prevStr = "";
                            beginCurStrIndex = t.Length;
                        }
                        else
                        {
                            var t2 = t[0..(beginCurStrIndex - 1)].ToList();
                            t2.AddRange(t[beginCurStrIndex..i].ToList());
                            if (i < t.Length - 1)
                                t2.AddRange(t[(i + 1)..t.Length].ToList());
                            t = t2.ToArray();
                            k = t.Length;
                            beginCurStrIndex--;
                            i -= 2;
                        }
                        inSingleQuoteString = inDoubleQuoteString = false;
                        inString = false;
                    }
                }

                if (isNeutralizer)
                {
                    // neutralizer symbol: should be removed from parsed split
                    char? nextChar = i < t.Length - 1 ? t[i + 1] : null;

                    if (nextChar != null)
                    {
                        var nc = nextChar.Value;
                        if (TopLevelNeutralizedSubstitutions.TryGetValue(nc, out var newChars))
                        {
                            t[i] = newChars.c0 == 0 ? metachar : newChars.c0;
                            t[i + 1] = newChars.c1 == 0 ? metachar : newChars.c1;
                        }
                        else
                        {
                            if (NeutralizableTopLevelSeparators.Contains(nc))
                                t[i] = metachar;
                        }
                    }
                }

                prevc = c;
                i++;
            }

            if ((t.Length - 1) >= beginCurStrIndex)
            {
                splits.Add(new StringSegment(prevStr + new string(t[beginCurStrIndex..(t.Length)]), beginCurStrIndex, i - 1));
                prevStr = "";
            }
#if debugParser
            for (i = 0; i < splits.Count; i++)
            {
                context.Out.Echoln($"(f=darkcyan){i} : {splits[i]}");
            }
#endif
            // remove splits that fit the separator pattern (length>=1) - keep any defined empty value: '' "" (length=0)

            splits = splits.Where(x => !(string.IsNullOrWhiteSpace(x.Text) && x.Length >= 1)).ToList();     // TODO: ⚠️⚠️⚠️ must perform a real separator pattern matching check ⚠️⚠️⚠️

            // clean up meta chars in splits

            foreach (var split in splits)
            {
                split.SetText(split.Text.Replace($"{metachar}", ""));
            }

            return splits.ToArray();
        }
    }
}
