using System;
using SocketIo.SocketTypes;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SocketIo.Core.Tests
{   
	public class WebSocketTests
	{
        private const string IP = "127.0.0.1";

        //Tests cannot be ran with Run All as all but the first will fail due to the socket not being truly closed.

        [Fact]
		public async Task TestWebSocketAsync()
		{
			bool hit1 = false;
			bool hit2 = false;

			var randomPort = RandomPort.Get();

			var socket = await Io.CreateAsync(IP, randomPort, randomPort, SocketHandlerType.WebSocket);

			socket.On("connect", async () =>
			{
				hit1 = true;
				socket.On("test", (int package) =>
				{
					if (package == 5)
					{
						hit2 = true;
					}
				});

				await socket.EmitAsync("test", 5);

			});

			await socket.EmitAsync("connect");

			int timer = 0;
			int timeout = 5000;
			while ((!hit1 || !hit2)
				&& timer < timeout)
			{
				Thread.Sleep(100);
				timer += 100;
			}
			socket.Close();

			Assert.True(hit1 && hit2);

		}

		//WebSockets apparently can't connect to themselves...
		[Fact]
		public void TestWebSocket_Server()
		{
			bool hit1 = false;
			bool hit2 = false;

			ushort receivePort = 45544;

			var socket = Io.CreateListener("0.0.0.0", receivePort, SocketHandlerType.WebSocket);

			socket.On("connect", () =>
			{
				hit1 = true;
				socket.On("test", (int package) =>
				{
					if (package == 5)
					{
						hit2 = true;
					}
				});

			});

			int timer = 0;
			while (!hit1 || !hit2)
			{
				Thread.Sleep(100);
				timer += 100;
			}
			socket.Close();

			Assert.True(hit1 && hit2);

		}
		[Fact]
		public void TestWebSocket_Client()
		{
			ushort testPort = 45544;

			var socket = Io.CreateSender("192.168.0.107", testPort, SocketHandlerType.WebSocket);

			socket.Emit("connect");
			socket.Emit("test", 5);
		}



		[Fact]
		public async Task TestDualSocketWebSocketPAsync()
		{
			bool hit1 = false;
			bool hit2 = false;

			var randomPort = RandomPort.Get();

			var socketListener = Io.CreateListener(IP, randomPort, SocketHandlerType.WebSocket);

			var socketSender = await Io.CreateSenderAsync(IP, randomPort, SocketHandlerType.WebSocket);

			socketListener.On("connect", async () =>
			{
				hit1 = true;
				socketListener.On("test", (int package) =>
				{
					if (package == 5)
					{
						hit2 = true;
					}
				});

				await socketSender.EmitAsync("test", 5);

			});

			await socketSender.EmitAsync("connect");

			int timer = 0;
			int timeout = 5000;
			while ((!hit1 || !hit2)
				&& timer < timeout)
			{
				Thread.Sleep(100);
				timer += 100;
			}

			socketSender.Close();
			socketListener.Close();

			Assert.True(hit1 && hit2);

		}

		[Fact]
		public void TestDualSocketWebSocket()
		{
			bool hit1 = false;
			bool hit2 = false;

			var randomPort = RandomPort.Get();

			var socketListener = Io.CreateListener(IP, randomPort, SocketHandlerType.WebSocket);

			var socketSender = Io.CreateSender(IP, randomPort, SocketHandlerType.WebSocket);


			socketListener.On("connect", () =>
			{
				hit1 = true;
				socketListener.On("test", (int package) =>
				{
					if (package == 5)
					{
						hit2 = true;
					}
				});

				socketSender.Emit("test", 5);

			});

			socketSender.Emit("connect");

			int timer = 0;
			int timeout = 5000;
			while ((!hit1 || !hit2)
				&& timer < timeout)
			{
				Thread.Sleep(100);
				timer += 100;
			}

			socketSender.Close();
			socketListener.Close();

			Assert.True(hit1 && hit2);

		}
	}
}
