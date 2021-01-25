using System;
using System.Linq;
using System.Collections.Generic;

namespace DotNetConsoleAppToolkit.Component.Parser.NonRecursiveFunctionalGrammar
{
    public class SyntacticBlockList : List<SyntacticBlock>
    {
        public bool IsSelected;

        public SyntacticBlockList Clone( ) {
            var r = new SyntacticBlockList();
            r.AddRange(this);
            r.IsSelected = IsSelected;
            return r;
        }
        
        /// <summary>
        /// get the real length of the text without ansi sequences non printed characters
        /// </summary>
        /// <returns>length of visible part of the text</returns>
        public int GetTextLength() => GetText().Length;

        /// <summary>
        /// gets the text part of the syntactic elements
        /// </summary>
        /// <returns>string without ansi sequences</returns>
        public string GetText() => string.Join("",this.Where(x => !x.IsANSISequence).Select(x => x.Text));
    }
}