namespace OrbitalShell.Lib.Sys
{
    public class StringWrapper
    {
        public string Prefix;

        public string Postfix;

        protected string _str;

        public StringWrapper(string str="",string prefix="",string postfix="") {
            _str = str;
            Prefix = prefix;
            Postfix = postfix;
        }

        string _text => Prefix + _str + Postfix;

        public override string ToString() => _text;


    }
}