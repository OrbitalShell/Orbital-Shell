using System;
using System.Linq;
using System.Collections.Generic;

namespace OrbitalShell.Module.PromptGitInfo
{
    public class RepoInfo
    {
        public RepoStatus RepoStatus;
        public Dictionary<char, int> X = new Dictionary<char, int>();
        public Dictionary<char, int> Y = new Dictionary<char, int>();
        public string ErrorMessage;
        public static List<char> Names = new List<char> { 'M', 'A', 'D', 'R', 'C', 'U', ' ', '?', '!' };

        public int LocalChanges, RemoteChanges, LocalAdded, LocalDeleted, RemoteAdded, RemoteDeleted, Untracked;
        public bool IsModified => (LocalChanges + RemoteChanges + LocalAdded + LocalDeleted + RemoteAdded + RemoteDeleted + Untracked) > 0;

        public RepoInfo()
        {
            foreach (var c in Names) { X.Add(c, 0); Y.Add(c, 0); }
        }

        public void Update()
        {
            LocalChanges = X.Values.Aggregate(0, (a, b) => a + b) - X['?'] - X['A'] - X['D'];
            RemoteChanges = Y.Values.Aggregate(0, (a, b) => a + b) - Y['?'] - Y['A'] - Y['D']; ;
            LocalAdded = X['A'];
            LocalDeleted = X['D'];
            RemoteAdded = Y['A'];
            RemoteDeleted = Y['D'];
            Untracked = X['?'];
            RepoStatus = RepoStatus.UpToDate;
            if (LocalChanges > 0 && RemoteChanges == 0) RepoStatus = RepoStatus.Ahead;
            if (LocalChanges >= 0 && RemoteChanges > 0) RepoStatus = RepoStatus.Behind;
        }

        public void Inc(char lName, char rName)
        {
            if (!X.ContainsKey(lName)) X[lName] = 0;
            if (!Y.ContainsKey(rName)) Y[rName] = 0;

            // untracked
            if (lName == '?' && rName == '?') { X[lName]++; Y[lName]++; }
            // ignored
            if (lName == '!' && rName == '!') { X[lName]++; Y[lName]++; }
            // updated in index
            if (lName == 'M' && rName == ' ') X[lName]++;
            // added in index
            if (lName == 'A' && rName == ' ') X[lName]++;
            // deleted from index
            if (lName == 'D' && rName == ' ') X[lName]++;
            // renamed from index
            if (lName == 'R' && rName == ' ') X[lName]++;
            // copied in index
            if (lName == 'C' && rName == ' ') X[lName]++;
            // remote change
            if (lName == ' ' && rName != ' ') Y[lName]++;
            // changed both equals
            if (lName == rName && lName != ' ' && lName != '?' && lName != '!') X[lName]++;
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