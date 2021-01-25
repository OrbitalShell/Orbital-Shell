using System;
using System.Linq;
using System.Collections.Generic;

namespace DotNetConsoleAppToolkit.Component.Parser.NonRecursiveFunctionalGrammar
{
    public class NonRecursiveFunctionGrammarParser
    {
        #region predefined grammar functions

        /// <summary>
        /// ascii<32 && !=27 (Single Code Function)
        /// </summary>
        public const string SCF = "scf";

        /// <summary>
        /// char
        /// </summary>
        public const string CHAR = "char";

        /// <summary>
        /// CHAR*
        /// </summary>
        public const string TEXT = "text";

        /// <summary>
        /// (NUM? ;? NUM?)*
        /// </summary>
        public const string NUMLIST = "numlist?";

        /// <summary>
        /// [0-9]+
        /// </summary>
        public const string NUM = "num";

        #endregion

        #region attributes

        Dictionary<string,string> _lexs = new Dictionary<string,string>();

        List<Rule> _rules = new List<Rule>();
        Dictionary<string,Rule> _rulesIndex = new Dictionary<string,Rule>();

        TreeNode _gramTree;

        #endregion

        #region init

        public NonRecursiveFunctionGrammarParser(
            IEnumerable<string> grammarDefinition)
        {
            var rules = new List<string>();
            foreach ( var line in grammarDefinition ) {
                var s = line.Trim();
                if (!string.IsNullOrWhiteSpace(s)) {
                    if (!s.StartsWith("//")) {
                        if (s.Contains(":=")) {
                            // lex def
                            var t = s.Split(":=");
                            var v = t[1].Trim();
                            if (v.StartsWith("\\x")) {
                                v = v.Replace("\\x","0x");
                                v = ""+(char)Convert.ToInt32(v , 16);
                            }
                            _lexs.Add(t[0].Trim(),v);
                        } else {
                            // rule def
                            rules.Add(s);
                        }
                    }
                }
            }
            
            foreach ( var rule in rules )
                AddRule(rule);

            _BuildGramTree();
        }

        #endregion

        #region grammar tree

        void _BuildGramTree() 
        {
            _gramTree = new TreeNode();
            foreach ( var rule in _rules ) {
                _AddRuleToGramTree(_gramTree,rule,rule);
            }
            foreach ( var rule in _rules ) {
                rule.SetKeyFromTreePath();
                if (!_rulesIndex.TryGetValue(rule.Key,out var rule2))
                    _rulesIndex.Add(rule.Key,rule);
                else {
                    // ignore duplicated rules
                    //throw new Exception($"duplicated rule error: rule1={rule} rule2={rule2}");
                }
            }
        }
        
        void _AddRuleToGramTree(TreeNode node,IEnumerable<string> ruleTerms,Rule rule ) 
        {
            var rt = ruleTerms.FirstOrDefault();
            if (rt==null) return;
            TreeNode rtNode;
            if (node.SubNodes.TryGetValue(rt,out var subNode)) rtNode = subNode;
            else node.SubNodes.Add(rt,rtNode = new TreeNode(rt));
            rule.TreePath.Add(rtNode);
            _AddRuleToGramTree(rtNode,ruleTerms.Skip(1),rule);
        }

        void AddRule(string rule) 
        {
            var rules = ReadRule(rule);
            _rules.AddRange(rules);
        }

        List<Rule> ReadRule(string rule) 
        {
            var t = rule.Split(' ');
            var seqs = new List<Rule>() { new Rule() };
            
            void AddSeq(string seq) {
                foreach ( var aseq in seqs ) aseq.Add(seq);  
            }          

            foreach ( var x in t ) {
                if (_lexs.TryGetValue(x,out var lex))
                {
                    var tlex = lex.Split(' ');
                    foreach ( var colex in tlex ) {
                        var nlex = colex;
                        if (_lexs.TryGetValue(colex,out var trlex)) nlex = trlex;
                        AddSeq(nlex);
                    }                        
                }            
                else {
                    if (!x.Contains(','))
                        AddSeq(x);
                    else {
                        var ors = x.Split(',');                        
                        var nseqs = new List<Rule>();
                        foreach ( var seq in seqs )
                        {
                            foreach ( var or in ors )
                            {
                                var nseq = new Rule(seq);
                                var sor = or;
                                var tor = sor.Split("\\x2;");
                                foreach ( var ttor in tor ) {
                                    if (_lexs.TryGetValue(x,out var trlex)) sor = trlex;
                                    nseq.Add(sor);
                                }
                                nseqs.Add(nseq);
                            }       
                        }
                        seqs = nseqs;
                    }
                }
             }
            for (int i=0;i<seqs.Count;i++)
                for (int j=0;j<seqs[i].Count;j++)
                    if (_lexs.TryGetValue(seqs[i][j],out var lx)) seqs[i][j] = lx;
            return seqs;
        }

        #endregion

        #region parsing

        public SyntacticBlockList Parse(string s) 
        {
            var t = s.ToCharArray();
            int i = 0;
            var paths = new SyntacticBlockList();
            var rootNode = _gramTree;
            _Parse(ref t,i,rootNode,rootNode,paths);  
            return paths;      
        }

        void _Parse(
            ref char[] chars,
            int i,
            TreeNode rootNode,
            TreeNode node,
            SyntacticBlockList paths,
            TreePath currentPath=null,
            bool returnImmediately = false,
            int recurseLevel = 0
            ) 
        {
            if (chars.Length==0 || i==chars.Length) return;
            
            bool matching = false;
            bool partialMatching = false;

            while (i<chars.Length)
            {
                bool lastChar = i==chars.Length-1;
                var c = chars[i];
                var sc = ""+c;
                matching = false;
                var beginIndex = i;

                /*if (recurseLevel==0 && node==null) {
                    // start by grammar multi roots
                    foreach ( var kv in rootNode.SubNodes ) 
                    {
                        _Parse(ref chars,i,rootNode,kv.Value,paths,currentPath,recurseLevel); 
                    }
                    SelectPaths(ref chars,ref i,ref matching,ref partialMatching,rootNode,ref node);
                }*/

                if (!node.IsRoot) 
                {
                    if (node.Label==sc)
                    {
                        // exact match - potential matching sequence
                        matching = true;        
                        partialMatching = false;     
                    }
                    else 
                    {
                        if (node.Label==SCF)
                        {
                            matching = c<32 && c!=27;
                            partialMatching = false;
                        }
                        else if (node.Label==CHAR)
                        {
                            matching = true;
                            partialMatching = false;
                        }
                        else if (node.Label==TEXT)
                        {
                            matching = true;
                            partialMatching = true;
                        } 
                        else if (node.Label==NUMLIST)
                        {
                            if (char.IsDigit(c) || c==';') {
                                matching = true;
                                partialMatching = true;
                            } else 
                                matching = !partialMatching;
                        } 
                        else if (node.Label==NUM)
                        {
                            if (char.IsDigit(c)) 
                            {
                                matching = true;
                                partialMatching = true;

                            }
                        } else matching = false;                  
                    }
                    
                    i++;
                }
                else {
                    matching = true;
                    partialMatching = false;
                }

                #region functions

                void AddRemainingText(ref char[] chars,SyntacticBlock selected)
                {
                    if (selected.Index>0) {
                        if ( paths.Count>1 )
                        {
                            int holeSize,blockIndex;
                            if (paths.Count>1)
                            {                            
                                var previous = paths[paths.Count-2];
                                holeSize = selected.Index - ( previous.Index + previous.Text.Length-1 ) - 1;
                                blockIndex = previous.Index + previous.Text.Length;
                            } else
                            {
                                holeSize = selected.Index;
                                blockIndex = 0;
                            }

                            if (holeSize>0) {
                                var textBlock = new SyntacticBlock(
                                    blockIndex,
                                    null,
                                    new string(chars,blockIndex,holeSize),
                                    true,false
                                );
                                paths.Insert(paths.Count-1,textBlock);
                            }
                        } else {
                            var textBlock = new SyntacticBlock(
                                    0,
                                    null,
                                    new string(chars,0,selected.Index),
                                    true,false
                                );
                                paths.Insert(paths.Count-1,textBlock);
                        }
                    }
                }

                void SelectPaths(
                    ref char[] chars,
                    ref int i,
                    ref bool matching,
                    ref bool partialMatching,
                    TreeNode rootNode,
                    ref TreeNode node
                ) 
                {
                    // any sub syntax of another syntax is removed
                    // equivalent are selected dependings on rule priority
                    var _paths = paths.Where(x => !x.IsSelected).ToList();
                    if (_paths.Count==0) return;
                    List<SyntacticBlock> t;
                    if (_paths.Count>1)
                    {
                        var maxLength = _paths.Max(x => x.Text.Length);
                        var longests = _paths.Where(x => x.Text.Length == maxLength);
                        t = new List<SyntacticBlock>(longests);
                        t.Sort((x,y) => x.SyntacticRule.Rule.ID.CompareTo(y.SyntacticRule.Rule.ID));
                    } else t = _paths;

                    // keep only the right syntax
                    var selected = t.First();
                    selected.IsSelected = true;
                    matching = false;
                    partialMatching = false;
                    var tmp = new List<SyntacticBlock>(paths);
                    foreach (var p in tmp) if (!p.IsSelected && p!=selected) paths.Remove(p);

                    // add text node if any
                    
                    AddRemainingText(ref chars,selected);

                    // setup parser to go on on next position

                    i += selected.Text.Length-1;
                    
                    if (rootNode!=null) node = rootNode;
                    currentPath = null;                    
                }

                #endregion

                if ( 
                    (matching && !partialMatching) ||
                    (!matching && partialMatching)                   
                    ) {
                    if (!partialMatching || !matching)
                    {
                        if (!node.IsRoot) 
                        {
                            if (currentPath==null) currentPath = new TreePath(null,beginIndex);
                            else currentPath = new TreePath(currentPath.Rule,currentPath.Index,currentPath);   
                            currentPath.Add(node);
                        }

                        if (node.SubNodes.Count==0) {

                            // gram path match
                            var sblock = 
                                new SyntacticBlock(
                                    currentPath.Index,
                                    currentPath,
                                    new string(chars,currentPath.Index,i-currentPath.Index ));
                            var key = sblock.SyntacticRule.Key;
                            if (_rulesIndex.TryGetValue(key,out var matchingRule)) {
                                sblock.SyntacticRule.Rule = matchingRule;
                            } else 
                                throw new Exception($"a syntactic pattern has been recognized but no corresponding rule can be found (grammar is ambiguous ?) : {sblock}");

                            paths.Add(sblock);
                            
                            if (recurseLevel==0)
                            {
                                SelectPaths(ref chars,ref i,ref matching,ref partialMatching,rootNode,ref node);
                            }
                            else
                                return; // stop on result
                        }
                        else
                        {
                            i += (partialMatching)?-1:0; /*ignore last match*/
                            //var m = matching;
                            //var pm = partialMatching;
                            //matching = partialMatching = false;
                            // explore gram next
                            foreach ( var kv in node.SubNodes ) {
                                var pathsCount = paths.Count;
                                _Parse(
                                    ref chars,i,rootNode,kv.Value,paths,currentPath,
                                    node.IsRoot,
                                    recurseLevel+1); 
                                var hasNewPath = paths.Count > pathsCount;
                                //if (node.IsRoot)
                                //    SelectPaths(ref chars,ref i,ref matching,ref partialMatching,rootNode,ref node);
                            }

                            if (recurseLevel==0 )
                            {
                                SelectPaths(ref chars,ref i,ref matching,ref partialMatching,rootNode,ref node);
                                if (node.IsRoot) i++;
                            }
                            else
                                // stop on result
                                return;
                        }      
                    } 
                    else {                        
                        // else scan next of current pattern
                    }              
                } 
                else
                {
                    if (!(matching && partialMatching))
                    {
                        if (currentPath!=null) {
                            // path doesn't match
                            currentPath = null;
                            return; // rule not fit at i - stop on result
                        }
                        else {
                            if (returnImmediately) return;
                        }
                    } 
                    else {
                        // scan next of current pattern ('node' partial matching)
                    }
                }
            }

            // end string

            #region handle tail

            if (currentPath!=null) {
                // path match to the end of the string but not ended
                // incomplete path
                var sblock = new SyntacticBlock(
                        currentPath.Index,
                        currentPath,
                        new string(chars,currentPath.Index,i-currentPath.Index ),
                        true
                        );
                var key = sblock.SyntacticRule.Key;
                if (_rulesIndex.TryGetValue(key,out var matchingRule)) {
                    sblock.SyntacticRule.Rule = matchingRule;
                }
                paths.Add(sblock);
            } 
            else 
            {
                // add remaining text
                int beginIndex=0;
                if (paths.Count>0) {
                    var last = paths.Last();
                    beginIndex = last.Index+last.Text.Length;
                }
                var textBlock = new SyntacticBlock(
                    beginIndex,
                    null,
                    new string(chars,beginIndex,chars.Length-beginIndex),
                    true,false
                );
                paths.Add(textBlock);
            }

            #endregion
        }

        #endregion
    }
}