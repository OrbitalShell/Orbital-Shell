using OrbitalShell.Component.CommandLine.Parsing;
using OrbitalShell.Console;
using System.Collections.Generic;
using System.Linq;
using static OrbitalShell.Component.EchoDirective.Shortcuts;

namespace OrbitalShell.Component.CommandLine.Pipeline
{
    public class PipelineWorkUnit
    {
        public List<StringSegment> Segments = new List<StringSegment>();

        public int StartIndex;
        public int EndIndex;

        public string Command;

        public PipelineWorkUnit NextUnit;

        public PipelineCondition PipelineCondition = PipelineCondition.NotAppliable;
        public StringSegment HereScript = null;
        public StringSegment InputRedirectSource = null;
        public StringSegment OutputRedirectTarget = null;
        public List<object> RedirectUnions = new List<object>();
        public bool IsInputRedirected => InputRedirectSource != null || HereScript != null;
        public bool IsOutputRedirected => OutputRedirectTarget != null || HereScript != null;

        public PipelineWorkUnit() { }

        public PipelineWorkUnit(StringSegment stringSegment)
        {
            Segments.Add(stringSegment);
            StartIndex = stringSegment.X;
        }

        public override string ToString()
        {
            var redirectUnions = "";
            var attributes = new string[]
            {
                CommandLineSyntax.PipelineConditionToStr(PipelineCondition),
                HereScript!=null?CommandLineSyntax.HereScript:null,
                InputRedirectSource!=null?"<":null,
                OutputRedirectTarget!=null?">":null,
                redirectUnions
            };
            var attrs = string.Join(' ', attributes.Where(x => x != null)).Trim();
            if (!string.IsNullOrWhiteSpace(attrs)) attrs = $" ({attrs})";
            return $"{Darkgreen}{StartIndex}-{EndIndex}: {string.Join(' ', Segments.Select(x => x.Text))}"
                + attrs
                + ((NextUnit == null) ? "" : " " + (NextUnit == null ? "" : Br) + NextUnit.ToString());
        }



    }
}
