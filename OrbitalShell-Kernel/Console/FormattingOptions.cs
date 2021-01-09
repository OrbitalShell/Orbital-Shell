using System;

namespace DotNetConsoleAppToolkit.Console
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

        public FormattingOptions() { }

        public FormattingOptions(bool lineBreak)
        {
            LineBreak = lineBreak;
        }
    }
}
