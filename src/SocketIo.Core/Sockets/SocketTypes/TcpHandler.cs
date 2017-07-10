using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SocketIo.SocketTypes
{
	internal sealed class TCPHandler : BaseNetworkProtocol
	{
		public TCPHandler(ushort receivePort, ushort sendPort, int timeout, SocketIo parentSocket)
		  : base(receivePort, sendPort, timeout, parentSocket)
		{

		}



		private TcpClient GetTCPClient(int port)
		{
			return GetTCPClient(new IPEndPoint(IPAddress.Any, port));
		}

		private TcpClient GetTCPClient(IPEndPoint endpoint)
		{
			TcpClient client = new TcpClient();
			client.ExclusiveAddressUse = false;
			client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			client.Client.Bind(endpoint);
			return client;
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
			TcpClient client;//Store outside so we need to reallocate less memory
			Listener.Start();
			while (_listening)
			{
				//ExtendedConsole.Output($"Reading TCP port {ReceivePort}. . .");

				try
				{
					byte[] data = new byte[4096];
					client = await Listener.AcceptTcpClientAsync();
					if (!_listening) { break; }
					var stream = client.GetStream();
					stream.Read(data, 0, data.Length);
					Task.Run(() =>
					{
						var endpoint = (client.Client.RemoteEndPoint as IPEndPoint).Address;
						ParentSocket.HandleMessage(data, endpoint);
					});
				}
				catch (Exception ex)
				{
					_listening = false;
					Listener.Stop();
					Console.WriteLine(ex.ToString());
				}
			}
			Listener.Stop();
			_listening = false;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Await.Warning", "CS4014:Await.Warning")]
		public override void Listen(IPEndPoint ReceiveEndPoint)
		{
			if (Listener == null)
			{
				Listener = new TcpListener(ReceiveEndPoint);
			}

			_listening = true;
			TcpClient client;//Store outside so we need to reallocate less memory
			Listener.Start();
			while (_listening)
			{
				//ExtendedConsole.Output($"Reading TCP port {ReceivePort}. . .");

				try
				{
					byte[] data = new byte[4096];
					client = Listener.AcceptTcpClientAsync().GetAwaiter().GetResult();
					if (!_listening) { break; }
					var stream = client.GetStream();
					stream.Read(data, 0, data.Length);
					Task.Run(() =>
					{
						var endpoint = (client.Client.RemoteEndPoint as IPEndPoint).Address;
						ParentSocket.HandleMessage(data, endpoint);
					});
				}
				catch (Exception ex)
				{
					_listening = false;
					Listener.Stop();
					Console.WriteLine(ex.ToString());
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
				using (var CurrentClient = new TcpClient())
				{
					using (CurrentClient.CreateTimeoutScope(TimeSpan.FromMilliseconds(NetworkTimeout)))
					{
						await CurrentClient.ConnectAsync(endpoint.Address, endpoint.Port);
						NetworkStream stream = CurrentClient.GetStream();
						stream.Write(data, 0, data.Length);
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}

		public override void Send(SocketMessage msg, IPEndPoint endpoint)
		{
			try
			{
				msg.CallbackPort = ReceivePort;
				byte[] data = ParentSocket.Serializer.Serialize(msg);
				using (var CurrentClient = new TcpClient())
				{
					using (CurrentClient.CreateTimeoutScope(TimeSpan.FromMilliseconds(NetworkTimeout)))
					{
						CurrentClient.ConnectAsync(endpoint.Address, endpoint.Port).GetAwaiter().GetResult();
						NetworkStream stream = CurrentClient.GetStream();
						stream.Write(data, 0, data.Length);
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}

		public override void Close()
		{
			_listening = false;
			if(Listener != null)
			{
				Listener.Stop();
				Listener.Server.Dispose();
			}

			this.NetworkTimeout = 0;
			this.ReceivePort = 0;
			this.SendPort = 0;
		}
	}
}