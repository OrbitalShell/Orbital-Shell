using System;
using System.Collections.Generic;

using OrbitalShell.Component.CommandLine.CommandModel;

namespace OrbitalShell.Component.CommandLine.Parsing
{
    public interface ISyntaxAnalyser
    {
        void Add(CommandSpecification comSpec);
        List<CommandSyntax> FindSyntaxesFromToken(string token, bool partialTokenMatch = false, StringComparison comparisonType = StringComparison.CurrentCulture);
        void Remove(CommandSpecification comSpec);
    }
}