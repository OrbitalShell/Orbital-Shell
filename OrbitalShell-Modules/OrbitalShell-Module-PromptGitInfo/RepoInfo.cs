using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using OrbitalShell.Lib;

namespace OrbitalShell.Module.PromptGitInfo
{
    public class RepoInfo
    {
        public RepoStatus RepoStatus;
        public Dictionary<char, int> X = new Dictionary<char, int>();
        public Dictionary<char, int> Y = new Dictionary<char, int>();
        public string ErrorMessage;
        public static List<char> Names = new List<char> { 'M', 'A', 'D', 'R', 'C', 'U', ' ', '?', '!' , '#' };

        public int IndexChanges, WorktreeChanges, LocalAdded, LocalDeleted, WorktreeAdded, WorktreeDeleted, Untracked, Behind;
        public bool IsModified => (IndexChanges + WorktreeChanges + LocalAdded + LocalDeleted + WorktreeAdded + WorktreeDeleted + Untracked + Behind) > 0;

        public RepoInfo()
        {
            foreach (var c in Names) { X.Add(c, 0); Y.Add(c, 0); }
        }

        public void Update()
        {
            IndexChanges = X.Values.Aggregate(0, (a, b) => a + b) - X['?'] - X['A'] - X['D'] - X['#'];
            WorktreeChanges = Y.Values.Aggregate(0, (a, b) => a + b) - Y['?'] - Y['A'] - Y['D'];
            LocalAdded = X['A'];
            LocalDeleted = X['D'];
            WorktreeAdded = Y['A'];
            WorktreeDeleted = Y['D'];
            Untracked = X['?'];
            Behind = X['#'];
            RepoStatus = RepoStatus.UpToDate;
            if ((IndexChanges > 0 || WorktreeChanges > 0) && Untracked==0) RepoStatus = RepoStatus.Modified;
            if ((IndexChanges > 0 || WorktreeChanges > 0) && Untracked>0) RepoStatus = RepoStatus.ModifiedUntracked;           
            if (Behind > 0) RepoStatus = RepoStatus.Behind;
        }

        public void Inc(char lName, char rName,string line)
        {
            if (!X.ContainsKey(lName)) X[lName] = 0;
            if (!Y.ContainsKey(rName)) Y[rName] = 0;

            // untracked
            if (lName == '?' && rName == '?') { X[lName]++; Y[rName]++; }
            // ignored
            if (lName == '!' && rName == '!') { X[lName]++; Y[rName]++; }
            // updated
            if (lName == 'M' && rName == ' ') X[lName]++;
            // added
            if (lName == 'A' && rName == ' ') X[lName]++;
            // deleted
            if (lName == 'D' && rName == ' ') X[lName]++;
            // renamed
            if (lName == 'R' && rName == ' ') X[lName]++;
            // copied
            if (lName == 'C' && rName == ' ') X[lName]++;
            // unstaged change
            if (lName == ' ' && rName != ' ') Y[rName]++;
            // changed both equals
            if (lName == rName && lName != ' ' && lName != '?' && lName != '#' && lName != '!') X[lName]++;
            // branch info: behind commites
            if (lName == '#' && rName == '#')
            {
                // branch
                Match m = null;
                if ((m = line.Match(@"ahead (\d+)\]$")) != null && m.Success)
                    X['#'] = -Convert.ToInt32(m.Groups[1].Value);
                if ((m = line.Match(@"behind (\d+)\]$")) != null && m.Success)
                    X['#'] = Convert.ToInt32(m.Groups[1].Value);
            }
        }

        public override string ToString()
        {
            var r = "";
            foreach (var kv in X)
            {
                r += kv.Key + "=" + kv.Value + Environment.NewLine;
            }
            return r;
        }
    }
}