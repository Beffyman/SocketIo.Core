using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Net;

namespace SocketIo
{
	public abstract class BaseEmitter
	{
		public readonly string Event;
		public readonly Guid Id;
		public IPEndPoint CurrentSender { get; protected set; }

		public BaseEmitter(string @event)
		{
			Id = Guid.NewGuid();
			Event = @event;
		}

		public void Invoke(object arg, IPEndPoint sender)
		{
			CurrentSender = sender;
			Invoke(arg);
			CurrentSender = null;
		}
		public abstract void Invoke(object arg);
	}


	public sealed class Emitter : BaseEmitter
	{
				
		public Action Body { get; set; }

		public Emitter(string @event, Action body) : base(@event)
		{
			Body = body;
		}

		public override void Invoke(object arg)
		{
			Body();
		}
	}

	public sealed class Emitter<T> : BaseEmitter
	{
		
		public Action<T> Body { get; set; }

		public Emitter(string @event, Action<T> body) : base(@event)
		{
			Body = body;
		}

		public override void Invoke(object arg)
		{			
			Body((T)Convert.ChangeType(arg, typeof(T)));
		}
	}

}
