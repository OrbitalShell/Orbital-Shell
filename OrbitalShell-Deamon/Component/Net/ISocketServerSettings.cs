namespace OrbitalShell.Component.Net
{
    public interface ISocketServerSettings
    {
        int ByteBufferLength { get; set; }
        int ListeningPort { get; set; }
    }
}