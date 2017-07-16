using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SocketIo.SocketTypes;
using System.Threading;
using System.Threading.Tasks;

namespace SocketIo.Core.Tests
{
	[TestClass]
	public class UdpTests
	{
		//Tests cannot be ran with Run All as all but the first will fail due to the socket not being truly closed.

		[TestMethod]
		public async Task TestUDPAsync()
		{
			bool hit1 = false;
			bool hit2 = false;

			var randomPort = RandomPort.Get();

			var socket = await Io.CreateAsync("127.0.0.1", randomPort, randomPort, SocketHandlerType.Udp);

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

			Assert.IsTrue(hit1 && hit2);

		}

		[TestMethod]
		public void TestUDP()
		{
			bool hit1 = false;
			bool hit2 = false;

			var randomPort = RandomPort.Get();

			var socket = Io.Create("127.0.0.1", randomPort, randomPort, SocketHandlerType.Udp);

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

			Assert.IsTrue(hit1 && hit2);

		}

		[TestMethod]
		public async Task TestDualSocketUDPAsync()
		{
			bool hit1 = false;
			bool hit2 = false;

			var randomPort = RandomPort.Get();

			var socketSender = await Io.CreateSenderAsync("127.0.0.1", randomPort, SocketHandlerType.Udp);

			var socketListener = Io.CreateListener("127.0.0.1", randomPort, SocketHandlerType.Udp);

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

			Assert.IsTrue(hit1 && hit2);

		}

		[TestMethod]
		public void TestDualSocketUDP()
		{
			bool hit1 = false;
			bool hit2 = false;

			var randomPort = RandomPort.Get();

			var socketSender = Io.CreateSender("127.0.0.1", randomPort, SocketHandlerType.Udp);

			var socketListener = Io.CreateListener("127.0.0.1", randomPort, SocketHandlerType.Udp);

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

			Assert.IsTrue(hit1 && hit2);

		}
	}
}
