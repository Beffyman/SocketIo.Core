using SocketIo.Core.Serializers;
using SocketIo.SocketTypes;
using System.Threading.Tasks;

namespace SocketIo
{
	/// <summary>
	/// Static class used to connect sockets
	/// </summary>
	public static class Io
	{
		private const int DefaultTimeout = 3000;

		/// <summary>
		/// Restarts the socket with the parameters provided, if null, it defaults to what is already set. If the socket has a listener setup, it will restart as well.
		/// </summary>
		/// <typeparam name="T">The type of serializer used.</typeparam>
		/// <param name="socket"></param>
		/// <param name="ip"></param>
		/// <param name="sendPort"></param>
		/// <param name="receivePort"></param>
		/// <param name="type"></param>
		/// <param name="timeout"></param>
		/// <returns></returns>
		public static async Task<SocketIo> RestartAsync<T>(this SocketIo socket, string ip = null, ushort? sendPort = null, ushort? receivePort = null, SocketHandlerType? type = null, int timeout = DefaultTimeout)
			where T : ISerializer, new()
		{
			socket.Close();
			await socket.ResetAsync(ip, sendPort, receivePort, timeout, type);
			return socket;
		}

		/// <summary>
		/// Restarts the socket with the parameters provided, if null, it defaults to what is already set. If the socket has a listener setup, it will restart as well.
		/// </summary>
		/// <param name="socket"></param>
		/// <param name="ip"></param>
		/// <param name="sendPort"></param>
		/// <param name="receivePort"></param>
		/// <param name="type"></param>
		/// <param name="timeout"></param>
		/// <returns></returns>
		public static async Task<SocketIo> RestartAsync(this SocketIo socket, string ip = null, ushort? sendPort = null, ushort? receivePort = null, SocketHandlerType? type = null, int timeout = DefaultTimeout)
		{
			socket.Close();
			await socket.ResetAsync(ip, sendPort, receivePort, timeout, type);
			return socket;
		}

		/// <summary>
		/// Restarts the socket with the parameters provided, if null, it defaults to what is already set. If the socket has a listener setup, it will restart as well.
		/// </summary>
		/// <typeparam name="T">The type of serializer used.</typeparam>
		/// <param name="socket"></param>
		/// <param name="ip"></param>
		/// <param name="sendPort"></param>
		/// <param name="receivePort"></param>
		/// <param name="type"></param>
		/// <param name="timeout"></param>
		/// <returns></returns>
		public static SocketIo Restart<T>(this SocketIo socket, string ip = null, ushort? sendPort = null, ushort? receivePort = null, SocketHandlerType? type = null, int timeout = DefaultTimeout)
			where T : ISerializer, new()
		{
			socket.Close();
			socket.Reset(ip, sendPort, receivePort, timeout, type);
			return socket;
		}

		/// <summary>
		/// Restarts the socket with the parameters provided, if null, it defaults to what is already set. If the socket has a listener setup, it will restart as well.
		/// </summary>
		/// <param name="socket"></param>
		/// <param name="ip"></param>
		/// <param name="sendPort"></param>
		/// <param name="receivePort"></param>
		/// <param name="type"></param>
		/// <param name="timeout"></param>
		/// <returns></returns>
		public static SocketIo Restart(this SocketIo socket, string ip = null, ushort? sendPort = null, ushort? receivePort = null, SocketHandlerType? type = null, int timeout = DefaultTimeout)
		{
			socket.Close();
			socket.Reset(ip, sendPort, receivePort, timeout, type);
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
			SocketIo socket = await SocketIo.CreateSenderAsync<T>(ip, sendPort, timeout, type, initialEmit);

			return socket;
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
			SocketIo socket = await SocketIo.CreateSenderAsync<JsonSerializer>(ip, sendPort, timeout, type, initialEmit);

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
		public static SocketIo CreateSender<T>(string ip, ushort sendPort, SocketHandlerType type, int timeout = DefaultTimeout, string initialEmit = null)
			where T : ISerializer, new()
		{
			SocketIo socket = SocketIo.CreateSender<T>(ip, sendPort, timeout, type, initialEmit);

			return socket;
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
			SocketIo socket = SocketIo.CreateSender<JsonSerializer>(ip, sendPort, timeout, type, initialEmit);

			return socket;
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
			await socket.AddSenderAsync(sendPort, initialEmit);
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
			socket.AddSender(sendPort, initialEmit);
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
			SocketIo socket = SocketIo.CreateListener<T>(ip, receivePort, timeout, type);

			return socket;
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
			SocketIo socket = SocketIo.CreateListener<JsonSerializer>(ip, receivePort, timeout, type);

			return socket;
		}
	}
}
