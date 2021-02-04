using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Lib.FileSystem;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using OrbitalShell.Lib;

namespace OrbitalShell.Component.Shell
{
    public class CommandsAlias
    {
        public string FileName { get; protected set; }
        public string Folder { get; protected set; }
        public FilePath FilePath => new FilePath(Path.Combine(Folder, FileName));

        SortedDictionary<string, string> _aliases = new SortedDictionary<string, string>();
        public IReadOnlyDictionary<string, string> Aliases => new ReadOnlyDictionary<string, string>(_aliases);
        public IReadOnlyCollection<string> AliasNames => _aliases.Keys;

        public CommandsAlias()
        {
        }

        public void Init(CommandEvaluationContext context, string folderPath, string fileName)
        {
            Folder = folderPath;
            FileName = fileName;
            context.CommandLineProcessor.CommandBatchProcessor.RunBatch(context, FilePath.FullName);
        }

        public void SaveAliases(CommandEvaluationContext context)
        {
            var defaultsAliasesFilePath =
                Path.Combine(
                    context.CommandLineProcessor.Settings.DefaultsFolderPath,
                    context.CommandLineProcessor.Settings.CommandsAliasFileName);
            var lines = File.ReadAllLines(defaultsAliasesFilePath).ToList();
            foreach (var kvp in _aliases)
                lines.Add(BuildAliasCommand(kvp.Key, kvp.Value));
            File.WriteAllLines(context.CommandLineProcessor.Settings.CommandsAliasFilePath, lines);
        }

        public void AddOrReplaceAlias(CommandEvaluationContext context, string name, string text)
        {
            _aliases.AddOrReplace(name, text);
        }

        public void UnsetAlias(CommandEvaluationContext context, string name)
        {
            if (_aliases.ContainsKey(name)) _aliases.Remove(name);
            else context.Errorln($"can't unset alias '{name}' because it is not defined");
        }

        public string GetAlias(string name)
        {
            return _aliases.TryGetValue(name, out var text) ? text : null;
        }

        public static string BuildAliasCommand(string name, string text)
        {
            return $"alias {name} {Str.AssureIsQuoted(text)}";
        }
    }
}
