using OrbitalShell.Lib.Sys;

namespace OrbitalShell.Component.Console.Formats
{
    public class FormatingOptions :
        ShellObject,
        IClonable<FormatingOptions>
    {
        static FormatingOptions _instance;
        public static FormatingOptions Instance
        {
            get
            {
                if (_instance == null) _instance = new FormatingOptions();
                return _instance;
            }
        }

        public bool LineBreak = true;

        public bool IsRawModeEnabled = false;

        public bool IsObjectDumpEnabled = true;

        public int Level = 0;

        public FormatingOptions() { }

        public FormatingOptions(FormatingOptions o) => InitFrom(o);

        public FormatingOptions InitFrom(FormatingOptions o)
        {
            this.IsRawModeEnabled = o.IsRawModeEnabled;
            this.LineBreak = o.LineBreak;
            this.IsObjectDumpEnabled = o.IsObjectDumpEnabled;
            this.Level = o.Level;
            return this;
        }

        public FormatingOptions(
            bool lineBreak = true,
            bool isRawModeEnabled = false,
            bool isObjectDumpEnabled = true,
            int level = 0)
        {
            LineBreak = lineBreak;
            IsRawModeEnabled = isRawModeEnabled;
            IsObjectDumpEnabled = isObjectDumpEnabled;
            Level = level;
        }

        public virtual FormatingOptions Clone() => new(this);

        public FormatingOptions AddLevel()
        {
            Level++;
            return this;
        }
    }
}
