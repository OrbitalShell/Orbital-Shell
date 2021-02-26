using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Console;
using OrbitalShell.Component.Shell.Hook;
using OrbitalShell.Lib.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;

namespace OrbitalShell.Component.Shell
{
    public class CommandsHistory
    {
        public string FileName { get; protected set; }
        public string Folder { get; protected set; }

        #region attributes

        readonly List<string> _history = new List<string>();
        int _historyIndex = -1;

        public List<string> History => new List<string>(_history);

        #endregion

        public CommandsHistory()
        {
        }

        public void Init(string folderPath, string fileName)
        {
            Folder = folderPath;
            FileName = fileName;
            var lines = File.ReadAllLines(FilePath.FullName);
            foreach (var line in lines) HistoryAppend(null,line);
        }

        public FilePath FilePath
        {
            get
            {
                var userPath = Folder;
                return new FilePath(Path.Combine(userPath, FileName));
            }
        }

        #region history operations

        public string GetBackwardHistory()
        {
            if (_historyIndex < 0)
                _historyIndex = _history.Count + 1;
            if (_historyIndex >= 1)
                _historyIndex--;
            return (_historyIndex < 0 || _history.Count == 0 || _historyIndex >= _history.Count) ? null : _history[_historyIndex];
        }

        public string GetForwardHistory()
        {
            if (_historyIndex < 0 || _historyIndex >= _history.Count)
                _historyIndex = _history.Count;
            if (_historyIndex < _history.Count - 1) _historyIndex++;

            return (_historyIndex < 0 || _history.Count == 0 || _historyIndex >= _history.Count) ? null : _history[_historyIndex];
        }

        public bool HistoryContains(string s) => _history.Contains(s);

        public void HistoryAppend(CommandEvaluationContext context,string s)
        {
            _history.Add(s);
            _historyIndex = _history.Count;
            context?.CommandLineProcessor.ModuleManager.ModuleHookManager.InvokeHooks(context, Hooks.PostHistoryAppend, this);
        }

        public void HistorySetIndex(int index, bool checkIndex = true)
        {
            if (index == -1)
                index = _history.Count;
            else
                index--;
            if (checkIndex && (index < 0 || index >= _history.Count))
                throw new IndexOutOfRangeException($"history index out of bounds (1..{_history.Count})");
            else _historyIndex = index;
        }

        public void ClearHistory(CommandEvaluationContext context)
        {
            _history.Clear();
            context?.CommandLineProcessor.ModuleManager.ModuleHookManager.InvokeHooks(context, Hooks.ClearHistory, this);
        }

        #endregion
    }
}
