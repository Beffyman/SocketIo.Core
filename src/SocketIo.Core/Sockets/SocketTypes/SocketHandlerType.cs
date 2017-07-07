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
		/// <summary>
		/// Websocket provides full-duplex communication channels over a single TCP connection. ws:// will be pre-appended to the ip provided
		/// </summary>
		//WebSocket
	}
}
