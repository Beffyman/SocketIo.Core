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
			await socket.AddListenerAsync(receivePort);
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
			await socket.AddListenerAsync(receivePort);
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
		/// Adds a listener to the socket
		/// </summary>
		/// <param name="socket"></param>
		/// <param name="receivePort"></param>
		/// <returns></returns>
		public static async Task<SocketIo> AddListenerAsync(this SocketIo socket, ushort receivePort)
		{
			await socket.ConnectAsync(receivePort);
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
		/// Creates a socket that will receive messages
		/// </summary>
		/// <typeparam name="T">The type of serializer used.</typeparam>
		/// <param name="ip"></param>
		/// <param name="receivePort"></param>
		/// <param name="type"></param>
		/// <param name="timeout"></param>
		/// <returns></returns>
		public static async Task<SocketIo> CreateListenerAsync<T>(string ip, ushort receivePort, SocketHandlerType type, int timeout = DefaultTimeout)
			where T : ISerializer, new()
		{
			SocketIo socket = await SocketIo.CreateListenerAsync<T>(ip, receivePort, timeout, type);

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
		public static async Task<SocketIo> CreateListenerAsync(string ip, ushort receivePort, SocketHandlerType type, int timeout = DefaultTimeout)
		{
			SocketIo socket = await SocketIo.CreateListenerAsync<JsonSerializer>(ip, receivePort, timeout, type);

			return socket;
		}
	}
}
