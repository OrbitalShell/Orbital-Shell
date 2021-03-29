using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using static OrbitalShell.Lib.Logger;

namespace OrbitalShell.Component.Net
{
    public class SocketServer
    {
        ISocketServerSettings _settings;
        object _endRequestedLock = new();
        bool _endRequested = false;
        
        public void Exit()
        {
            lock (_endRequestedLock)
            {
                _endRequested = true;
            }
        }

        public SocketServer(ISocketServerSettings settings)
        {
            _settings = settings;
        }

        public async Task StartListeningAsync()
        {
            using var listenSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(new IPEndPoint(IPAddress.Loopback, _settings.ListeningPort));

            Log($"Listening on {listenSocket.LocalEndPoint}");

            listenSocket.Listen();

            bool end = false;
            while (!end)
            {
                lock (_endRequestedLock)
                {
                    end = _endRequested;
                }

                // Wait for a new connection to arrive
                var connection = await listenSocket.AcceptAsync();

                // We got a new connection spawn a task to so that we can echo the contents of the connection
                _ = Task.Run(async () =>
                {
                    var buffer = new byte[4096];
                    try
                    {
                        while (true)
                        {
                            int read = await connection.ReceiveAsync(buffer, SocketFlags.None);
                            if (read == 0)
                            {
                                break;
                            }
                            await connection.SendAsync(buffer[..read], SocketFlags.None);
                        }
                    }
                    finally
                    {
                        connection.Dispose();
                    }
                });
            }
        }
    }
}
