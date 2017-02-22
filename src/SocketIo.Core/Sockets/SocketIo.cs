using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using SocketIo.SocketTypes;

namespace SocketIo
{
	public class SocketIo
	{
		protected ConcurrentDictionary<string, BaseEmitter> Emitters { get; private set; } = new ConcurrentDictionary<string, BaseEmitter>();
		//protected ConcurrentBag<BaseEmitter> Emitters { get; private set; } = new ConcurrentBag<BaseEmitter>();
		
		protected ushort SendPort { get; set; }
		protected ushort ReceivePort { get; set; }
		protected int Timeout { get; set; }
		protected string ConnectedIP { get; set; }

		private BaseNetworkProtocol Handler { get; set; }

		private BaseEmitter CurrentEmitter;

		/// <summary>
		/// Socket that sends/receives from the endpoint
		/// </summary>
		/// <param name="ip"></param>
		/// <param name="port"></param>
		public SocketIo(ushort sendPort, ushort receivePort,int timeout, SocketHandlerType socketType)
		{
			SendPort = sendPort;
			ReceivePort = receivePort;
			Timeout = timeout;

			if (Handler == null)
			{
				switch (socketType)
				{
					case SocketHandlerType.Tcp:
						Handler = new TCPHandler(ReceivePort, SendPort, Timeout, this);
						break;
					case SocketHandlerType.Udp:
						Handler = new UDPHandler(ReceivePort, SendPort, Timeout, this);
						break;
				}
			}
			else
			{
				Handler.Setup(ReceivePort, SendPort, Timeout);
			}
		}

		public void Connect(string ip, string initialEmit = null)
		{
			ConnectedIP = ip;

			Handler.Listen(new IPEndPoint(IPAddress.Parse(ip), ReceivePort));

			if (!string.IsNullOrEmpty(initialEmit))
			{
				Emit(initialEmit, new IPEndPoint(IPAddress.Parse(ip), SendPort));
			}
		}



		public Emitter On(string @event, Action body)
		{
			Emitter e = new Emitter(@event, body);
			Emitters.TryAdd(e.Event,e);
			
			return e;
		}
		public Emitter<T> On<T>(string @event, Action<T> body)
		{
			Emitter<T> e = new Emitter<T>(@event, body);
			Emitters.TryAdd(e.Event, e);

			return e;
		}

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

		private void Emit(SocketMessage message)
		{

			if (CurrentEmitter != null)
			{
				Handler.Send(message, CurrentEmitter.CurrentSender);
			}
			else
			{
				Handler.Send(message, new IPEndPoint(IPAddress.Parse(ConnectedIP),SendPort));
			}
		}


		public void Close()
		{
			Emitters.Clear();
			Handler.Close();
		}


		internal void HandleMessage(byte[] message, IPAddress endpoint)
		{
			SocketMessage msg = message.Deserialize<SocketMessage>();
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
