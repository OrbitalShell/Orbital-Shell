using OrbitalShell.Component.CommandLine.Parsing;
using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.CommandLine.Variable;
using OrbitalShell.Console;
using System;
using System.Collections.Generic;
using static OrbitalShell.Component.CommandLine.Parsing.CommandLineSyntax;
using static OrbitalShell.DotNetConsole;
using static OrbitalShell.Component.EchoDirective.Shortcuts;
using OrbitalShell.Component.EchoDirective;

namespace OrbitalShell.Component.CommandLine.Pipeline
{
    public class PipelineParser
    {
        public static PipelineWorkUnit GetPipeline(
            CommandEvaluationContext context,
            ArraySegment<StringSegment> segments
            )
        {
            /* 
             * cmdLine ::= ( (InputRedirect? (Cmd CmdParamOptions?) OutputRedirect* Pipe? )+
             */
            PipelineWorkUnit model = null;
            PipelineWorkUnit workUnit = null;
            string cmd = null;
            int i = 0;
            StringSegment nextSegment() => i + 1 < segments.Count ? segments[i + 1] : null;
            StringSegment nxtSeg = null;

            while (i < segments.Count)
            {
                var segment = segments[i];
                var s = segment.Text;
                if (cmd == null)
                {
                    if (IsNotAnOperator(s))
                    {
                        cmd = s;
                        workUnit = BuildPipelineWorkUnit(segment);
                        workUnit.Command = cmd;
                        if (model == null) model = workUnit;
                    }
                    else
                        // unexpected operator
                        throw new ParseErrorException(
                            new ParseError($"unexpected operator: {s}", i, segment.X, null));
                }
                else
                {
                    nxtSeg = nextSegment();

                    if (IsPipeOperator(s))
                    {
                        if (nxtSeg == null)
                            // expected pipe something
                            throw new ParseErrorException(
                                new ParseError($"unexpected operator: {CommandLineSyntax.Pipeline}", i, segment.X, null));
                        else
                        {
                            workUnit.NextUnit = BuildPipelineWorkUnit(nxtSeg);
                            var previousWorkUnit = workUnit;
                            workUnit = workUnit.NextUnit;
                            if (s == CommandLineSyntax.Pipeline) previousWorkUnit.PipelineCondition = PipelineCondition.Always;
                            if (s == ConditionalPipelineFail) previousWorkUnit.PipelineCondition = PipelineCondition.Error;
                            if (s == ConditionalPipelineSuccess) previousWorkUnit.PipelineCondition = PipelineCondition.Success;
                            cmd = nxtSeg.Text;
                            workUnit.Command = cmd;
                            if (model == null) model = workUnit;
                            i++;
                        }
                    }
                    else
                    {
                        if (IsNotAnOperator(s))
                        {
                            workUnit.Segments.Add(segment);
                        }
                        else
                        {
                            if (IsRedirectInput(s))
                            {
                                if (nxtSeg == null)
                                    throw new ParseErrorException(
                                        new ParseError($"unexpected operator {s}", i, segment.X, null));
                                if (IsHereScript(s))
                                    workUnit.HereScript = nxtSeg;
                                else
                                    workUnit.InputRedirectSource = nxtSeg;
                                i++;
                            }
                            if (IsRedirectOutput(s))
                            {
                                if (nxtSeg == null)
                                    throw new ParseErrorException(
                                        new ParseError($"unexpected operator {s}", i, segment.X, null));
                                workUnit.OutputRedirectTarget = nxtSeg;
                                i++;
                            }
                        }
                    }
                }

                i++;
            }

            if (context.ShellEnv.GetValue<bool>(ShellEnvironmentVar.debug_pipeline, false))
                context.Out.Echoln(Darkcyan + model);

            return model;
        }

        static PipelineWorkUnit BuildPipelineWorkUnit(StringSegment segment)
        {
            var workUnit = new PipelineWorkUnit(segment);
            return workUnit;
        }


    }

}
