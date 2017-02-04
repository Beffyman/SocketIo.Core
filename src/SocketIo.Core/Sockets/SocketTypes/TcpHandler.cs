using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SocketIo.SocketTypes
{
	internal class TCPHandler : BaseNetworkProtocol
	{
		public TCPHandler(ushort receivePort, ushort sendPort, int timeout, SocketIo parentSocket)
		  : base(receivePort, sendPort, timeout,parentSocket)
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
		public override async void Listen(IPEndPoint ReceiveEndPoint)
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


					byte[] data = new byte[1024];
					client = await Listener.AcceptTcpClientAsync();
					if (!_listening) { break; }
					var stream = client.GetStream();
					stream.Read(data, 0, data.Length);
					await Task.Factory.StartNew(() => ParentSocket.HandleMessage(data, client.Client.RemoteEndPoint as IPEndPoint));
				}
				catch (Exception ex)
				{
					_listening = false;
					Listener.Stop();
#if DEBUG
					throw ex;
#endif
				}
			}
			Listener.Stop();
			_listening = false;
		}


		public override void Send(SocketMessage msg, IPEndPoint endpoint)
		{
			try
			{
				msg.CallbackPort = ReceivePort;
				byte[] data = msg.Serialize();
				using (var CurrentClient = new TcpClient())
				{
					using (CurrentClient.CreateTimeoutScope(TimeSpan.FromMilliseconds(NetworkTimeout)))
					{
						CurrentClient.ConnectAsync(endpoint.Address, endpoint.Port).Wait();
						NetworkStream stream = CurrentClient.GetStream();
						stream.Write(data, 0, data.Length);
					}
				}
			}
			catch (Exception ex)
			{
#if DEBUG
				throw ex;
#endif
			}
		}

		public override void Close()
		{
			_listening = false;
			Listener.Stop();
			this.NetworkTimeout = 0;
			this.ReceivePort = 0;
			this.SendPort = 0;
		}
	}
}