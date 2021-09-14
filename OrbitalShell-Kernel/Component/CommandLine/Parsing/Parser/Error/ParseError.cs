using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using OrbitalShell.Component.CommandLine.CommandModel;

namespace OrbitalShell.Component.CommandLine.Parsing.Parser.Error
{
    public class ParseError
    {
        public string Description { get; protected set; }
        public readonly CommandSpecification CommandSpecification;
        public readonly ReadOnlyCollection<CommandParameterSpecification> CommandParameterSpecifications;
        public readonly int Position;
        public readonly int Index;

        public ParseError(
            string description,
            int position,
            int index,
            CommandSpecification commandSpecification
        )
        {
            Description = description;
            Position = position;
            Index = index;
            CommandSpecification = commandSpecification;
            CommandParameterSpecifications = new ReadOnlyCollection<CommandParameterSpecification>(
                new List<CommandParameterSpecification>());
        }

        public ParseError(
            string description,
            int position,
            int index,
            CommandSpecification commandSpecification,
            IList<CommandParameterSpecification> commandParameterSpecifications
        )
        {
            Description = description;
            Position = position;
            Index = index;
            CommandSpecification = commandSpecification;
            CommandParameterSpecifications = new ReadOnlyCollection<CommandParameterSpecification>(commandParameterSpecifications);
        }

        public ParseError(
            string description,
            int position,
            int index,
            CommandSpecification commandSpecification,
            CommandParameterSpecification commandParameterSpecification
        )
        {
            Description = description;
            Position = position;
            Index = index;
            CommandSpecification = commandSpecification;
            CommandParameterSpecifications = new ReadOnlyCollection<CommandParameterSpecification>(
                new List<CommandParameterSpecification> { commandParameterSpecification });
        }

        public override string ToString() => Description;

        public void Merge(ParseError prsError)
        {
            if (!Description.Contains(prsError.Description))
                Description += Environment.NewLine + prsError.Description;
        }
    }
}
