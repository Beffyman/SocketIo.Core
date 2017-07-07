using System;

namespace SocketIo
{

	internal class SocketMessage
	{
		public string Event { get; set; }

		public object Content { get; set; }

		public Guid Id = Guid.NewGuid();

		public ushort CallbackPort { get; set; }
	}
}
