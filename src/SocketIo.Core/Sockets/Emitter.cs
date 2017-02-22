using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Net;
using System.Collections.Concurrent;

namespace SocketIo
{
	public abstract class BaseEmitter
	{
		public readonly string Event;
		public readonly Guid Id;
		public ConcurrentDictionary<Guid,IPEndPoint> SenderList { get; set; }
		public IPEndPoint CurrentSender { get; set; }


		public BaseEmitter(string @event)
		{
			SenderList = new ConcurrentDictionary<Guid, IPEndPoint>();
			Id = Guid.NewGuid();
			Event = @event;
		}

		internal void Invoke(SocketMessage arg, IPEndPoint sender)
		{
			CurrentSender = sender;
			SenderList.TryAdd(arg.Id, sender);
			Invoke(arg.Content);
			SenderList.TryRemove(arg.Id, out sender);
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
