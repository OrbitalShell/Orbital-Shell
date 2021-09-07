using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using OrbitalShell.Component.Net;

using static OrbitalShell.Lib.Logger;

namespace OrbitalShell.Component.Deamon
{
    public class OrbitalShellDeamonClient
    {
        ISocketServerSettings _settings;

        public OrbitalShellDeamonClient(
            ISocketServerSettings settings
            )
        {
            _settings = settings;
        }

        public async Task<string> OpenRshAsync()
        {
            var res = await SendRequestAsync(OrbitalShellDeamonRequest.open_rsh);

            Log($"open rsh = {res}");

            return res;
        }

        public async Task<string> ListRshAsync()
        {
            var res = await SendRequestAsync(OrbitalShellDeamonRequest.list_rsh);

            Log($"list rsh:{Environment.NewLine}{res}");

            return res;
        }

        public async Task<string> SendRequestAsync(OrbitalShellDeamonRequest request)
            => await SendRequestAsync("" + request);

        public async Task<string> SendRequestAsync(string request)
        {
            using var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            await socket.ConnectAsync(new IPEndPoint(IPAddress.Loopback, _settings.ListeningPort));

            var ms = new MemoryStream(_settings.BufferCapacity);
            var ns = new NetworkStream(socket);
            var bytes = Encoding.Default.GetBytes(request);
            var readTask = ns.WriteAsync(bytes, 0, bytes.Length);
            var writeTask = ns.CopyToAsync(ms);

            await Task.WhenAll(readTask, writeTask);

            string result = Encoding.Default.GetString(ms.ToArray());
            return result;           
        }
    }
}
