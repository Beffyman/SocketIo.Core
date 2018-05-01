using System;
using static System.Console;

namespace SocketIo.Core.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var manager = new SocketIOManager("127.0.0.1", 3000);
            manager.Run();

            WriteLine();
            WriteLine("Pulse INTRO para finalizar...");
            ReadLine();
        }
    }
}
