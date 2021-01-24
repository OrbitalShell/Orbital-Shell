using System.IO;
using DotNetConsoleAppToolkit.Component.Parser.NonRecursiveFunctionalGrammar;

namespace DotNetConsoleAppToolkit.Component.Parser.ANSI
{
    public static class ANSIParser
    {
        #region attributes

        public const string GrammarFileName = "ansi-seq-patterns.txt";
        
        static NonRecursiveFunctionGrammarParser _parser;

        #endregion

        #region init

        static ANSIParser() {
            var ap = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var p = Path.Combine( Path.GetDirectoryName(ap),"Component","Parser","ANSI",GrammarFileName );    
            var lines = File.ReadLines(p);
            _parser = new NonRecursiveFunctionGrammarParser(lines);
        }

        #endregion

        public static SyntacticBlockList Parse(string s) => _parser.Parse(s);

    }
}