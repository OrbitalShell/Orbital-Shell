﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace DotNetConsoleAppToolkit.Console
{
    public class TextWriterWrapper
    {
        #region attributes

        public bool IsRedirected { get; protected set; }
        public bool IsBufferEnabled { get; protected set; }
        public static int InitialBufferCapacity = 16384;
        public object Lock => _textWriter;

        protected TextWriter _textWriter;
        protected TextWriter _redirectedTextWriter;
        protected MemoryStream _buffer = new MemoryStream(InitialBufferCapacity);
        protected TextWriter _bufferWriter;

        #region echo to filestream

        public bool FileEchoDebugDumpDebugInfo = true;
        public bool FileEchoDebugCommands = true;
        public bool FileEchoDebugAutoFlush = true;
        public bool FileEchoDebugAutoLineBreak = true;
        public bool FileEchoDebugEnabled => _debugEchoStreamWriter != null;
        protected StreamWriter _debugEchoStreamWriter;
        protected FileStream _debugEchoFileStream;

        #endregion

        #region echo to memory

        public bool ReplicateAutoFlush = true;
        public bool IsEchoEnabled => _replicateStreamWriter != null;
        protected StreamWriter _replicateStreamWriter;
        protected MemoryStream _replicattMemoryStream;
        protected MemoryStream _captureMemoryStream;
        protected FileStream _replicateFileStream;

        #endregion

        #endregion

        #region construction & init

        public TextWriterWrapper()
        {
            _textWriter = new StreamWriter(new MemoryStream());
        }

        public TextWriterWrapper(TextWriter textWriter)
        {
            _textWriter = textWriter;
        }

        #endregion

        #region stream operations

        /// <summary>
        /// capture the output stream to a string
        /// </summary>
        /// <returns></returns>
        public void Capture()
        {
            lock (this) 
            {
                if (_redirectedTextWriter == null && _captureMemoryStream == null)
                {
                    _captureMemoryStream = new MemoryStream();
                    var sw = new StreamWriter(_captureMemoryStream);
                    _redirectedTextWriter = _textWriter;
                    _textWriter = sw;
                    IsRedirected = true;
                }
            }
        }

        public string StopCapture()
        {
            lock (this)
            {
                if (_captureMemoryStream != null)
                {
                    _textWriter.Flush();

                    _captureMemoryStream.Position = 0;
                    var str = Encoding.Default.GetString(_captureMemoryStream.ToArray());

                    _textWriter.Close();
                    _textWriter = _redirectedTextWriter;
                    _redirectedTextWriter = null;
                    _captureMemoryStream = null;
                    IsRedirected = false;
                    return str;
                }
                return null;
            }
        }

        public void Redirect(TextWriter sw)
        {
            if (sw != null)
            {
                _redirectedTextWriter = _textWriter;
                _textWriter = sw;
                IsRedirected = true;
            }
            else
            {
                _textWriter.Flush();
                _textWriter.Close();
                _textWriter = _redirectedTextWriter;
                _redirectedTextWriter = null;
                IsRedirected = false;
            }
        }

        public void Redirect(string filePath = null)
        {
            if (filePath != null)
            {
                _redirectedTextWriter = _textWriter;
                _textWriter = new StreamWriter(new FileStream(filePath, FileMode.Append, FileAccess.Write));
                IsRedirected = true;
            }
            else
            {
                _textWriter.Flush();
                _textWriter.Close();
                _textWriter = _redirectedTextWriter;
                _redirectedTextWriter = null;
                IsRedirected = false;
            }
        }

        /// <summary>
        /// echo Out to a memory stream
        /// </summary>
        /// <param name="autoFlush"></param>
        public void ReplicateToMem(
            bool autoFlush = false
            )
        {
            lock (this)
            {
                StopReplicate();
                ReplicateAutoFlush = autoFlush;
                _replicattMemoryStream = new MemoryStream();
                _replicateStreamWriter = new StreamWriter(_replicattMemoryStream);
            }
        }

        /// <summary>
        /// echo Out to a file
        /// </summary>
        /// <param name="filepath">file path where to echo Out</param>
        /// <param name="autoFlush">if set, flush Out before each echo</param>
        public void ReplicateToFile(
            string filepath,
            bool autoFlush = false)
        {
            lock (this)
            {
                if (!string.IsNullOrWhiteSpace(filepath) && _debugEchoFileStream == null)
                {
                    StopReplicate();
                    ReplicateAutoFlush = autoFlush;
                    _replicateFileStream = new FileStream(filepath, FileMode.Append, FileAccess.Write);
                    _replicateStreamWriter = new StreamWriter(_replicateFileStream);
                }
            }
        }

        public string StopReplicate()
        {
            if (_replicateFileStream != null)
            {
                _replicateStreamWriter.Flush();
                _replicateStreamWriter.Close();
                _replicateFileStream = null;
                _replicateStreamWriter = null;
                return null;
            }
            if (_replicattMemoryStream!=null)
            {
                _replicateStreamWriter.Flush();
                _replicattMemoryStream.Position = 0;
                var str = Encoding.Default.GetString(_replicattMemoryStream.ToArray());
                _replicateStreamWriter.Close();
                _replicattMemoryStream = null;
                _replicateStreamWriter = null;
                return str;
            }
            return null;
        }

        #endregion

        #region buffering operations

        public virtual void EnableBuffer()
        {
            lock (Lock)
            {
                if (IsBufferEnabled) return;
                if (_bufferWriter==null) _bufferWriter = new StreamWriter(_buffer);
                IsBufferEnabled = true;
            }
        }

        public virtual void CloseBuffer()
        {
            lock (Lock)
            {
                if (!IsBufferEnabled) return;
                _buffer.Seek(0,SeekOrigin.Begin);
                var txt = Encoding.Default.GetString( _buffer.ToArray() );
                _textWriter.Write(txt);
                _buffer.SetLength(0);
                IsBufferEnabled = false;
            }
        }

        #endregion

        #region stream write operations

        /// <summary>
        /// writes a string to the stream
        /// </summary>
        /// <param name="s">string to be written to the stream</param>
        public virtual void Write(string s)
        {
            if (IsEchoEnabled)
                _replicateStreamWriter.Write(s);
            if (IsBufferEnabled)
            {
                _bufferWriter.Write(s);
            }
            else
            {
                _textWriter.Write(s);
            }
        }

        /// <summary>
        /// writes a string to the stream
        /// </summary>
        /// <param name="s">string to be written to the stream</param>
        public virtual void WriteLine(string s)
        {
            if (IsBufferEnabled)
            {
                _bufferWriter.WriteLine(s);
            }
            else
            {
                _textWriter.WriteLine(s);
            }
        }

        public virtual void EchoDebug(
            string s, 
            bool lineBreak = false, 
            [CallerMemberName]string callerMemberName = "", 
            [CallerLineNumber]int callerLineNumber = -1)
        {
            if (!FileEchoDebugEnabled) return;
            if (FileEchoDebugDumpDebugInfo)
                _debugEchoStreamWriter?.Write($"l={s.Length},br={lineBreak} [{callerMemberName}:{callerLineNumber}] :");
            _debugEchoStreamWriter?.Write(s);
            if (lineBreak | FileEchoDebugAutoLineBreak) _debugEchoStreamWriter?.WriteLine(string.Empty);
            if (FileEchoDebugAutoFlush) _debugEchoStreamWriter?.Flush();
        }

        #endregion

        #region lock operations

        public void Locked(Action action)
        {
            lock (Lock)
            {
                action?.Invoke();
            }
        }

        #endregion
    }
}
