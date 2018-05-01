using System;
using SocketIo.SocketTypes;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SocketIo.Core.Tests
{
    public class UdpTests
	{
        private const string IP = "127.0.0.1";

        //Tests cannot be ran with Run All as all but the first will fail due to the socket not being truly closed.

        [Fact]
        public async Task TestUDPAsync()
		{
			bool hit1 = false;
			bool hit2 = false;

			var randomPort = RandomPort.Get();

			var socket = await Io.CreateAsync(IP, randomPort, randomPort, SocketHandlerType.Udp);

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

		[Fact]
		public void TestUDP()
		{
			bool hit1 = false;
			bool hit2 = false;

			var randomPort = RandomPort.Get();

			var socket = Io.Create(IP, randomPort, randomPort, SocketHandlerType.Udp);

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

				socket.Emit("test", 5);

			});

			socket.Emit("connect");

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

		[Fact]
		public async Task TestDualSocketUDPAsync()
		{
			bool hit1 = false;
			bool hit2 = false;

			var randomPort = RandomPort.Get();

			var socketSender = await Io.CreateSenderAsync(IP, randomPort, SocketHandlerType.Udp);

			var socketListener = Io.CreateListener(IP, randomPort, SocketHandlerType.Udp);

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
		public void TestDualSocketUDP()
		{
			bool hit1 = false;
			bool hit2 = false;

			var randomPort = RandomPort.Get();

			var socketSender = Io.CreateSender(IP, randomPort, SocketHandlerType.Udp);

			var socketListener = Io.CreateListener(IP, randomPort, SocketHandlerType.Udp);

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
