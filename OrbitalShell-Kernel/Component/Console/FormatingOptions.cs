using System;

namespace OrbitalShell.Component.Console
{
    public class FormatingOptions : ShellObject
    {
        static new FormatingOptions _instance;
        public new static FormatingOptions Instance
        {
            get
            {
                if (_instance == null) _instance = new FormatingOptions();
                return _instance;
            }
        }

        public bool LineBreak = true;

        public bool IsRawModeEnabled = false;

        public FormatingOptions() { }

        public FormatingOptions(FormatingOptions o) => InitFrom(o);

        public FormatingOptions InitFrom(FormatingOptions o)
        {
            this.IsRawModeEnabled = o.IsRawModeEnabled;
            this.LineBreak = o.LineBreak;
            return this;
        }

        public FormatingOptions(
            bool lineBreak,
            bool isRawModeEnabled)
        {
            LineBreak = lineBreak;
            IsRawModeEnabled = isRawModeEnabled;
        }
    }
}
