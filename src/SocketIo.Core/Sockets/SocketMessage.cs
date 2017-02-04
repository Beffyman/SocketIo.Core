using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace SocketIo
{

	internal class SocketMessage
	{
		public string Event { get; set; }
		public object Content { get; set; }

		public ushort CallbackPort { get; set; }
	}
}
