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

        public bool IsRoot => Label == null;

        public TreeNode() {
            _Init();
        }

        public TreeNode(string label) { 
            if (label==null) throw new Exception("label can't be null");
            _Init();            
            Label = label;
        }

        void _Init() {
            lock (_counterLock) {
                ID = _counter;
                _counter++;
            }
        }

        public override string ToString() => $" {ID}:  {Label}  -> {string.Join(" ",SubNodes.Values.Select(x => x.Label))}";
    }
}