namespace DotNetConsoleAppToolkit.Lib.Sys
{
    public class StringWrapper
    {
        public string Prefix;

        protected string _str;

        public StringWrapper(string str="",string prefix="") {
            _str = str;
            Prefix = prefix;
        }

        string _text => Prefix + _str;

        public override string ToString() => _text;


    }
}