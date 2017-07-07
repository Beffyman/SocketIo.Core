using System;
using System.Net;

namespace SocketIo
{
	/// <summary>
	/// Class all emitter types inherit from
	/// </summary>
	public abstract class BaseEmitter
	{
		internal readonly string Event;
		internal readonly Guid Id;
		internal IPEndPoint CurrentSender { get; set; }


		internal BaseEmitter(string @event)
		{
			Id = Guid.NewGuid();
			Event = @event;
		}

		internal void Invoke(SocketMessage arg, IPEndPoint sender)
		{
			CurrentSender = sender;
			Invoke(arg.Content);
			CurrentSender = null;
		}
		internal abstract void Invoke(object arg);
	}

	/// <summary>
	/// Parameterless Emitter
	/// </summary>
	public sealed class Emitter : BaseEmitter
	{

		internal Action Body { get; set; }

		internal Emitter(string @event, Action body) : base(@event)
		{
			Body = body;
		}

		internal override void Invoke(object arg)
		{
			Body();
		}
	}

	/// <summary>
	/// Emitter with 1 Parameter
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public sealed class Emitter<T> : BaseEmitter
	{

		internal Action<T> Body { get; set; }

		internal Emitter(string @event, Action<T> body) : base(@event)
		{
			Body = body;
		}

		internal override void Invoke(object arg)
		{
			Body((T)Convert.ChangeType(arg, typeof(T)));
		}
	}

}
