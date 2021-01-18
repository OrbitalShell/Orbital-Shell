using System.Collections.Generic;

namespace DotNetConsoleAppToolkit.Component.EchoDirective
{
    public class CommandMap 
    {
        public Dictionary<string, (EchoDirectiveProcessor.SimpleCommandDelegate simpleCommand,EchoDirectiveProcessor.CommandDelegate command,object parameter)> Map = null;

        public CommandMap( Dictionary<string, (EchoDirectiveProcessor.SimpleCommandDelegate simpleCommand,EchoDirectiveProcessor.CommandDelegate command,object parameter)> map)
        {
            Map = map;
        }

    }
}