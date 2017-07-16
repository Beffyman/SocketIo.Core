using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SocketIo.SocketTypes
{
	/// <summary>
	/// Handles all UDP input via async methods
	/// </summary>
	internal sealed class UDPHandler : BaseNetworkProtocol
	{
		public UDPHandler(string ip, ushort sendPort, ushort receivePort, int timeout, SocketIo parentSocket)
		  : base(ip, sendPort, receivePort, timeout, parentSocket)
		{

		}

		private UdpClient GetUDP(int port)
		{
			return GetUDP(new IPEndPoint(IPAddress.Any, port));
		}

		private UdpClient GetUDP(IPEndPoint endpoint)
		{
			UdpClient client = new UdpClient();
			client.Client.ReceiveBufferSize = 16384;
			client.Client.SendBufferSize = 16384;
			client.ExclusiveAddressUse = false;
			client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			client.Client.Bind(endpoint);
			return client;
		}

		/// <summary>
		/// Listens to incoming UDP packets on the ReceivePort and passes them to the HandleMessage in a Parallel task
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Await.Warning", "CS4014:Await.Warning")]
		public override async Task ListenAsync(IPEndPoint ReceiveEndPoint)
		{
			_listening = true;
			using(var client = GetUDP(ReceiveEndPoint))
			{
				while (_listening)
				{
					try
					{
						UdpReceiveResult asyncReceive = await client.ReceiveAsync();

						if (!_listening) { break; }

						if (asyncReceive != null)
						{
							Task.Run(() => HandleMessage(asyncReceive));
						}
					}
					catch (OperationCanceledException)
					{

					}
					catch (ObjectDisposedException)
					{

					}
					catch (Exception)
					{
						_listening = false;
#if DEBUG
						throw;
#endif
					}
				}
			}

			_listening = false;
		}

		/// <summary>
		/// Handles the network message and hands it to the correct Handler
		/// </summary>
		/// <param name="message"></param>
		private void HandleMessage(UdpReceiveResult message)
		{
			if (message != null && message.Buffer != null && message.Buffer.Length > 0)
			{
				ParentSocket.HandleMessage(message.Buffer, message.RemoteEndPoint.Address);
			}

		}

		/// <summary>
		/// Sends the message and doesn't wait for input, that should be handled in Listen
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="endpoint"></param>
		public override async Task SendAsync(SocketMessage msg, IPEndPoint endpoint)
		{
			try
			{
				msg.CallbackPort = ReceivePort;
				byte[] data = ParentSocket.Serializer.Serialize(msg);

				using (var client = GetUDP(endpoint.Port))
				using (client.CreateTimeoutScope(TimeSpan.FromMilliseconds(NetworkTimeout)))
				{
					await client.SendAsync(data, data.Length, endpoint);
				}
			}
			catch (OperationCanceledException)
			{

			}
			catch (Exception)
			{
#if DEBUG
				throw;
#endif
			}
		}
		/// <summary>
		/// Sends the message and doesn't wait for input, that should be handled in Listen
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="endpoint"></param>
		public override void Send(SocketMessage msg, IPEndPoint endpoint)
		{
			try
			{
				msg.CallbackPort = ReceivePort;
				byte[] data = ParentSocket.Serializer.Serialize(msg);

				using (var client = GetUDP(endpoint.Port))
				using (client.CreateTimeoutScope(TimeSpan.FromMilliseconds(NetworkTimeout)))
				{
					client.SendAsync(data, data.Length, endpoint).GetAwaiter().GetResult();
				}
			}
			catch (OperationCanceledException)
			{

			}
			catch (Exception)
			{
#if DEBUG
				throw;
#endif
			}
		}

		public override void Close()
		{
			_listening = false;

			this.NetworkTimeout = 0;
			this.ReceivePort = 0;
			this.SendPort = 0;
		}

	}
}
