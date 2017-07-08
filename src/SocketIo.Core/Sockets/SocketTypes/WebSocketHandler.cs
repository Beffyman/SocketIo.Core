using SocketIo.SocketTypes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SocketIo.SocketTypes
{
	internal sealed class WebSocketHandler : BaseNetworkProtocol
	{
		private ClientWebSocket _socket;
		private CancellationTokenSource _cancelSource;
		private CancellationToken _cancelToken;

		public WebSocketHandler(ushort receivePort, ushort sendPort, int timeout, SocketIo parentSocket)
			: base(receivePort, sendPort, timeout, parentSocket)
		{
			_socket = new ClientWebSocket();
			_cancelSource = new CancellationTokenSource();
			_cancelToken = _cancelSource.Token;
		}


		public override void Close()
		{
			if(_socket != null)
			{
				_socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing normally", _cancelToken).Wait();
				_cancelSource.Cancel();
				_cancelSource = new CancellationTokenSource();
				_cancelToken = _cancelSource.Token;
			}

			_listening = false;
			this.NetworkTimeout = 0;
			this.ReceivePort = 0;
			this.SendPort = 0;
		}

		public override async Task ListenAsync(IPEndPoint ReceiveEndPoint)
		{
			byte[] buffer = new byte[4096];
			_listening = true;
			while (_listening)
			{
				await _socket.ConnectAsync(new Uri($@"ws://{ReceiveEndPoint.Address}:{ReceiveEndPoint.Port}"), _cancelToken);

				var result = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
				if (result.MessageType == WebSocketMessageType.Binary)
				{
					ParentSocket.HandleMessage(buffer, ReceiveEndPoint.Address);
				}
			}

			if (_socket != null)
			{
				await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing normally", _cancelToken);
			}

			_listening = false;

		}

		public override async Task SendAsync(SocketMessage msg, IPEndPoint endpoint)
		{
			byte[] data = ParentSocket.Serializer.Serialize(msg);
			await _socket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Binary, true, _cancelToken);
		}
	}
}
