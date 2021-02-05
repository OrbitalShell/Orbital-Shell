using System;

namespace OrbitalShell.Component.Console
{
    public class FormattingOptions : ShellObject
    {
        static new FormattingOptions _instance;
        public new static FormattingOptions Instance
        {
            get
            {
                if (_instance == null) _instance = new FormattingOptions();
                return _instance;
            }
        }

        public bool LineBreak = true;

        public bool IsRawModeEnabled = false;

        public FormattingOptions() { }

        public FormattingOptions(FormattingOptions o) => InitFrom(o);

        public void InitFrom(FormattingOptions o)
        {
            this.IsRawModeEnabled = o.IsRawModeEnabled;
            this.LineBreak = o.LineBreak;
        }

        public FormattingOptions(
            bool lineBreak,
            bool isRawModeEnabled)
        {
            LineBreak = lineBreak;
            IsRawModeEnabled = isRawModeEnabled;
        }
    }
}
