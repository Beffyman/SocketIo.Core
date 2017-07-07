using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SocketIo.SocketTypes
{
	internal abstract class BaseNetworkProtocol
	{
		public bool Listening { get { return _listening; } }
		protected bool _listening = false;

		protected int NetworkTimeout = 0;
		protected ushort ReceivePort = 0;
		protected ushort SendPort = 0;

		protected SocketIo ParentSocket { get; set; }

		public BaseNetworkProtocol(ushort receivePort, ushort sendPort, int timeout, SocketIo parentSocket)
		{
			Setup(receivePort, sendPort, timeout);
			ParentSocket = parentSocket;
		}

		internal void Setup(ushort receivePort, ushort sendPort, int timeout)
		{
			NetworkTimeout = timeout;
			ReceivePort = receivePort;
			SendPort = sendPort;
		}

		/// <summary>
		/// Listens to incoming UDP packets on the ReceivePort and passes them to the HandleMessage in a Parallel task
		/// </summary>
		public abstract void Listen(IPEndPoint ReceiveEndPoint);


		/// <summary>
		/// Sends the message and doesn't wait for input, that should be handled in Listen
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="endpoint"></param>
		public abstract void Send(SocketMessage msg, IPEndPoint endpoint);

		public abstract void Close();
	}



	internal static class AsyncTimeouts
	{
		public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
		{
			var tcs = new TaskCompletionSource<bool>();
			using (cancellationToken.Register(
				  s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
				if (task != await Task.WhenAny(task, tcs.Task))
					throw new OperationCanceledException(cancellationToken);
			return await task;
		}



		public static IDisposable CreateTimeoutScope(this IDisposable disposable, TimeSpan timeSpan)
		{
			var cancellationTokenSource = new CancellationTokenSource(timeSpan);
			var cancellationTokenRegistration = cancellationTokenSource.Token.Register(disposable.Dispose);
			return new DisposableScope(
			  () =>
			  {
				  cancellationTokenRegistration.Dispose();
				  cancellationTokenSource.Dispose();
				  disposable.Dispose();
			  });
		}

		public sealed class DisposableScope : IDisposable
		{
			private readonly Action _closeScopeAction;
			public DisposableScope(Action closeScopeAction)
			{
				_closeScopeAction = closeScopeAction;
			}
			public void Dispose()
			{
				_closeScopeAction();
			}
		}
	}
}
