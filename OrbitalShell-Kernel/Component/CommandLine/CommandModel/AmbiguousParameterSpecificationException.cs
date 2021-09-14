using System;

namespace OrbitalShell.Component.CommandLine.CommandModel
{
    public class AmbiguousParameterSpecificationException : Exception
    {
        public AmbiguousParameterSpecificationException(string message) : base(message) { }
    }
}
