using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OrbitalShell.Component.Net;

namespace OrbitalShell.Component.Deamon
{
    public class Settings
        : SocketServerSettings
    {
        public string WelcomeMessage { get; set; }

        public string EndMessage { get; set; }
    }
}
