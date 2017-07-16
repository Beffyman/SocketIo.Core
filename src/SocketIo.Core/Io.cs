using SocketIo.Core.Serializers;
using SocketIo.SocketTypes;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace SocketIo
{
	/// <summary>
	/// Static class used to connect sockets
	/// </summary>
	public static class Io
	{
		private const int DefaultTimeout = 3000;

		private static ushort GetOpenPort(ushort startPort = 2555)
		{
			ushort portStartIndex = startPort;
			ushort count = 99;
			IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
			IPEndPoint[] tcpEndPoints = properties.GetActiveTcpListeners();

			List<ushort> usedPorts = tcpEndPoints.Select(p => p.Port).Cast<ushort>().ToList();
			ushort unusedPort = 0;

			unusedPort = (ushort)Enumerable.Range(portStartIndex, count).Where(port => !usedPorts.Contains((ushort)port)).FirstOrDefault();
			return unusedPort;
		}

		/// <summary>
		/// Creates a socket that will send and receive messages
		/// </summary>
		/// <typeparam name="T">The type of serializer used.</typeparam>
		/// <param name="ip"></param>
		/// <param name="port"></param>
		/// <param name="type"></param>
		/// <param name="timeout"></param>
		/// <param name="initialEmit"></param>
		public static async Task<SocketIo> CreateAsync<T>(string ip, ushort port, SocketHandlerType type, int timeout = DefaultTimeout, string initialEmit = null)
			where T : ISerializer, new()
		{
			var receivePort = GetOpenPort(port);

			SocketIo socket = await SocketIo.CreateSenderAsync<T>(ip, port, timeout, type, initialEmit);
			socket.AddListener(receivePort);
			return socket;
		}
		/// <summary>
		/// Creates a socket that will send and receive messages
		/// </summary>
		/// <param name="ip"></param>
		/// <param name="port"></param>
		/// <param name="type"></param>
		/// <param name="timeout"></param>
		/// <param name="initialEmit"></param>
		public static async Task<SocketIo> CreateAsync(string ip, ushort port, SocketHandlerType type, int timeout = DefaultTimeout, string initialEmit = null)
		{
			var receivePort = GetOpenPort(port);

			SocketIo socket = await SocketIo.CreateSenderAsync<JsonSerializer>(ip, port, timeout, type, initialEmit);
			socket.AddListener(receivePort);
			return socket;
		}

		/// <summary>
		/// Creates a socket that will send and receive messages
		/// </summary>
		/// <param name="ip"></param>
		/// <param name="port"></param>
		/// <param name="type"></param>
		/// <param name="timeout"></param>
		/// <param name="initialEmit"></param>
		public static SocketIo Create(string ip, ushort port, SocketHandlerType type, int timeout = DefaultTimeout, string initialEmit = null)
		{

			var receivePort = GetOpenPort(port);

			SocketIo socket = SocketIo.CreateSender<JsonSerializer>(ip, port, timeout, type, initialEmit);
			socket.AddListener(receivePort);
			return socket;
		}

		/// <summary>
		/// Creates a socket that will send and receive messages
		/// </summary>
		/// <typeparam name="T">The type of serializer used.</typeparam>
		/// <param name="ip"></param>
		/// <param name="port"></param>
		/// <param name="type"></param>
		/// <param name="timeout"></param>
		/// <param name="initialEmit"></param>
		public static SocketIo Create<T>(string ip, ushort port, SocketHandlerType type, int timeout = DefaultTimeout, string initialEmit = null)
	where T : ISerializer, new()
		{
			var receivePort = GetOpenPort(port);

			SocketIo socket = SocketIo.CreateSender<T>(ip, port, timeout, type, initialEmit);
			socket.AddListener(receivePort);
			return socket;
		}

		/// <summary>
		/// Creates a socket that will send and receive messages
		/// </summary>
		/// <typeparam name="T">The type of serializer used.</typeparam>
		/// <param name="ip"></param>
		/// <param name="sendPort"></param>
		/// <param name="receivePort"></param>
		/// <param name="type"></param>
		/// <param name="timeout"></param>
		/// <param name="initialEmit"></param>
		/// <returns></returns>
		public static async Task<SocketIo> CreateAsync<T>(string ip, ushort sendPort, ushort receivePort, SocketHandlerType type, int timeout = DefaultTimeout, string initialEmit = null)
			where T : ISerializer, new()
		{
			SocketIo socket = await SocketIo.CreateSenderAsync<T>(ip, sendPort, timeout, type, initialEmit);
			socket.AddListener(receivePort);
			return socket;
		}

		/// <summary>
		/// Creates a socket that will send and receive messages
		/// </summary>
		/// <param name="ip"></param>
		/// <param name="sendPort"></param>
		/// <param name="receivePort"></param>
		/// <param name="type"></param>
		/// <param name="timeout"></param>
		/// <param name="initialEmit"></param>
		/// <returns></returns>
		public static async Task<SocketIo> CreateAsync(string ip, ushort sendPort, ushort receivePort, SocketHandlerType type, int timeout = DefaultTimeout, string initialEmit = null)
		{
			SocketIo socket = await SocketIo.CreateSenderAsync<JsonSerializer>(ip, sendPort, timeout, type, initialEmit);
			socket.AddListener(receivePort);
			return socket;
		}

		/// <summary>
		/// Creates a socket that will send and receive messages
		/// </summary>
		/// <typeparam name="T">The type of serializer used.</typeparam>
		/// <param name="ip"></param>
		/// <param name="sendPort"></param>
		/// <param name="receivePort"></param>
		/// <param name="type"></param>
		/// <param name="timeout"></param>
		/// <param name="initialEmit"></param>
		/// <returns></returns>
		public static SocketIo Create<T>(string ip, ushort sendPort, ushort receivePort, SocketHandlerType type, int timeout = DefaultTimeout, string initialEmit = null)
			where T : ISerializer, new()
		{
			SocketIo socket = SocketIo.CreateSender<T>(ip, sendPort, timeout, type, initialEmit);
			socket.AddListener(receivePort);
			return socket;
		}

		/// <summary>
		/// Creates a socket that will send and receive messages
		/// </summary>
		/// <param name="ip"></param>
		/// <param name="sendPort"></param>
		/// <param name="receivePort"></param>
		/// <param name="type"></param>
		/// <param name="timeout"></param>
		/// <param name="initialEmit"></param>
		/// <returns></returns>
		public static SocketIo Create(string ip, ushort sendPort, ushort receivePort, SocketHandlerType type, int timeout = DefaultTimeout, string initialEmit = null)
		{
			SocketIo socket = SocketIo.CreateSender<JsonSerializer>(ip, sendPort, timeout, type, initialEmit);
			socket.AddListener(receivePort);
			return socket;
		}

		/// <summary>
		/// Creates a socket that will send messages
		/// </summary>
		/// <param name="ip"></param>
		/// <param name="sendPort"></param>
		/// <param name="type"></param>
		/// <param name="timeout"></param>
		/// <param name="initialEmit"></param>
		/// <returns></returns>
		public static async Task<SocketIo> CreateSenderAsync<T>(string ip, ushort sendPort, SocketHandlerType type, int timeout = DefaultTimeout, string initialEmit = null)
			where T : ISerializer, new()
		{
			return await SocketIo.CreateSenderAsync<T>(ip, sendPort, timeout, type, initialEmit);
		}

		/// <summary>
		/// Creates a socket that will send messages
		/// </summary>
		/// <typeparam name="T">The type of serializer used.</typeparam>
		/// <param name="ip"></param>
		/// <param name="sendPort"></param>
		/// <param name="type"></param>
		/// <param name="timeout"></param>
		/// <param name="initialEmit"></param>
		/// <returns></returns>
		public static async Task<SocketIo> CreateSenderAsync(string ip, ushort sendPort, SocketHandlerType type, int timeout = DefaultTimeout, string initialEmit = null)
		{
			return await SocketIo.CreateSenderAsync<JsonSerializer>(ip, sendPort, timeout, type, initialEmit);
		}

		/// <summary>
		/// Creates a socket that will send messages
		/// </summary>
		/// <param name="ip"></param>
		/// <param name="sendPort"></param>
		/// <param name="type"></param>
		/// <param name="timeout"></param>
		/// <param name="initialEmit"></param>
		/// <returns></returns>
		public static SocketIo CreateSender<T>(string ip, ushort sendPort, SocketHandlerType type, int timeout = DefaultTimeout, string initialEmit = null)
			where T : ISerializer, new()
		{
			return SocketIo.CreateSender<T>(ip, sendPort, timeout, type, initialEmit);
		}

		/// <summary>
		/// Creates a socket that will send messages
		/// </summary>
		/// <typeparam name="T">The type of serializer used.</typeparam>
		/// <param name="ip"></param>
		/// <param name="sendPort"></param>
		/// <param name="type"></param>
		/// <param name="timeout"></param>
		/// <param name="initialEmit"></param>
		/// <returns></returns>
		public static SocketIo CreateSender(string ip, ushort sendPort, SocketHandlerType type, int timeout = DefaultTimeout, string initialEmit = null)
		{
			return SocketIo.CreateSender<JsonSerializer>(ip, sendPort, timeout, type, initialEmit);
		}

		/// <summary>
		/// Adds a listener to the socket
		/// </summary>
		/// <param name="socket"></param>
		/// <param name="receivePort"></param>
		/// <returns></returns>
		public static SocketIo AddListener(this SocketIo socket, ushort receivePort)
		{
			socket.Connect(receivePort);
			return socket;
		}

		/// <summary>
		/// Adds a sender to the socket
		/// </summary>
		/// <param name="socket"></param>
		/// <param name="sendPort"></param>
		/// <param name="initialEmit"></param>
		/// <returns></returns>
		public static async Task<SocketIo> AddSenderAsync(this SocketIo socket, ushort sendPort, string initialEmit = null)
		{
			await socket.AddSenderAsync(sendPort,initialEmit);
			return socket;
		}

		/// <summary>
		/// Adds a sender to the socket
		/// </summary>
		/// <param name="socket"></param>
		/// <param name="sendPort"></param>
		/// <param name="initialEmit"></param>
		/// <returns></returns>
		public static SocketIo AddSender(this SocketIo socket, ushort sendPort, string initialEmit = null)
		{
			socket.AddSender(sendPort,initialEmit);
			return socket;
		}


		/// <summary>
		/// Creates a socket that will receive messages
		/// </summary>
		/// <typeparam name="T">The type of serializer used.</typeparam>
		/// <param name="ip"></param>
		/// <param name="receivePort"></param>
		/// <param name="type"></param>
		/// <param name="timeout"></param>
		/// <returns></returns>
		public static SocketIo CreateListener<T>(string ip, ushort receivePort, SocketHandlerType type, int timeout = DefaultTimeout)
			where T : ISerializer, new()
		{
			return SocketIo.CreateListener<T>(ip, receivePort, timeout, type);
		}

		/// <summary>
		/// Creates a socket that will receive messages
		/// </summary>
		/// <param name="ip"></param>
		/// <param name="receivePort"></param>
		/// <param name="type"></param>
		/// <param name="timeout"></param>
		/// <returns></returns>
		public static SocketIo CreateListener(string ip, ushort receivePort, SocketHandlerType type, int timeout = DefaultTimeout)
		{
			return SocketIo.CreateListener<JsonSerializer>(ip, receivePort, timeout, type);
		}
	}
}
