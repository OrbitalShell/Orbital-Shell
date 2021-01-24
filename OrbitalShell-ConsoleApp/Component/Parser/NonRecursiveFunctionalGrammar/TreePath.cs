using System;
using System.Linq;
using System.Collections.Generic;

namespace DotNetConsoleAppToolkit.Component.Parser.NonRecursiveFunctionalGrammar
{
    public class TreePath : List<TreeNode>
    {
        public Rule Rule;

        public int Index;

        public TreePath(Rule rule,int index) {
            Rule = rule;
            Index = index;
        } 

        public TreePath(Rule rule,int index,IEnumerable<TreeNode> o) : base(o) {
            Rule = rule;
            Index = index;
        } 

        public override string ToString() => $"<#{Rule?.ID}> " + string.Join(' ',this.Select(x => x.Label));

        public string Key => string.Join( '-', this.Select(x => x.ID) );
    }
}