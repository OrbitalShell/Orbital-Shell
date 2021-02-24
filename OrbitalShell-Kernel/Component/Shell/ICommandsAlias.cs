using System.Collections.Generic;

using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Lib.FileSystem;

namespace OrbitalShell.Component.Shell
{
    public interface ICommandsAlias
    {
        IReadOnlyDictionary<string, string> Aliases { get; }
        IReadOnlyCollection<string> AliasNames { get; }
        string FileName { get; }
        FilePath FilePath { get; }
        string Folder { get; }

        void AddOrReplaceAlias(CommandEvaluationContext context, string name, string text);
        string GetAlias(string name);
        void Init(CommandEvaluationContext context, string folderPath, string fileName);
        void SaveAliases(CommandEvaluationContext context);
        void UnsetAlias(CommandEvaluationContext context, string name);
    }
}