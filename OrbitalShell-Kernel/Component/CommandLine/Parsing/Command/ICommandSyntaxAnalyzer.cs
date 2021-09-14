using System;
using System.Collections.Generic;

using OrbitalShell.Component.CommandLine.CommandModel;

namespace OrbitalShell.Component.CommandLine.Parsing.Command
{
    public interface ICommandSyntaxAnalyzer
    {
        ICommandSyntaxAnalyzer Add(CommandSpecification comSpec);
        List<CommandSyntax> FindSyntaxesFromToken(string token, bool partialTokenMatch = false, StringComparison comparisonType = StringComparison.CurrentCulture);
        ICommandSyntaxAnalyzer Remove(CommandSpecification comSpec);
    }
}