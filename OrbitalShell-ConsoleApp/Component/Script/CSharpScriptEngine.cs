using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

using OrbitalShell.Component.Console;

namespace OrbitalShell.Component.Script
{
    /// <summary>
    /// c# script engine
    /// </summary>
    public class CSharpScriptEngine
    {
        readonly Dictionary<string, Script<object>> _csscripts = new Dictionary<string, Script<object>>();
        
        public ScriptOptions DefaultScriptOptions;

        public CSharpScriptEngine() { _Init(); }

        public CSharpScriptEngine(ScriptOptions defaultScriptOptions)
        {
            DefaultScriptOptions = defaultScriptOptions;
            _Init();
        }

        private void _Init() {
            DefaultScriptOptions ??= ScriptOptions.Default;
            DefaultScriptOptions = DefaultScriptOptions
                .AddImports("System")
                .AddReferences(typeof(DotNetConsole).Assembly);
        }

        public object ExecCSharp(
            string csharpText,
            ConsoleTextWriterWrapper @out,
            ScriptOptions scriptOptions = null
            )
        {            
            try
            {
                scriptOptions ??= DefaultScriptOptions;
                var scriptKey = csharpText;
                if (!_csscripts.TryGetValue(scriptKey, out var script))
                {
                    script = CSharpScript.Create<object>(
                        csharpText,
                        scriptOptions
                        );
                    var cpl = script.Compile();
                    _csscripts[scriptKey] = script;
                }
                var res = script.RunAsync();
                return res.Result.ReturnValue;
            }
            catch (CompilationErrorException ex)
            {
                @out?.Errorln($"{csharpText}");
                @out?.Errorln(string.Join(Environment.NewLine, ex.Diagnostics));
                return null;
            }
        }
    }
}
