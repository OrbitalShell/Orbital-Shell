using OrbitalShell.Component.CommandLine.CommandModel;
using System;
using System.Linq;
using System.Collections.Generic;
using static OrbitalShell.DotNetConsole;
using OrbitalShell.Component.CommandLine.Processor;

namespace OrbitalShell.Component.CommandLine.Parsing
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
            bool partialTokenMatch = false,
            StringComparison comparisonType = StringComparison.CurrentCulture
            )
        {
            var r = new List<CommandSyntax>();
            foreach (var ctokenkv in _syntaxes)
            {
                var ctoken = ctokenkv.Key;

                if ((partialTokenMatch && ctoken.StartsWith(token, comparisonType))
                    || ctoken.Equals(token, comparisonType))
                    r.AddRange(_syntaxes[ctoken]);

                foreach (var sn in ctokenkv.Value)
                {
                    var fullname = sn.CommandSpecification.FullName;
                    if ((partialTokenMatch && fullname.StartsWith(token, comparisonType))
                    || fullname.Equals(token, comparisonType))
                        r.AddRange(_syntaxes[ctoken]);
                }
            }
            return r;
        }
    }
}
