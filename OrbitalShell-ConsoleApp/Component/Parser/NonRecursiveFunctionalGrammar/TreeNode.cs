using System;
using System.Linq;
using System.Collections.Generic;

namespace DotNetConsoleAppToolkit.Component.Parser.NonRecursiveFunctionalGrammar
{
    public class TreeNode {
        static int _counter = 0;
        static object _counterLock = new object();
        public int ID;
        public string Label;
        public readonly Dictionary<string,TreeNode> SubNodes = new Dictionary<string, TreeNode>();
        public TreeNode(string label) { 
            lock (_counterLock) {
                ID = _counter;
                _counter++;
            }
            Label = label;
        }
        public override string ToString() => $" {ID}:  {Label}  -> {string.Join(" ",SubNodes.Values.Select(x => x.Label))}";
    }
}