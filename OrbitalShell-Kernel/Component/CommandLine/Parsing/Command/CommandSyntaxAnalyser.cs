using System;
using System.Collections.Generic;
using System.Linq;

using OrbitalShell.Component.CommandLine.CommandModel;

namespace OrbitalShell.Component.CommandLine.Parsing.Command
{
    public class CommandSyntaxAnalyser : ICommandSyntaxAnalyzer
    {
        readonly Dictionary<string, List<CommandSyntax>> _syntaxes
            = new();

        public ICommandSyntaxAnalyzer Add(CommandSpecification comSpec)
        {
            if (_syntaxes.TryGetValue(comSpec.Name, out var lst))
                lst.Add(new CommandSyntax(comSpec));
            else
                _syntaxes.Add(comSpec.Name, new List<CommandSyntax> { new CommandSyntax(comSpec) });
            return this;
        }

        public ICommandSyntaxAnalyzer Remove(CommandSpecification comSpec)
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
            return this;
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
