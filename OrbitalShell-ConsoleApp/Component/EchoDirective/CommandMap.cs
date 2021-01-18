using System.Collections.Generic;

namespace DotNetConsoleAppToolkit.Component.EchoDirective
{
    public class CommandMap 
    {
        public Dictionary<string, (EchoDirectiveProcessor.SimpleCommandDelegate simpleCommand,EchoDirectiveProcessor.CommandDelegate command,string parameter)> Map = null;

        public CommandMap( Dictionary<string, (EchoDirectiveProcessor.SimpleCommandDelegate simpleCommand,EchoDirectiveProcessor.CommandDelegate command,string parameter)> map)
        {
            Map = map;
        }

    }
}