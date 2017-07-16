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
			var socket = new SocketIo
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
						socket.Handler = new TCPHandler(ip,socket.SendPort, socket.ReceivePort, socket.Timeout, socket);
						break;
					case SocketHandlerType.Udp:
						socket.Handler = new UDPHandler(ip, socket.SendPort, socket.ReceivePort, socket.Timeout, socket);
						break;
					case SocketHandlerType.WebSocket:
						socket.Handler = new WebSocketHandler(ip, socket.SendPort, socket.ReceivePort, socket.Timeout, socket);
						break;
				}
			}
			else
			{
				socket.Handler.Setup(socket.ConnectedIP, socket.SendPort, socket.ReceivePort, socket.Timeout);
			}

			return socket;
		}

		internal static async Task<SocketIo> CreateSenderAsync<T>(string ip, ushort sendPort, int timeout, SocketHandlerType socketType, string initialEmit = null)
			where T : ISerializer, new()
		{
			var socket = SetupSocketHandler<T>(ip, sendPort,0, timeout, socketType);

			if (!string.IsNullOrEmpty(initialEmit))
			{
				await socket.EmitAsync(initialEmit, new IPEndPoint(IPAddress.Parse(socket.ConnectedIP), socket.SendPort));
			}

			return socket;
		}

		internal static SocketIo CreateSender<T>(string ip, ushort sendPort, int timeout, SocketHandlerType socketType, string initialEmit = null)
			where T : ISerializer, new()
		{
			var socket = SetupSocketHandler<T>(ip, sendPort,0, timeout, socketType);

			if (!string.IsNullOrEmpty(initialEmit))
			{
				socket.Emit(initialEmit, new IPEndPoint(IPAddress.Parse(socket.ConnectedIP), socket.SendPort));
			}

			return socket;
		}

		internal static SocketIo CreateListener<T>(string ip, ushort receivePort, int timeout, SocketHandlerType socketType)
			where T : ISerializer, new()
		{
			var socket = SetupSocketHandler<T>(ip, 0, receivePort, timeout, socketType);

			socket.Connect(receivePort);

			return socket;
		}

		internal void Connect(ushort receivePort)
		{
			ReceivePort = receivePort;
			if (ReceivePort == 0)
			{
				throw new Exception($"ReceivePort cannot be 0, to listen for messages setup the listener.");
			}

			Handler.Setup(ConnectedIP, 0,ReceivePort, Timeout);

			Task.Run(async () =>
			{
				await Handler.ListenAsync(new IPEndPoint(IPAddress.Parse(ConnectedIP), ReceivePort));
			});

		}

		internal async Task AddSenderAsync(string initialEmit = null)
		{
			if (SendPort == 0)
			{
				throw new Exception($"SendPort cannot be 0, to emit messages setup the sender.");
			}

			Handler.Setup(ConnectedIP, SendPort,0, Timeout);
			if (!string.IsNullOrEmpty(initialEmit))
			{
				await EmitAsync(initialEmit, new IPEndPoint(IPAddress.Parse(ConnectedIP), SendPort));
			}
		}

		internal void AddSender(string initialEmit = null)
		{
			if (SendPort == 0)
			{
				throw new Exception($"SendPort cannot be 0, to emit messages setup the sender.");
			}

			Handler.Setup(ConnectedIP, SendPort,0, Timeout);
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
			var e = new Emitter(@event, body);
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
			var e = new Emitter<T>(@event, body);
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
			var sm = new SocketMessage
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
			var sm = new SocketMessage
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
			var sm = new SocketMessage
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
			var sm = new SocketMessage
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
				throw new Exception($"Port cannot be 0, to emit messages setup the sender.");
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
			var sm = new SocketMessage
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
			var sm = new SocketMessage
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

		private void ResetValues<T>(string ip, ushort? sendPort, ushort? receivePort, int? timeout, SocketHandlerType? socketType)
			where T : ISerializer, new()
		{
			ConnectedIP = ip ?? ConnectedIP;
			SendPort = sendPort ?? SendPort;
			ReceivePort = receivePort ?? ReceivePort;
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
						Handler = new TCPHandler(ConnectedIP, SendPort, ReceivePort, Timeout, this);
						break;
					case SocketHandlerType.Udp:
						Handler = new UDPHandler(ConnectedIP, SendPort, ReceivePort, Timeout, this);
						break;
					case SocketHandlerType.WebSocket:
						Handler = new WebSocketHandler(ConnectedIP, SendPort, ReceivePort, Timeout, this);
						break;
				}
			}
		}

		#endregion

		internal void SetupSerializer<T>()
			where T : ISerializer, new()
		{
			Serializer = Activator.CreateInstance<T>();
		}

		private readonly object _lockEmitter = new object();

		internal void HandleMessage(byte[] message, IPAddress endpoint)
		{
			SocketMessage msg = Serializer.Deserialize<SocketMessage>(message);
			IPEndPoint ipPort = new IPEndPoint(endpoint, msg.CallbackPort);
			if (Emitters.ContainsKey(msg.Event))
			{
				lock (_lockEmitter)
				{
					CurrentEmitter = Emitters[msg.Event];
					CurrentEmitter.Invoke(msg, ipPort);
					CurrentEmitter = null;
				}
			}
		}

		internal void HandleMessage(string message, IPAddress endpoint)
		{
			var bytes = System.Text.Encoding.UTF8.GetBytes(message);
			SocketMessage msg = Serializer.Deserialize<SocketMessage>(bytes);
			IPEndPoint ipPort = new IPEndPoint(endpoint, msg.CallbackPort);
			if (Emitters.ContainsKey(msg.Event))
			{
				lock (_lockEmitter)
				{
					CurrentEmitter = Emitters[msg.Event];
					CurrentEmitter.Invoke(msg, ipPort);
					CurrentEmitter = null;
				}
			}
		}
	}

}
