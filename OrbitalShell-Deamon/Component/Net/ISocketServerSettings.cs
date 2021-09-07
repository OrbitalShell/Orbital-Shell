namespace OrbitalShell.Component.Net
{
    public interface ISocketServerSettings
    {
        int BufferCapacity { get; set; }
        int ListeningPort { get; set; }
    }
}