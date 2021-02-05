using System.Collections.Generic;

namespace OrbitalShell.Component.Console
{
    /// <summary>
    /// all ASCII codes that we take into account for the targetted terminals referential
    /// from https://en.wikipedia.org/wiki/ANSI_escape_code
    /// </summary>
    public static class ASCII
    {
        #region codes

        public const char NUL = (char)0;
        public const char SOH = (char)1;
        public const char STX = (char)2;
        public const char ELX = (char)3;
        public const char EOT = (char)4;
        public const char ENQ = (char)5;
        public const char ACK = (char)6;
        public const char BEL = (char)7;
        public const char BS = (char)8;
        public const char HT = (char)9;
        public const char LF = (char)10;
        public const char VT = (char)11;
        public const char FF = (char)12;
        public const char CR = (char)13;
        public const char SO = (char)14;
        public const char SI = (char)15;
        public const char DLE = (char)16;
        public const char DC1 = (char)17;
        public const char DC2 = (char)18;
        public const char DC3 = (char)19;
        public const char DC4 = (char)20;
        public const char NAK = (char)21;
        public const char SYN = (char)22;
        public const char ETB = (char)23;
        public const char CAN = (char)24;
        public const char EM = (char)25;
        public const char SUB = (char)26;
        public const char ESC = (char)27;
        public const char FS = (char)28;
        public const char GS = (char)29;
        public const char RS = (char)30;
        public const char US = (char)31;
        public const char SP = (char)32;

        #endregion

        #region texts (that should be understandable in command line and kernel methods)

        public const string NUL_TXT = "\\0";
        public const string BEL_TXT = "\\a";
        public const string BS_TXT = "\\b";
        public const string HT_TXT = "\\t";
        public const string LF_TXT = "\\n";
        public const string VT_TXT = "\\v";
        public const string FF_TXT = "\\f";
        public const string CR_TXT = "\\r";
        public const string ESC_TXT = "\\e";

        #endregion

        #region util

        static Dictionary<char,string> _codesToNames = null;
        static Dictionary<char,string> _codesToTexts = new Dictionary<char, string>() {
            { NUL , NUL_TXT },
            { BEL , BEL_TXT },
            { BS , BS_TXT },
            { HT , HT_TXT },
            { LF , LF_TXT },
            { VT , VT_TXT },
            { FF , FF_TXT },
            { CR , CR_TXT },
            { ESC , ESC_TXT },
        };

        static void _requireCodesToNames() {
            if (_codesToNames!=null) return;
            _codesToNames = new Dictionary<char, string>();
            var fields = typeof(ASCII).GetFields();
            foreach ( var field in fields ) 
                if (!field.Name.EndsWith("_TXT"))
                    _codesToNames.Add((char)field.GetValue(null),field.Name);
        }

        static string GetPreferredRepresentation(char c,string labelFormat="<{0}>",string textFormat="{0}")
        {
            if (_codesToTexts.TryGetValue(c,out var txt)) return string.Format(textFormat,txt);
            return string.Format(labelFormat,_codesToNames[c]);
        }

        public static string GetNonPrintablesCodesAsLabel(string s,bool includeSP,string labelFormat="<{0}>",string textFormat="{0}")
        {
            _requireCodesToNames();
            var r = "";
            foreach ( var c in s ) {
                var min = (int)SP + (includeSP?0:-1);
                r += (c<=min)? GetPreferredRepresentation(c,labelFormat,textFormat) : ""+c;                
            }
            return r;
        }

        #endregion
    }
}