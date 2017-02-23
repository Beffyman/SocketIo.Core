using System;
using System.Collections.Generic;
using System.Text;

namespace SocketIo.SocketTypes
{
	/// <summary>
	/// Socket Type
	/// </summary>
    public enum SocketHandlerType
    {
		/// <summary>
		/// UDP is a connection-less, unreliable, datagram protocol 
		/// </summary>
		Udp,
		/// <summary>
		/// TCP is a connection-oriented, reliable and stream based protocol
		/// </summary>
		Tcp,
    }
}
