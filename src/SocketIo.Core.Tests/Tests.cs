using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SocketIo.SocketTypes;
using System.Threading;

namespace SocketIo.Core.Tests
{
	[TestClass]
	public class Tests
	{
		[TestMethod]
		public void TestUDP()
		{
			bool hit1 = false;
			bool hit2 = false;


			var socket = Io.Connect("127.0.0.1", 4333, 4333, SocketHandlerType.Udp);

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
		public void TestTCP()
		{
			bool hit1 = false;
			bool hit2 = false;


			var socket = Io.Connect("127.0.0.1", 4333, 4333, SocketHandlerType.Tcp);

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
	}
}
