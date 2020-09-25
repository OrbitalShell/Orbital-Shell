using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DotNetConsoleAppToolkit.Component.CommandLine.Parsing
{
    public class MatchingParameters
    {
        readonly Dictionary<string, IMatchingParameter> _parameters
            = new Dictionary<string, IMatchingParameter>();

        public ReadOnlyDictionary<string, IMatchingParameter> Parameters =>
            new ReadOnlyDictionary<string, IMatchingParameter>(_parameters);

        public bool Contains(string parameterName) => _parameters.ContainsKey(parameterName);

        public bool TryGet(string parameterName,out IMatchingParameter matchingParameter)
        {
            return _parameters.TryGetValue(parameterName, out matchingParameter);
        }

        public void Add(string parameterName, IMatchingParameter matchingParameter)
        {
            _parameters.Add(parameterName, matchingParameter);
        }
    }
}
