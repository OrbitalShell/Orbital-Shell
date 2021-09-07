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
