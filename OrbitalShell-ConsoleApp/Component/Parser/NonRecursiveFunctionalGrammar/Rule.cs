using System;
using System.Linq;
using System.Collections.Generic;

namespace DotNetConsoleAppToolkit.Component.Parser.NonRecursiveFunctionalGrammar
{
    public class Rule : List<string> {
        static int _counter = 0;
        static object _counterLock = new object();
        public int ID;

        public TreePath TreePath;

        public string Key;

        public Rule() => _Init();

        public Rule(IEnumerable<string> range) : base(range) => _Init();

        void _Init() {
            TreePath = new TreePath(this,-1);
            lock (_counterLock) {
                ID = _counter;
                _counter ++;
            }
        }

        public void SetKeyFromTreePath() {
            Key = TreePath.Key;
        }
    }
}