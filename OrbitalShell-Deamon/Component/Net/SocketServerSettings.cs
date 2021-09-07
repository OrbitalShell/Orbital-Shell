namespace OrbitalShell.Component.Net
{
    public class SocketServerSettings : ISocketServerSettings
    {
        public int ListeningPort { get; set; } = 8181;

        public int BufferCapacity { get; set; } = 4096;
    }
}
