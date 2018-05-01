//
// https://github.com/Beffyman/SocketIo.Core
//
using Newtonsoft.Json;
using SocketIo;
using SocketIo.SocketTypes;
using System;
using static System.Console;

namespace SocketIo.Core.Console
{
    public class SocketIOManager
    {
        private string _ip;
        private ushort _port;

        public SocketIOManager(string ip, ushort port)
        {
            _ip = ip;
            _port = port;
        }

        public void Run()
        {
            var socket = Io.Create(_ip, _port, _port, SocketHandlerType.Udp);

            socket.On("connect", () =>
            {
                WriteLine("Connected !");
                socket.On("login", (string user) =>
                {
                    WriteLine("Logged !");
                });


                var login = new { name = "juanlu", password = "123456" };

                var json = JsonConvert.SerializeObject(login);

                socket.Emit("login",  json);
            });

            socket.Emit("connect");


            WriteLine("Waiting for connection...");
            WriteLine("Pulse INTRO para finalizar...");
            ReadLine();

            socket.Close();

        }

    }
}
