using SocketIo.SocketTypes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.WebSockets;
using System.Threading;

namespace SocketIo.Core.Sockets.SocketTypes
{
	//internal sealed class WebSocketHandler : BaseNetworkProtocol
	//{
	//	private ClientWebSocket _socket;

	//	public WebSocketHandler(ushort receivePort, ushort sendPort, int timeout, SocketIo parentSocket)
	//		: base(receivePort, sendPort, timeout, parentSocket)
	//	{
	//		_socket = new ClientWebSocket();
	//	}


	//	public override void Close()
	//	{
	//		this.NetworkTimeout = 0;
	//		this.ReceivePort = 0;
	//		this.SendPort = 0;
	//	}

	//	public override void Listen(IPEndPoint ReceiveEndPoint)
	//	{
	//		byte[] buffer = new byte[4096];

	//		_socket.ConnectAsync(new Uri($@"ws://{ReceiveEndPoint}"), CancellationToken.None).Wait();

	//		var result = _socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None).GetAwaiter().GetResult();
	//		if(result.MessageType == WebSocketMessageType.Binary)
	//		{
	//			ParentSocket.HandleMessage(buffer, (client.Client.RemoteEndPoint as IPEndPoint).Address)
	//		}
	//	}

	//	public override void Send(SocketMessage msg, IPEndPoint endpoint)
	//	{
	//		throw new NotImplementedException();
	//	}
	//}
}
