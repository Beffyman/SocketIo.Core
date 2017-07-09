using SocketIo.Core.Serializers;
using SocketIo.SocketTypes;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading.Tasks;

namespace SocketIo
{
	/// <summary>
	/// Abstraction of a UDP/TCP client that writes like Socket.IO
	/// </summary>
	public sealed class SocketIo
	{
		private ConcurrentDictionary<string, BaseEmitter> Emitters { get; set; } = new ConcurrentDictionary<string, BaseEmitter>();

		internal ISerializer Serializer;

		private ushort SendPort { get; set; }
		private ushort ReceivePort { get; set; }
		private int Timeout { get; set; }
		private string ConnectedIP { get; set; }

		private BaseNetworkProtocol Handler { get; set; }

		private BaseEmitter CurrentEmitter;

		private SocketIo() { }


		private static SocketIo SetupSocketHandler<T>(string ip, ushort sendPort, ushort receivePort, int timeout, SocketHandlerType socketType)
			where T : ISerializer, new()
		{
			SocketIo socket = new SocketIo
			{
				ConnectedIP = ip,
				SendPort = sendPort,
				ReceivePort = receivePort,
				Timeout = timeout,
			};

			socket.SetupSerializer<T>();

			if (socket.Handler == null)
			{
				switch (socketType)
				{
					case SocketHandlerType.Tcp:
						socket.Handler = new TCPHandler(socket.ReceivePort, socket.SendPort, socket.Timeout, socket);
						break;
					case SocketHandlerType.Udp:
						socket.Handler = new UDPHandler(socket.ReceivePort, socket.SendPort, socket.Timeout, socket);
						break;
					case SocketHandlerType.WebSocket:
						socket.Handler = new WebSocketHandler(socket.ReceivePort, socket.SendPort, socket.Timeout, socket);
						break;
				}
			}
			else
			{
				socket.Handler.Setup(socket.ReceivePort, socket.SendPort, socket.Timeout);
			}

			return socket;
		}

		internal static async Task<SocketIo> CreateSenderAsync<T>(string ip, ushort sendPort, int timeout, SocketHandlerType socketType, string initialEmit = null)
			where T : ISerializer, new()
		{
			var socket = SetupSocketHandler<T>(ip, sendPort, 0, timeout, socketType);

			if (!string.IsNullOrEmpty(initialEmit))
			{
				await socket.EmitAsync(initialEmit, new IPEndPoint(IPAddress.Parse(socket.ConnectedIP), socket.SendPort));
			}

			return socket;
		}

		internal static SocketIo CreateSender<T>(string ip, ushort sendPort, int timeout, SocketHandlerType socketType, string initialEmit = null)
			where T : ISerializer, new()
		{
			var socket = SetupSocketHandler<T>(ip, sendPort, 0, timeout, socketType);

			if (!string.IsNullOrEmpty(initialEmit))
			{
				socket.Emit(initialEmit, new IPEndPoint(IPAddress.Parse(socket.ConnectedIP), socket.SendPort));
			}

			return socket;
		}

		internal static async Task<SocketIo> CreateListenerAsync<T>(string ip, ushort recievePort, int timeout, SocketHandlerType socketType)
			where T : ISerializer, new()
		{
			var socket = SetupSocketHandler<T>(ip, 0, recievePort, timeout, socketType);

			await socket.ConnectAsync(recievePort);

			return socket;
		}

		internal static SocketIo CreateListener<T>(string ip, ushort recievePort, int timeout, SocketHandlerType socketType)
			where T : ISerializer, new()
		{
			var socket = SetupSocketHandler<T>(ip, 0, recievePort, timeout, socketType);

			socket.Connect(recievePort);

			return socket;
		}

		internal async Task ConnectAsync(ushort receivePort)
		{
			if (receivePort == 0)
			{
				throw new Exception($"ReceivePort cannot be 0, to listen for messages setup the listener.");
			}

			if (ReceivePort != 0)
			{
				throw new Exception($"You cannot add another sender to this socket. Please restart the socket to change settings.");
			}

			ReceivePort = receivePort;
			Handler.Setup(ReceivePort, SendPort, Timeout);
			await Handler.ListenAsync(new IPEndPoint(IPAddress.Parse(ConnectedIP), ReceivePort));
		}

		internal void Connect(ushort receivePort)
		{
			if (receivePort == 0)
			{
				throw new Exception($"ReceivePort cannot be 0, to listen for messages setup the listener.");
			}

			if (ReceivePort != 0)
			{
				throw new Exception($"You cannot add another sender to this socket. Please restart the socket to change settings.");
			}

			ReceivePort = receivePort;
			Handler.Setup(ReceivePort, SendPort, Timeout);
			Handler.Listen(new IPEndPoint(IPAddress.Parse(ConnectedIP), ReceivePort));
		}

		internal async Task AddSenderAsync(ushort sendPort, string initialEmit = null)
		{
			if (sendPort == 0)
			{
				throw new Exception($"SendPort cannot be 0, to emit messages setup the sender.");
			}

			if (SendPort != 0)
			{
				throw new Exception($"You cannot add another listener to this socket. Please restart the socket to change settings.");
			}

			SendPort = sendPort;
			Handler.Setup(ReceivePort, SendPort, Timeout);
			if (!string.IsNullOrEmpty(initialEmit))
			{
				await EmitAsync(initialEmit, new IPEndPoint(IPAddress.Parse(ConnectedIP), SendPort));
			}
		}

		internal void AddSender(ushort sendPort, string initialEmit = null)
		{
			if (sendPort == 0)
			{
				throw new Exception($"SendPort cannot be 0, to emit messages setup the sender.");
			}

			if (SendPort != 0)
			{
				throw new Exception($"You cannot add another listener to this socket. Please restart the socket to change settings.");
			}

			SendPort = sendPort;
			Handler.Setup(ReceivePort, SendPort, Timeout);
			if (!string.IsNullOrEmpty(initialEmit))
			{
				Emit(initialEmit, new IPEndPoint(IPAddress.Parse(ConnectedIP), SendPort));
			}
		}

		/// <summary>
		/// Register an event with an action
		/// </summary>
		/// <param name="event"></param>
		/// <param name="body"></param>
		/// <returns></returns>
		public Emitter On(string @event, Action body)
		{
			Emitter e = new Emitter(@event, body);
			Emitters.TryAdd(e.Event, e);

			return e;
		}

		/// <summary>
		/// Register an event with an action with a paramter
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="event"></param>
		/// <param name="body"></param>
		/// <returns></returns>
		public Emitter<T> On<T>(string @event, Action<T> body)
		{
			Emitter<T> e = new Emitter<T>(@event, body);
			Emitters.TryAdd(e.Event, e);

			return e;
		}


		#region Emit

		#region Async

		/// <summary>
		/// Sends a empty message to the connected Listener
		/// </summary>
		/// <param name="event"></param>
		public async Task EmitAsync(string @event)
		{
			SocketMessage sm = new SocketMessage
			{
				Content = null,
				Event = @event,
				CallbackPort = ReceivePort
			};

			await EmitAsync(sm);
		}
		/// <summary>
		/// Sends a message to the connected Listener
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="event"></param>
		/// <param name="message"></param>
		public async Task EmitAsync<T>(string @event, T message)
		{
			SocketMessage sm = new SocketMessage
			{
				Content = message,
				Event = @event,
				CallbackPort = ReceivePort
			};

			await EmitAsync(sm);
		}

		/// <summary>
		/// Sends a empty message to the specified enpoint
		/// </summary>
		/// <param name="event"></param>
		/// <param name="endpoint"></param>
		public async Task EmitAsync(string @event, IPEndPoint endpoint)
		{
			SocketMessage sm = new SocketMessage
			{
				Content = null,
				Event = @event,
				CallbackPort = ReceivePort
			};

			await Handler.SendAsync(sm, endpoint);
		}

		/// <summary>
		/// Sends a message to the specified enpoint
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="event"></param>
		/// <param name="message"></param>
		/// <param name="endpoint"></param>
		public async Task EmitAsync<T>(string @event, T message, IPEndPoint endpoint)
		{
			SocketMessage sm = new SocketMessage
			{
				Content = message,
				Event = @event,
				CallbackPort = ReceivePort
			};

			await Handler.SendAsync(sm, endpoint);
		}

		private async Task EmitAsync(SocketMessage message)
		{
			if (SendPort == 0)
			{
				throw new Exception($"SendPort cannot be 0, to emit messages setup the sender.");
			}

			if (CurrentEmitter != null)
			{
				await Handler.SendAsync(message, CurrentEmitter.CurrentSender);
			}
			else
			{
				await Handler.SendAsync(message, new IPEndPoint(IPAddress.Parse(ConnectedIP), SendPort));
			}
		}

		#endregion

		#region Sync

		/// <summary>
		/// Sends a empty message to the connected Listener
		/// </summary>
		/// <param name="event"></param>
		public void Emit(string @event)
		{
			SocketMessage sm = new SocketMessage
			{
				Content = null,
				Event = @event,
				CallbackPort = ReceivePort
			};

			Emit(sm);
		}
		/// <summary>
		/// Sends a message to the connected Listener
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="event"></param>
		/// <param name="message"></param>
		public void Emit<T>(string @event, T message)
		{
			SocketMessage sm = new SocketMessage
			{
				Content = message,
				Event = @event,
				CallbackPort = ReceivePort
			};

			Emit(sm);
		}

		/// <summary>
		/// Sends a empty message to the specified enpoint
		/// </summary>
		/// <param name="event"></param>
		/// <param name="endpoint"></param>
		public void Emit(string @event, IPEndPoint endpoint)
		{
			SocketMessage sm = new SocketMessage
			{
				Content = null,
				Event = @event,
				CallbackPort = ReceivePort
			};

			Handler.Send(sm, endpoint);
		}

		/// <summary>
		/// Sends a message to the specified enpoint
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="event"></param>
		/// <param name="message"></param>
		/// <param name="endpoint"></param>
		public void Emit<T>(string @event, T message, IPEndPoint endpoint)
		{
			SocketMessage sm = new SocketMessage
			{
				Content = message,
				Event = @event,
				CallbackPort = ReceivePort
			};

			Handler.Send(sm, endpoint);
		}

		private void Emit(SocketMessage message)
		{
			if (SendPort == 0)
			{
				throw new Exception($"SendPort cannot be 0, to emit messages setup the sender.");
			}

			if (CurrentEmitter != null)
			{
				Handler.Send(message, CurrentEmitter.CurrentSender);
			}
			else
			{
				Handler.Send(message, new IPEndPoint(IPAddress.Parse(ConnectedIP), SendPort));
			}
		}

		#endregion

		#endregion


		/// <summary>
		/// Closes the socket and all emitters.
		/// </summary>
		public void Close()
		{
			Emitters.Clear();
			Handler.Close();
		}



		#region Reset

		private void ResetValues<T>(string ip, ushort? sendPort, ushort? recievePort, int? timeout, SocketHandlerType? socketType)
			where T : ISerializer, new()
		{
			ConnectedIP = ip ?? ConnectedIP;
			SendPort = sendPort ?? SendPort;
			ReceivePort = recievePort ?? ReceivePort;
			Timeout = timeout ?? Timeout;

			SetupSerializer<T>();

			if (socketType != null)
			{
				if (Handler != null)
				{
					try
					{
						Handler.Close();
					}
					catch { }
				}

				switch (socketType)
				{
					case SocketHandlerType.Tcp:
						Handler = new TCPHandler(ReceivePort, SendPort, Timeout, this);
						break;
					case SocketHandlerType.Udp:
						Handler = new UDPHandler(ReceivePort, SendPort, Timeout, this);
						break;
					case SocketHandlerType.WebSocket:
						Handler = new WebSocketHandler(ReceivePort, SendPort, Timeout, this);
						break;
				}
			}
		}

		#region Sync

		internal void Reset(string ip, ushort? sendPort, ushort? receivePort, int? timeout, SocketHandlerType? socketType)
		{
			Reset<JsonSerializer>(ip, sendPort, receivePort, timeout, socketType);
		}

		internal void Reset<T>(string ip, ushort? sendPort, ushort? receivePort, int? timeout, SocketHandlerType? socketType)
			where T : ISerializer, new()
		{
			ResetValues<T>(ip, sendPort, receivePort, timeout, socketType);

			if (ReceivePort != 0)
			{
				Handler.Listen(new IPEndPoint(IPAddress.Parse(ConnectedIP), ReceivePort));
			}
		}

		#endregion

		#region Async

		internal async Task ResetAsync(string ip, ushort? sendPort, ushort? receivePort, int? timeout, SocketHandlerType? socketType)
		{
			await ResetAsync<JsonSerializer>(ip, sendPort, receivePort, timeout, socketType);
		}

		internal async Task ResetAsync<T>(string ip, ushort? sendPort, ushort? receivePort, int? timeout, SocketHandlerType? socketType)
			where T : ISerializer, new()
		{
			ResetValues<T>(ip, sendPort, receivePort, timeout, socketType);

			if (ReceivePort != 0)
			{
				await Handler.ListenAsync(new IPEndPoint(IPAddress.Parse(ConnectedIP), ReceivePort));
			}
		}
		#endregion

		#endregion

		internal void SetupSerializer<T>()
			where T : ISerializer, new()
		{
			Serializer = Activator.CreateInstance<T>();
		}

		internal void HandleMessage(byte[] message, IPAddress endpoint)
		{
			SocketMessage msg = Serializer.Deserialize<SocketMessage>(message);
			IPEndPoint ipPort = new IPEndPoint(endpoint, msg.CallbackPort);
			if (Emitters.ContainsKey(msg.Event))
			{
				CurrentEmitter = Emitters[msg.Event];
				CurrentEmitter.Invoke(msg, ipPort);
				CurrentEmitter = null;
			}
		}
	}

}
