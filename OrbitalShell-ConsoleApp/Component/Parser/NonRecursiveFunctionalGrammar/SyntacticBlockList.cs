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
        /// gets the text part of the syntactic elements
        /// </summary>
        /// <returns>string without ansi sequences</returns>
        public string GetText() => string.Join("",this.Where(x => !x.IsANSISequence).Select(x => x.Text));
    }
}