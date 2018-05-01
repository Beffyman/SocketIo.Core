using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SocketIo.SocketTypes
{
	internal sealed class TCPHandler : BaseNetworkProtocol
	{
		public TCPHandler(string ip, ushort sendPort, ushort receivePort, int timeout, SocketIo parentSocket)
		  : base(ip, sendPort, receivePort, timeout, parentSocket)
		{

		}

		private TcpListener Listener { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Await.Warning", "CS4014:Await.Warning")]
		public override async Task ListenAsync(IPEndPoint ReceiveEndPoint)
		{
			if (Listener == null)
			{
				Listener = new TcpListener(ReceiveEndPoint);
			}

			_listening = true;
			TcpClient listenerClient;

			Listener.Start();
			while (_listening)
			{
				try
				{
					listenerClient = await Listener.AcceptTcpClientAsync();
					if (!_listening) { break; }
					var stream = listenerClient.GetStream();
					var data = new StreamReader(stream).ReadToEnd();
					Task.Run(() =>
					{
						var endpoint = (listenerClient.Client.RemoteEndPoint as IPEndPoint).Address;
						ParentSocket.HandleMessage(data, endpoint);
					});
				}
				catch (OperationCanceledException)
				{

				}
				catch (Exception)
				{
					_listening = false;
					Listener.Stop();
					throw;
				}
			}
			Listener.Stop();
			_listening = false;
		}

		public override async Task SendAsync(SocketMessage msg, IPEndPoint endpoint)
		{
			try
			{
				msg.CallbackPort = ReceivePort;
				byte[] data = ParentSocket.Serializer.Serialize(msg);

				using (var client = new TcpClient())
				using (client.CreateTimeoutScope(TimeSpan.FromMilliseconds(NetworkTimeout)))
				{
					await client.ConnectAsync(endpoint.Address, endpoint.Port);
					NetworkStream stream = client.GetStream();
					stream.Write(data, 0, data.Length);
				}
			}
			catch (OperationCanceledException)
			{

			}
			catch (Exception)
			{
				throw;
			}
		}

		public override void Send(SocketMessage msg, IPEndPoint endpoint)
		{
			try
			{
				msg.CallbackPort = ReceivePort;
				byte[] data = ParentSocket.Serializer.Serialize(msg);

				using (var client = new TcpClient())
				using (client.CreateTimeoutScope(TimeSpan.FromMilliseconds(NetworkTimeout)))
				{
					client.ConnectAsync(endpoint.Address, endpoint.Port).GetAwaiter().GetResult();
					NetworkStream stream = client.GetStream();
					stream.Write(data, 0, data.Length);
				}

			}
			catch (OperationCanceledException)
			{

			}
			catch (Exception)
			{
				throw;
			}
		}

		public override void Close()
		{
			_listening = false;

			if (Listener != null)
			{
				Listener.Stop();
				Listener.Server.Dispose();
			}

			this.NetworkTimeout = 0;
			this.SendPort = 0;
			this.ReceivePort = 0;
		}
	}
}