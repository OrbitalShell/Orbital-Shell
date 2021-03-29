using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

using OrbitalShell.Component.Deamon;
using OrbitalShell.Component.Net;

using static OrbitalShell.Lib.Logger;

namespace OrbitalShell_Deamon
{
    public class App
    {
        public const string settingsFilename = "settings.json";

        static Thread _mainThread;
        Settings _settings;
        SocketServer _server;

        public static async Task Main(string[] args) {
            _mainThread = Thread.CurrentThread;
            await new App().Run();        
        }

        async Task Run()
        {
            _settings = JsonConvert.DeserializeObject<Settings>(
                File.ReadAllText(settingsFilename));

            Log(_settings.WelcomeMessage);

            _server = new SocketServer(_settings);

            System.Console.CancelKeyPress += (o, e) => Exit();
            await _server.StartListeningAsync();            
        }

        void Exit()
        {
            Log("terminating socket server...");
            _server.Exit();
            _mainThread.Interrupt();
            Log(_settings.EndMessage);
            Environment.Exit(0);
        }
        
    }
}
