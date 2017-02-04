using SocketIo.SocketTypes;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace SocketIo
{
	public static class Io
	{

		public static SocketIo Connect(string ip, ushort recievePort,ushort sendPort,SocketHandlerType type,int timeout=3000, string initialEmit=null)
		{
			SocketIo socket = new SocketIo(sendPort, recievePort, timeout, type);
			socket.Connect(ip,initialEmit);

			return socket;
		}

	}
}
