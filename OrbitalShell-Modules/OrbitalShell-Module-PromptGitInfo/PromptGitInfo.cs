using OrbitalShell.Component.CommandLine.CommandModel;
using OrbitalShell.Component.Shell.Module;
using OrbitalShell.Component.Shell.Hook;
using OrbitalShell.Component.Shell;
using OrbitalShell.Component.Shell.Variable;
using OrbitalShell.Component.CommandLine.Processor;
using System;
using System.IO;
using System.Linq;
using OrbitalShell.Component.Console;
using System.Collections.Generic;

namespace OrbitalShell.Module.PromptGitInfo
{
    /// <summary>
    /// module commands : prompt git infos
    /// </summary>
    [Commands("prompt git info module commands")]
    [CommandsNamespace(CommandNamespace.tools, ToolNamespace)]
    [Hooks]
    public class PromptGitInfo : ICommandsDeclaringType
    {
        #region attributes 

        public const string ToolNamespace = "git";
        public const string ToolVarSettingsName = "promptInfo";
        public const string VarIsEnabled = "isEnabled";
        public const string VarInfoBackgroundColor = "infoBackgroundColor";
        public const string VarBehindBackgroundColor = "behindBackgroundColor";
        public const string VarUpToDateBackgroundColor = "upToDateBackgroundColor";
        public const string VarModifiedBackgroundColor = "modifiedBackgroundColor";
        public const string VarModifiedUntrackedBackgroundColor = "modifiedUntrackedBackgroundColor";
        public const string VarUnknownBackgroundColor = "unknownBackgroundColor";
        public const string VarModifiedTextTemplate = "modifiedTextTemplate";
        public const string VarBehindTextTemplate = "behindTextTemplate";
        public const string VarTextTemplateNoData = "noDataTextTemplate";
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
            var branchSymbol = Unicode.EdgeRowLeft;
            var sepSymbol = Unicode.RightChevron;
            context.ShellEnv.AddNew(_namespace, VarIsEnabled, true, false);
            var behindColor = "(b=darkred)";
            var infoColor = ANSI.SGR_SetBackgroundColor8bits(237/*59*/);

            context.ShellEnv.AddNew(_namespace, VarInfoBackgroundColor, infoColor);

            context.ShellEnv.AddNew(
                _namespace,
                VarModifiedTextTemplate,
                $"%bgColor%(f=white) %repoName% {branchSymbol} %branch% %sepSymbol%%errorMessage%{infoColor}+%localAdded% ~%localChanges% -%localDeleted% | ~%remoteChanges% -%remoteDeleted% ?%untracked%(rdc) ", false);

            context.ShellEnv.AddNew(
                _namespace,
                VarBehindTextTemplate,
                $"%bgColor%(f=white) %repoName% {branchSymbol} %branch% %sepSymbol%%errorMessage%{infoColor}+%localAdded% ~%localChanges% -%localDeleted% | ~%remoteChanges% -%remoteDeleted% ?%untracked% {behindColor}{Unicode.ArrowDown}%behind%(rdc) ", false);
           
            context.ShellEnv.AddNew(
                _namespace,
                VarTextTemplateNoData,
                $"%bgColor%(f=white) %repoName% {branchSymbol} %branch%%errorMessage%(rdc) ", false);
            
            context.ShellEnv.AddNew(
                _namespace,
                VarTextTemplateNoRepository,
                $"(b=darkblue)(f=white) {sepSymbol} %errorMessage%(rdc) ", false);
            
            context.ShellEnv.AddNew(_namespace, VarBehindBackgroundColor, behindColor, false);
            context.ShellEnv.AddNew(_namespace, VarUpToDateBackgroundColor, ANSI.SGR_SetBackgroundColor8bits(22), false);
            context.ShellEnv.AddNew(_namespace, VarModifiedBackgroundColor, ANSI.SGR_SetBackgroundColor8bits(130), false);
            context.ShellEnv.AddNew(_namespace, VarModifiedUntrackedBackgroundColor, ANSI.SGR_SetBackgroundColor8bits(166), false);
            context.ShellEnv.AddNew(_namespace, VarUnknownBackgroundColor, "(b=darkblue)", false);
        }

        #endregion

        #region Command

        /// <summary>
        /// enable or disable prompt git info
        /// </summary>
        [Command("setup prompt git infos")]
        public CommandVoidResult PromptInfo(
            CommandEvaluationContext context,
            [Option("e", "enable", "if true enable the prompt customization, otherwise disable it", true, true)] bool isEnabled = true
        )
        {
            context.Variables.SetValue(Variables.Nsp(VariableNamespace.env + "", _namespace), VarIsEnabled, isEnabled);
            return CommandVoidResult.Instance;
        }

        #endregion

        #region prompt hook

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
                var repoName = Path.GetFileName(Path.GetDirectoryName(repoPath));

                string text =
                     context.ShellEnv.GetValue<string>(
                         _namespace,
                         repoPath != null ?
                            ((repo.Behind>0)? VarBehindTextTemplate :
                            ((repo.IsModified) ? VarModifiedTextTemplate : VarTextTemplateNoData))
                            : VarTextTemplateNoRepository
                     );

                var bgColor = "";
                switch (repo.RepoStatus)
                {
                    case RepoStatus.Behind:
                        bgColor = context.ShellEnv.GetValue<string>(_namespace, VarBehindBackgroundColor);
                        break;
                    case RepoStatus.Modified:
                        bgColor = context.ShellEnv.GetValue<string>(_namespace, VarModifiedBackgroundColor);
                        break;
                    case RepoStatus.ModifiedUntracked:
                        bgColor = context.ShellEnv.GetValue<string>(_namespace, VarModifiedUntrackedBackgroundColor);
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
                    { "localChanges" , repo.IndexChanges+"" },
                    { "localDeleted" , repo.LocalDeleted+"" },
                    { "remoteChanges" , repo.WorktreeChanges+"" },
                    { "remoteAdded" , repo.WorktreeAdded+"" },
                    { "remoteDeleted" , repo.WorktreeDeleted+"" },
                    { "untracked" , repo.Untracked+"" },
                    { "isBehindSymbol" , "" },
                    { "repoName" , repoName },
                    { "behind" , repo.Behind+"" },
                    { "sepSymbol" , "" }
                };
                text = _SetVars(context, text, vars);

                context.Out.Echo(text, false);
            }
        }

        #endregion

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

        RepoInfo _GetRepoStatus(
            CommandEvaluationContext context,
            string repoPath)
        {
            var r = new RepoInfo { RepoStatus = RepoStatus.UpToDate };

            try
            {
                context.CommandLineProcessor.ShellExec(context, "git", "status -s -b -u -M --porcelain", out var output, true, false);
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
                            #       behind
                            !       ignored
                        */
                        if (!string.IsNullOrWhiteSpace(line) && line.Length > 2)
                        {
                            var x = line[0];
                            var y = line[1];
                            r.Inc(x, y,line);
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