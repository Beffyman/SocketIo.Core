using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketIo.SocketTypes
{
    //https://gist.githubusercontent.com/xamlmonkey/4737291/raw/714f4feda3a4081b3ce761ee9f1535624d68d582/WebSocketWrapper.cs
    internal sealed class WebSocketWrapper
	{
		private const int ReceiveChunkSize = 1024;
		private const int SendChunkSize = 1024;

		private ClientWebSocket _ws;
		private readonly Uri _uri;
		private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
		private CancellationToken _cancellationToken;

		private Action<WebSocketWrapper> _onConnected;
		private Action<string, WebSocketWrapper> _onMessage;
		private Action<WebSocketWrapper> _onDisconnected;

		private WebSocketWrapper(string uri)
		{
			_ws = new ClientWebSocket();
			_ws.Options.KeepAliveInterval = TimeSpan.FromSeconds(20);
			_uri = new Uri(uri);
			_cancellationToken = _cancellationTokenSource.Token;
		}

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="uri">The URI of the WebSocket server.</param>
		/// <returns></returns>
		public static WebSocketWrapper Create(string uri)
		{
			return new WebSocketWrapper(uri);
		}

		/// <summary>
		/// Connects to the WebSocket server.
		/// </summary>
		/// <returns></returns>
		public WebSocketWrapper Connect()
		{
			if (_ws == null)
			{
				_ws = new ClientWebSocket();
				_ws.Options.KeepAliveInterval = TimeSpan.FromSeconds(20);
				_cancellationTokenSource = new CancellationTokenSource();
				_cancellationToken = _cancellationTokenSource.Token;
			}

			ConnectAsync().GetAwaiter().GetResult();
			return this;
		}

		/// <summary>
		/// Set the Action to call when the connection has been established.
		/// </summary>
		/// <param name="onConnect">The Action to call.</param>
		/// <returns></returns>
		public WebSocketWrapper OnConnect(Action<WebSocketWrapper> onConnect)
		{
			_onConnected = onConnect;
			return this;
		}

		/// <summary>
		/// Set the Action to call when the connection has been terminated.
		/// </summary>
		/// <param name="onDisconnect">The Action to call</param>
		/// <returns></returns>
		public WebSocketWrapper OnDisconnect(Action<WebSocketWrapper> onDisconnect)
		{
			_onDisconnected = onDisconnect;
			return this;
		}

		/// <summary>
		/// Set the Action to call when a messages has been received.
		/// </summary>
		/// <param name="onMessage">The Action to call.</param>
		/// <returns></returns>
		public WebSocketWrapper OnMessage(Action<string, WebSocketWrapper> onMessage)
		{
			_onMessage = onMessage;
			return this;
		}

		/// <summary>
		/// Send a message to the WebSocket server.
		/// </summary>
		/// <param name="message">The message to send</param>
		public void SendMessage(byte[] message)
		{
			SendMessageAsync(message);
		}

		private async void SendMessageAsync(byte[] messageBuffer)
		{
			if (_ws.State != WebSocketState.Open)
			{
				throw new Exception("Connection is not open.");
			}

			var messagesCount = (int)Math.Ceiling((double)messageBuffer.Length / SendChunkSize);

			for (var i = 0; i < messagesCount; i++)
			{
				var offset = (SendChunkSize * i);
				var count = SendChunkSize;
				var lastMessage = ((i + 1) == messagesCount);

				if ((count * (i + 1)) > messageBuffer.Length)
				{
					count = messageBuffer.Length - offset;
				}

				await _ws.SendAsync(new ArraySegment<byte>(messageBuffer, offset, count), WebSocketMessageType.Text, lastMessage, _cancellationToken);
			}
		}

		private async Task ConnectAsync()
		{
			await _ws.ConnectAsync(_uri, _cancellationToken);
			CallOnConnected();
			StartListen();
		}

		private async void StartListen()
		{
			var buffer = new byte[ReceiveChunkSize];

			try
			{
				while (_ws.State == WebSocketState.Open)
				{
					var stringResult = new StringBuilder();


					WebSocketReceiveResult result;
					do
					{
						result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationToken);

						if (result.MessageType == WebSocketMessageType.Close)
						{
							await
								_ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
							CallOnDisconnected();
						}
						else
						{
							var str = Encoding.UTF8.GetString(buffer, 0, result.Count);
							stringResult.Append(str);
						}

					} while (!result.EndOfMessage);

					CallOnMessage(stringResult);

				}
			}
			catch (Exception)
			{
				CallOnDisconnected();
			}
			finally
			{
				_ws.Dispose();
			}
		}

		private void CallOnMessage(StringBuilder stringResult)
		{
			if (_onMessage != null)
				RunInTask(() => _onMessage(stringResult.ToString(), this));
		}

		private void CallOnDisconnected()
		{
			if (_onDisconnected != null)
				RunInTask(() => _onDisconnected(this));
		}

		private void CallOnConnected()
		{
			if (_onConnected != null)
				RunInTask(() => _onConnected(this));
		}

		private static void RunInTask(Action action)
		{
			Task.Run(action);
		}

		public void Disconnect()
		{
			_cancellationTokenSource.Cancel();
			_ws.Abort();
			_ws.Dispose();
		}
	}

	internal sealed class WebSocketHandler : BaseNetworkProtocol
	{
		private WebSocketWrapper _socket;

		public WebSocketHandler(string ip, ushort sendPort, ushort receivePort, int timeout, SocketIo parentSocket)
		  : base(ip, sendPort, receivePort, timeout, parentSocket)
		{
			if(SendPort == 0)
			{
				SendPort = ReceivePort;
			}

			if(ReceivePort == 0)
			{
				ReceivePort = SendPort;
			}

			if(SendPort != ReceivePort)
			{
				throw new Exception("WebSockets must Send and Receive on the same port.");
			}

			_socket = WebSocketWrapper.Create($@"ws://{ip}:{ReceivePort}");
			_socket.Connect();
			Thread.Sleep(1000);
		}


		public override void Close()
		{
			if (_socket != null)
			{
				_socket.OnMessage(null);
				_socket.Disconnect();
			}

			_listening = false;
			this.NetworkTimeout = 0;
			this.SendPort = 0;
			this.ReceivePort = 0;
		}

		public override async Task ListenAsync(IPEndPoint ReceiveEndPoint)
		{
			_listening = true;

			_socket.OnMessage((msg, sender) =>
			{
				var data = System.Text.Encoding.UTF8.GetBytes(msg);
				ParentSocket.HandleMessage(data, ReceiveEndPoint.Address);
			});

			await Task.CompletedTask;
		}

		public override async Task SendAsync(SocketMessage msg, IPEndPoint endpoint)
		{
			byte[] data = ParentSocket.Serializer.Serialize(msg);

			_socket.SendMessage(data);

			await Task.CompletedTask;
		}

		public override void Send(SocketMessage msg, IPEndPoint endpoint)
		{
			byte[] data = ParentSocket.Serializer.Serialize(msg);

			_socket.SendMessage(data);


		}
	}
}
