using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbitalShell.Component.Net
{
    public class SocketServerSettings : ISocketServerSettings
    {
        public int ListeningPort { get; set; } = 8181;

        public int ByteBufferLength { get; set; } = 4096;
    }
}
