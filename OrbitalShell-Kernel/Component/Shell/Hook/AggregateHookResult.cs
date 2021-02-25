using System.Collections.Generic;

namespace OrbitalShell.Component.Shell.Hook
{
    public class AggregateHookResult<ReturnType>
        where ReturnType: class
    {
        public readonly List<(object hook, ReturnType result)> Results
            = new List<(object hook, ReturnType result)>(); 
    }
}
