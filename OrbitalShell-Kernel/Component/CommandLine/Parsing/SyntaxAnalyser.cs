using DotNetConsoleAppToolkit.Component.CommandLine.CommandModel;
using System;
using System.Linq;
using System.Collections.Generic;
using static DotNetConsoleAppToolkit.DotNetConsole;
using DotNetConsoleAppToolkit.Component.CommandLine.Processor;

namespace DotNetConsoleAppToolkit.Component.CommandLine.Parsing
{
    public class SyntaxAnalyser
    {
        readonly Dictionary<string, List<CommandSyntax>> _syntaxes
            = new Dictionary<string, List<CommandSyntax>>();

        public void Add(CommandSpecification comSpec)
        {
            if (_syntaxes.TryGetValue(comSpec.Name, out var lst))
                lst.Add(new CommandSyntax(comSpec));
            else
                _syntaxes.Add(comSpec.Name, new List<CommandSyntax> { new CommandSyntax(comSpec) });
        }

        public void Remove(CommandSpecification comSpec)
        {
            if (_syntaxes.TryGetValue(comSpec.Name, out var lst))
            {
                var sytx = lst.Where(x => x.CommandSpecification == comSpec).FirstOrDefault();
                if (sytx != null)
                {
                    lst.Remove(sytx);
                    if (lst.Count == 0)
                        _syntaxes.Remove(comSpec.Name);
                }
            }
        }

        public List<CommandSyntax> FindSyntaxesFromToken(
            string token,
            bool partialTokenMatch=false,
            StringComparison comparisonType = StringComparison.CurrentCulture
            )
        {
            var r = new List<CommandSyntax>();
            foreach (var ctoken in _syntaxes.Keys)
                if ((partialTokenMatch && ctoken.StartsWith(token, comparisonType)) || ctoken.Equals(token, comparisonType))
                    r.AddRange(_syntaxes[ctoken]);
            return r;
        }
    }
}
