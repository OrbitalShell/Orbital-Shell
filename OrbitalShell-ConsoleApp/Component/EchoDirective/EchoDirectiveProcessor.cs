using System.Collections.Generic;
using static OrbitalShell.DotNetConsole;
using OrbitalShell.Console;
using System;

namespace OrbitalShell.Component.EchoDirective
{
    /// <summary>
    /// echo directives processor
    /// <para>&#9989; drives directly the writer, but this dependency could be removed (returning text instead).</para>
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


        public EchoDirectiveProcessor(
            ConsoleTextWriterWrapper writer,
            CommandMap commandMap
        )
        {
            Writer = writer;
            this.CommandMap = commandMap;
        }

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
                while (cmd == null && i < n)
                {
                    // TODO echo directive parser too slow : first check the presence of a directive block starts: '(' (directive map is huge!)
                    foreach (var ccmd in CommandMap.Map)
                    {
                        if (s.IndexOf(CommandBlockBeginChar + ccmd.Key, i) == i)
                        {
                            cmd = ccmd;
                            cmdindex = i;
                            isAssignation = ccmd.Key.EndsWith("=");
                        }
                    }
                    if (cmd == null)
                        tmps += s.Substring(i, 1);
                    i++;
                }
                if (cmd == null)
                {
                    Writer.ConsolePrint(tmps, false);

                    printSequences?.Add(new EchoSequence((string)null, 0, i - 1, null, tmps, startIndex));
                    return;
                }
                else i = cmdindex;

                if (!string.IsNullOrEmpty(tmps))
                {
                    Writer.ConsolePrint(tmps);

                    printSequences?.Add(new EchoSequence((string)null, 0, i - 1, null, tmps, startIndex));
                }

                int firstCommandEndIndex = 0;
                int k = -1;
                string value = null;
                if (isAssignation)
                {
                    firstCommandEndIndex = s.IndexOf(CommandValueAssignationChar, i + 1);
                    if (firstCommandEndIndex > -1)
                    {
                        firstCommandEndIndex++;
                        var subs = s.Substring(firstCommandEndIndex);
                        if (subs.StartsWith(CodeBlockBegin))
                        {
                            firstCommandEndIndex += CodeBlockBegin.Length;
                            k = s.IndexOf(CodeBlockEnd, firstCommandEndIndex);
                            if (k > -1)
                            {
#pragma warning disable IDE0057
                                value = s.Substring(firstCommandEndIndex, k - firstCommandEndIndex);
#pragma warning restore IDE0057
                                k += CodeBlockEnd.Length;
                            }
                            else
                            {
                                Writer.ConsolePrint(s);

                                printSequences?.Add(new EchoSequence((string)null, i, s.Length - 1, null, s, startIndex));
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
                    if (inCmt && s.IndexOf(CodeBlockEnd, j) == j)
                    {
                        inCmt = false;
                        j += CodeBlockEnd.Length - 1;
                    }
                    if (!inCmt && s.IndexOf(CodeBlockBegin, j) == j)
                    {
                        inCmt = true;
                        j += CodeBlockBegin.Length - 1;
                    }
                    if (!inCmt && s.IndexOf(CommandSeparatorChar, j) == j && firstCommandSeparatorCharIndex == -1)
                        firstCommandSeparatorCharIndex = j;
                    if (!inCmt && s.IndexOf(CommandBlockEndChar, j) == j)
                        break;
                    j++;
                }
                if (j == s.Length)
                {
                    Writer.ConsolePrint(s);

                    printSequences?.Add(new EchoSequence((string)null, i, j, null, s, startIndex));
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
                        var t = cmdtxt.Split(CommandValueAssignationChar);
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
                                    if (TraceCommandErrors) Error(ex.Message);
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
                                    if (TraceCommandErrors) Error(ex.Message);
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
                                if (TraceCommandErrors) Error(ex.Message);
                            }
                            result = null;
                        }
                        // else: no command: do nothing
                    }
                    // <--

                    if (Writer.FileEchoDebugEnabled && Writer.FileEchoDebugCommands)
                        Writer.EchoDebug(CommandBlockBeginChar + cmd.Value.Key + value + CommandBlockEndChar);

                    printSequences?.Add(new EchoSequence(cmd.Value.Key.Substring(0, cmd.Value.Key.Length - 1), i, j, value, null, startIndex));
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
                                if (TraceCommandErrors) Error(ex.Message);
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
                                    if (TraceCommandErrors) Error(ex.Message);
                                }
                                result = null;
                            }
                            // else: no command: do nothing
                        }
                    }
                    // <--

                    if (Writer.FileEchoDebugEnabled && Writer.FileEchoDebugCommands)
                        Writer.EchoDebug(CommandBlockBeginChar + cmd.Value.Key + CommandBlockEndChar);

                    printSequences?.Add(new EchoSequence(cmd.Value.Key, i, j, value, null, startIndex));
                }
                if (result != null)
                    Writer.Echo(result, false);    // recurse

                if (firstCommandSeparatorCharIndex > -1)
                {
                    s = CommandBlockBeginChar + s.Substring(firstCommandSeparatorCharIndex + 1 /*+ i*/ );
                    startIndex += firstCommandSeparatorCharIndex + 1;
                }
                else
                {
                    if (j + 1 < s.Length)
                    {
                        s = s.Substring(j + 1);
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