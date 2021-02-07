using System.Collections.Generic;

namespace OrbitalShell.Component.EchoDirective
{
    /// <summary>
    /// these map attribute a echo command delegate to an echo directive syntax
    /// </summary>
    public class CommandMap 
    {
        public Dictionary<string, (EchoDirectiveProcessor.SimpleCommandDelegate simpleCommand,EchoDirectiveProcessor.CommandDelegate command,object parameter)> Map = null;

        public CommandMap( Dictionary<string, (EchoDirectiveProcessor.SimpleCommandDelegate simpleCommand,EchoDirectiveProcessor.CommandDelegate command,object parameter)> map)
        {
            Map = map;
        }

    }
}