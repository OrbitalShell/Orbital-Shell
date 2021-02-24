using System.Collections.Generic;
using OrbitalShell.Component.Console;
using System;
using System.Text;

namespace OrbitalShell.Component.EchoDirective
{
    /// <summary>
    /// echo directives processor
    /// <para>&#9989; drives directly the writer (simple approach to execute print directives)</para>
    /// </summary>
    public class EchoDirectiveProcessor
    {
        public delegate object Command1pIntDelegate(int n = 1);
        public delegate object Command2pIntDelegate(int x = 1, int y = 1);
        public delegate object Command2pDelegate(object parameter, object argument);
        public delegate object CommandDelegate(object x);
        public delegate void SimpleCommandDelegate();
        public readonly ConsoleTextWriterWrapper Writer;
        public readonly CommandMap CommandMap;
        readonly IDotNetConsole Console;

        public EchoDirectiveProcessor(
            ConsoleTextWriterWrapper writer,
            CommandMap commandMap
        )
        {
            Console = writer.Console;
            Writer = writer;
            this.CommandMap = commandMap;
        }

        readonly StringBuilder tmpsb = new StringBuilder(100000);

        public void ParseTextAndApplyCommands(
            string s,
            bool lineBreak = false,
            string tmps = "",
            bool doNotEvalutatePrintDirectives = false,
            EchoSequenceList printSequences = null,
            int startIndex = 0)
        {
            lock (Writer.Lock)
            {
                int i = 0;
                KeyValuePair<string, (SimpleCommandDelegate simpleCommand, CommandDelegate command, object parameter)>? cmd = null;
                int n = s.Length;
                bool isAssignation = false;
                int cmdindex = -1;

                //var tmpsb = new StringBuilder(tmps, s.Length * 20);
                tmpsb.Clear();
                tmpsb.Append(tmps);

                while (cmd == null && i < n)
                {
                    if (s[i] == Console.CommandBlockBeginChar)
                    {
                        foreach (var ccmd in CommandMap.Map)
                        {
                            if (s.IndexOf(Console.CommandBlockBeginChar + ccmd.Key, i) == i)
                            {
                                cmd = ccmd;
                                cmdindex = i;
                                isAssignation = ccmd.Key.EndsWith("=");
                            }
                        }
                        if (cmd == null)
                            //tmps += s[i];
                            tmpsb.Append(s[i]);
                    }
                    else
                        //tmps += s[i];
                        tmpsb.Append(s[i]);
                    i++;
                }

                var stmps = tmpsb.ToString();

                if (cmd == null)
                {
                    Writer.ConsolePrint(/*tmps*/stmps, false);

                    printSequences?.Add(new EchoSequence(Console,(string)null, 0, i - 1, null, /*tmps*/stmps, startIndex));
                    return;
                }
                else i = cmdindex;

                if (!string.IsNullOrEmpty(/*tmps*/stmps))
                {
                    Writer.ConsolePrint(/*tmps*/stmps);

                    printSequences?.Add(new EchoSequence(Console,(string)null, 0, i - 1, null, /*tmps*/stmps, startIndex));
                }

                tmpsb.Clear();
                //tmpsb = null;

                int firstCommandEndIndex = 0;
                int k = -1;
                string value = null;
                if (isAssignation)
                {
                    firstCommandEndIndex = s.IndexOf(Console.CommandValueAssignationChar, i + 1);
                    if (firstCommandEndIndex > -1)
                    {
                        firstCommandEndIndex++;
                        var subs = s[firstCommandEndIndex..];
                        if (subs.StartsWith(Console.CodeBlockBegin))
                        {
                            firstCommandEndIndex += Console.CodeBlockBegin.Length;
                            k = s.IndexOf(Console.CodeBlockEnd, firstCommandEndIndex);
                            if (k > -1)
                            {
                                value = s[firstCommandEndIndex..k];
                                //value = s.Substring(firstCommandEndIndex, k - firstCommandEndIndex);
                                k += Console.CodeBlockEnd.Length;
                            }
                            else
                            {
                                Writer.ConsolePrint(s);

                                printSequences?.Add(new EchoSequence(Console,(string)null, i, s.Length - 1, null, s, startIndex));
                                return;
                            }
                        }
                    }
                }

                int j = i + cmd.Value.Key.Length;
                bool inCmt = false;
                int firstCommandSeparatorCharIndex = -1;
                while (j < s.Length)
                {
                    if (inCmt && s.IndexOf(Console.CodeBlockEnd, j) == j)
                    {
                        inCmt = false;
                        j += Console.CodeBlockEnd.Length - 1;
                    }
                    if (!inCmt && s.IndexOf(Console.CodeBlockBegin, j) == j)
                    {
                        inCmt = true;
                        j += Console.CodeBlockBegin.Length - 1;
                    }
                    if (!inCmt && s.IndexOf(Console.CommandSeparatorChar, j) == j && firstCommandSeparatorCharIndex == -1)
                        firstCommandSeparatorCharIndex = j;
                    if (!inCmt && s.IndexOf(Console.CommandBlockEndChar, j) == j)
                        break;
                    j++;
                }
                if (j == s.Length)
                {
                    Writer.ConsolePrint(s);

                    printSequences?.Add(new EchoSequence(Console,(string)null, i, j, null, s, startIndex));
                    return;
                }

                var cmdtxt = s[i..j];
                if (firstCommandSeparatorCharIndex > -1)
                    cmdtxt = cmdtxt.Substring(0, firstCommandSeparatorCharIndex - i/*-1*/);

                object result = null;
                if (isAssignation)
                {
                    if (value == null)
                    {
                        var t = cmdtxt.Split(Console.CommandValueAssignationChar);
                        value = t[1];
                    }

                    // ❎ --> exec echo directive command : with ASSIGNATION
                    if (!doNotEvalutatePrintDirectives)
                    {
                        if (cmd.Value.Value.command != null)
                        {
                            if (cmd.Value.Value.parameter == null)
                            {
                                try
                                {
                                    result = cmd.Value.Value.command(value);    // CommandDelegate
                                }
                                catch (Exception ex)
                                {
                                    if (Console.TraceCommandErrors) Console.Error(ex.Message);
                                }
                            }
                            else
                            {
                                try
                                {
                                    result = cmd.Value.Value.command((cmd.Value.Value.parameter, value));
                                }
                                catch (Exception ex)
                                {
                                    if (Console.TraceCommandErrors) Console.Error(ex.Message);
                                }
                            }
                        }
                        else
                            if (cmd.Value.Value.simpleCommand != null)
                        {
                            try
                            {
                                cmd.Value.Value.simpleCommand();
                            }
                            catch (Exception ex)
                            {
                                if (Console.TraceCommandErrors) Console.Error(ex.Message);
                            }
                            result = null;
                        }
                        // else: no command: do nothing
                    }
                    // <--

                    if (Writer.FileEchoDebugEnabled && Writer.FileEchoDebugCommands)
                        Writer.EchoDebug(Console.CommandBlockBeginChar + cmd.Value.Key + value + Console.CommandBlockEndChar);

                    printSequences?.Add(new EchoSequence(Console,cmd.Value.Key[0..^1], i, j, value, null, startIndex));
                }
                else
                {
                    // ❎ --> exec echo directive command : NO ASSIGNATION
                    if (!doNotEvalutatePrintDirectives)
                    {
                        if (cmd.Value.Value.command != null)
                        {
                            try
                            {
                                result = cmd.Value.Value.command(cmd.Value.Value.parameter);
                            }
                            catch (Exception ex)
                            {
                                if (Console.TraceCommandErrors) Console.Error(ex.Message);
                            }
                        }
                        else
                        {
                            if (cmd.Value.Value.simpleCommand != null)
                            {
                                try
                                {
                                    cmd.Value.Value.simpleCommand();
                                }
                                catch (Exception ex)
                                {
                                    if (Console.TraceCommandErrors) Console.Error(ex.Message);
                                }
                                result = null;
                            }
                            // else: no command: do nothing
                        }
                    }
                    // <--

                    if (Writer.FileEchoDebugEnabled && Writer.FileEchoDebugCommands)
                        Writer.EchoDebug(Console.CommandBlockBeginChar + cmd.Value.Key + Console.CommandBlockEndChar);

                    printSequences?.Add(new EchoSequence(Console,cmd.Value.Key, i, j, value, null, startIndex));
                }
                if (result != null)
                    Writer.Echo(result, false);    // recurse

                if (firstCommandSeparatorCharIndex > -1)
                {
                    s = Console.CommandBlockBeginChar + s[(firstCommandSeparatorCharIndex + 1) /*+ i*/ ..];
                    startIndex += firstCommandSeparatorCharIndex + 1;
                }
                else
                {
                    if (j + 1 < s.Length)
                    {
                        s = s[(j + 1)..];
                        startIndex += j + 1;
                    }
                    else
                        s = string.Empty;
                }

                if (!string.IsNullOrEmpty(s)) ParseTextAndApplyCommands(s, lineBreak, "", doNotEvalutatePrintDirectives, printSequences, startIndex);
            }
        }



    }
}