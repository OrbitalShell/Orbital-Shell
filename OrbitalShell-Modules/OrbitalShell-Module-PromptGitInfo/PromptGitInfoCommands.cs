using OrbitalShell.Component.CommandLine.CommandModel;
using OrbitalShell.Component.Shell.Module;
using OrbitalShell.Component.Shell.Hook;
using OrbitalShell.Component.Shell;
using OrbitalShell.Component.Shell.Variable;
using OrbitalShell.Component.CommandLine.Processor;
using System;
using System.IO;
using System.Linq;
using OrbitalShell.Console;
using System.Collections.Generic;

namespace OrbitalShell.Module.PromptGitInfo
{
    /// <summary>
    /// module commands : prompt git infos
    /// </summary>
    [Commands("prompt git info module commands")]
    [CommandsNamespace(CommandNamespace.tools, ToolNamespace)]
    [Hooks]
    public class PromptGitInfoCommands : ICommandsDeclaringType
    {
        #region attributes 

        public const string ToolNamespace = "git";
        public const string ToolVarSettingsName = "promptInfo";
        public const string VarIsEnabled = "isEnabled";
        public const string VarBehindBackgroundColor = "behindBackgroundColor";
        public const string VarUpToDateBackgroundColor = "upToDateBackgroundColor";
        public const string VarAheadBackgroundColor = "aheadBackgroundColor";
        public const string VarUnknownBackgroundColor = "unknownBackgroundColor";
        public const string VarTextTemplate = "template";
        public const string VarTextTemplateNoData = "templateNoData";
        public const string VarTextTemplateNoRepository = "templateNoRepository";
        public const string GitFolder = ".git";
        string _namespace => Variables.Nsp(ShellEnvironmentNamespace.com + "", ToolNamespace, ToolVarSettingsName);

        #endregion

        #region init

        /// <summary>
        /// init module hook
        /// </summary>
        [Hook(Hooks.ModuleInit)]
        public void Init(CommandEvaluationContext context)
        {
            // init settings
            var branchSymbol = Unicode.EdgeFlatTopRight;
            context.ShellEnv.AddValue(_namespace, VarIsEnabled, true, false);
            context.ShellEnv.AddValue(
                _namespace,
                VarTextTemplate,
                $"%bgColor%(f=white) %branch% {branchSymbol} %errorMessage%{ANSI.SGR_SetBackgroundColor8bits(100)}+%localAdded% ~%localChanges% -%localDeleted% | ~%remoteChanges% -%remoteDeleted% %isBehindSymbol%(rdc) ", false);
            context.ShellEnv.AddValue(
            _namespace,
                VarTextTemplateNoData,
                $"%bgColor%(f=white) %branch% {branchSymbol} %errorMessage%", false);
            context.ShellEnv.AddValue(
                _namespace,
                VarTextTemplateNoRepository,
                $"(b=darkblue)(f=white) {branchSymbol} %errorMessage%(rdc) ", false);
            context.ShellEnv.AddValue(_namespace, VarBehindBackgroundColor, "(b=darkred)", false);
            context.ShellEnv.AddValue(_namespace, VarUpToDateBackgroundColor, "(b=darkgreen)", false);
            context.ShellEnv.AddValue(_namespace, VarAheadBackgroundColor, ANSI.SGR_SetBackgroundColor8bits(172), false);
            context.ShellEnv.AddValue(_namespace, VarUnknownBackgroundColor, "(b=darkblue)", false);
        }

        #endregion

        #region Commands

        /// <summary>
        /// enable or disable prompt git info TODO: fix do not works the first time (datavalue cloned ?)
        /// </summary>
        [Command("setup prompt git infos")]
        public CommandVoidResult PromptInfo(
            CommandEvaluationContext context,
            [Option("e", "enable", "if true enable the prompt customization, otherwise disable it", true, true)] bool isEnabled = true
        )
        {
            context.ShellEnv.SetValue(_namespace, VarIsEnabled, isEnabled);
            return CommandVoidResult.Instance;
        }

        #endregion

        /// <summary>
        /// prompt begin hook
        /// </summary>
        [Hook(Hooks.PromptOutputBegin)]
        public void PromptOutputBegin(CommandEvaluationContext context)
        {
            if (context.ShellEnv.IsOptionSetted(_namespace, VarIsEnabled))
            {
                var repoPath = _RepoPathExists(Environment.CurrentDirectory);
                var repo = _GetRepoStatus(context, repoPath);

                string text =
                     context.ShellEnv.GetValue<string>(
                         _namespace,
                         repoPath != null ?
                            ((repo.IsModified) ? VarTextTemplate : VarTextTemplateNoData)
                            : VarTextTemplateNoRepository
                     );

                var bgColor = "";
                switch (repo.RepoStatus)
                {
                    case RepoStatus.Behind:
                        bgColor = context.ShellEnv.GetValue<string>(_namespace, VarBehindBackgroundColor);
                        break;
                    case RepoStatus.Ahead:
                        bgColor = context.ShellEnv.GetValue<string>(_namespace, VarAheadBackgroundColor);
                        break;
                    case RepoStatus.UpToDate:
                        bgColor = context.ShellEnv.GetValue<string>(_namespace, VarUpToDateBackgroundColor);
                        break;
                    case RepoStatus.Unknown:
                        bgColor = context.ShellEnv.GetValue<string>(_namespace, VarUnknownBackgroundColor);
                        break;
                }
                var branch = _GetBranch(repoPath);

                var vars = new Dictionary<string, string>
                {
                    { "bgColor" , bgColor },
                    { "branch" , branch },
                    { "errorMessage" , repo.ErrorMessage },
                    { "localAdded" , repo.LocalAdded+"" },
                    { "localChanges" , repo.LocalChanges+"" },
                    { "localDeleted" , repo.LocalDeleted+"" },
                    { "remoteChanges" , repo.RemoteChanges+"" },
                    { "remoteAdded" , repo.RemoteAdded+"" },
                    { "remoteDeleted" , repo.RemoteDeleted+"" },
                    { "untracked" , repo.Untracked+"" },
                    { "isBehindSymbol" , repo.RepoStatus==RepoStatus.Behind?"!":""}
                };
                text = _SetVars(context, text, vars);

                context.Out.Echo(text, false);
            }
        }

        #region utils

        string _RepoPathExists(string path)
        {
            while (true)
            {
                string repPath;
                if (Directory.Exists(repPath = Path.Combine(path, GitFolder)))
                    return repPath;
                var lastPath = path;
                path = Path.Combine(path, "..");
                var ppath = new DirectoryInfo(path);
                if (!ppath.Exists) break;
                path = ppath.FullName;
                if (path == lastPath) break;
            }
            return null;
        }

        public enum RepoStatus
        {
            UpToDate,
            Ahead,
            Behind,
            Unknown
        }

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
                LocalChanges = X.Values.Aggregate(0, (a, b) => a + b);
                RemoteChanges = Y.Values.Aggregate(0, (a, b) => a + b);
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
                if (lName == '?' && rName == '?') X[lName]++;
                // ignored
                if (lName == '!' && rName == '!') X[lName]++;
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
                if (lName == rName && lName != ' ') X[lName]++;
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

        RepoInfo _GetRepoStatus(
            CommandEvaluationContext context,
            string repoPath)
        {
            var r = new RepoInfo { RepoStatus = RepoStatus.UpToDate };

            try
            {
                context.CommandLineProcessor.ShellExec(context, "git", "status --porcelain", out var output, true, false);
                //string output = null;
                if (output != null)
                {
                    var lines = output.Split(Environment.NewLine);
                    foreach (var line in lines)
                    {
                        /*
                            C       copied
                            R       renamed
                            D       deleted
                            A       added       
                            M,' '   updated
                            U       updated but not merged
                            ?       untracked
                        */
                        if (!string.IsNullOrWhiteSpace(line) && line.Length > 2)
                        {
                            var x = line[0];
                            var y = line[1];
                            r.Inc(x, y);
                        }
                    }
                    r.Update();
                }
            }
            catch (Exception ex)
            {
                return new RepoInfo { RepoStatus = RepoStatus.Unknown, ErrorMessage = ex.Message };
            }
            return r;
        }

        string _SetVars(CommandEvaluationContext context, string text, Dictionary<string, string> vars)
        {
            foreach (var kv in vars)
                text = text.Replace($"%{kv.Key}%", kv.Value);
            return text;
        }

        string _GetBranch(string repoPath)
        {
            try
            {
                var lines = File.ReadAllLines(Path.Combine(repoPath, "HEAD"));
                var txt = lines.Where(x => !string.IsNullOrWhiteSpace(x)).FirstOrDefault();
                if (txt == null) return "";
                var t = txt.Split("/");
                return t.Last();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        #endregion
    }
}