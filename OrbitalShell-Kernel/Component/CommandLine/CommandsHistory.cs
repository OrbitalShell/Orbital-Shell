using DotNetConsoleAppToolkit.Lib.FileSystem;
using System.Collections.Generic;
using System.IO;
using static DotNetConsoleAppToolkit.DotNetConsole;

namespace DotNetConsoleAppToolkit.Component.CommandLine
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

        public CommandsHistory() {
        }
        
        public void Init(string folderPath,string fileName)
        {
            Folder = folderPath;
            FileName = fileName;
            var lines = File.ReadAllLines(FilePath.FullName);
            foreach (var line in lines) HistoryAppend(line);
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

        public void HistoryAppend(string s)
        {
            _history.Add(s);
            _historyIndex = _history.Count;
        }

        public void HistorySetIndex(int index,bool checkIndex=true)
        {
            if (index == -1)
                index = _history.Count;
            else
                index--;
            if (checkIndex && (index < 0 || index >= _history.Count))
                Errorln($"history index out of bounds (1..{_history.Count})");
            else _historyIndex = index;
        }

        public void ClearHistory() => _history.Clear();

        #endregion
    }
}
