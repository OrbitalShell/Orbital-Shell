using OrbitalShell.Component.Console;
using System;
using System.Collections.Generic;
using static OrbitalShell.DotNetConsole;
using sc = System.Console;

namespace OrbitalShell.Component.CommandLine.Reader
{
    /// <summary>
    /// TODO: move to Component.UI.Input
    /// </summary>
    public static class Interaction
    {
        /// <summary>
        /// ask a question, read input to enter. returns true if input is 'y' or 'Y'
        /// </summary>
        /// <param name="question"></param>
        /// <returns>returns true if input is 'y' or 'Y'</returns>
        public static bool Confirm(string question)
        {
            var r = false;
            void endReadln(IAsyncResult result)
            {
                r = result.AsyncState?.ToString()?.ToLower() == "y";
            }
            var cmdlr = new CommandLineReader(null, question + "? ", null);
            cmdlr.BeginReadln(endReadln, null, true, false);
            Out.Echoln();
            return r;
        }

        /// <summary>
        /// display a bar of text that wait a text input
        /// </summary>
        /// <param name="text"></param>
        /// <param name="inputMap"></param>
        /// <returns></returns>
        public static object InputBar(string text, List<InputMap> inputMaps)
        {
            object r = null;
            Out.Echo($"{Colors.InteractionBar}{text}{ANSI.RSTXTA}");
            bool end = false;
            string input = "";
            while (!end)
            {
                var c = sc.ReadKey(true);
                if (!Char.IsControl(c.KeyChar))
                    input += c.KeyChar;
                bool partialMatch = false;
                foreach (var inputMap in inputMaps)
                {
                    var match = inputMap.Match(input, c);
                    if (match == InputMap.ExactMatch)
                    {
                        r = inputMap.Code;
                        end = true;
                        break;
                    }
                    partialMatch |= match == InputMap.PartialMatch;
                }
                if (!partialMatch) input = "";

                System.Diagnostics.Debug.WriteLine($"{input}");
            }
            return r;
        }
    }
}
