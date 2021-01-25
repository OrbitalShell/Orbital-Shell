using System;

namespace OrbitalShell.Component.CommandLine.Parsing
{
    public class AmbiguousParameterSpecificationException : Exception
    {
        public AmbiguousParameterSpecificationException( string message ) : base(message) { }
    }
}
