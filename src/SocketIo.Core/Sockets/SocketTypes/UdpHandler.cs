using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketIo.SocketTypes
{
	/// <summary> 
	/// Handles all UDP input via async methods 
	/// </summary> 
	internal class UDPHandler : BaseNetworkProtocol
	{
		public UDPHandler(ushort receivePort, ushort sendPort, int timeout, SocketIo parentSocket)
		  : base(receivePort, sendPort, timeout, parentSocket)
		{

		}

		/// <summary> 
		/// Wrapper for creating a non-blocking UDP port 
		/// </summary> 
		/// <param name="port"></param> 
		/// <returns></returns> 
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
		public override async void Listen(IPEndPoint ReceiveEndPoint)
		{
			_listening = true;
			//ExtendedConsole.Output($"Reading UDP port {ReceivePort}. . ."); 
			using (var CurrentClient = GetUDP(ReceiveEndPoint))//Keep outside while loop since that would create overhead and might miss packets 
			{

				while (_listening)
				{
					try
					{
						UdpReceiveResult asyncReceive = await CurrentClient.ReceiveAsync();
						//Task<UdpReceiveResult> asyncReceive = CurrentClient.ReceiveAsync(); 
						//asyncReceive.Wait(); 
						if (!_listening) { break; }
						//Parallel.Invoke(() => HandleMessage(asyncReceive)); 
						await Task.Factory.StartNew(() => HandleMessage(asyncReceive));
					}
					catch (ObjectDisposedException)
					{

					}
					catch (Exception ex)
					{
						_listening = false;
#if DEBUG
						throw ex;
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
				ParentSocket.HandleMessage(message.Buffer, message.RemoteEndPoint);
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
				byte[] data = msg.Serialize();
				using (var CurrentClient = GetUDP(endpoint.Port))
				{
					using (CurrentClient.CreateTimeoutScope(TimeSpan.FromMilliseconds(NetworkTimeout)))
					{
						CurrentClient.SendAsync(data, data.Length, endpoint).Wait();
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
			this.NetworkTimeout = 0;
			this.ReceivePort = 0;
			this.SendPort = 0;
		}

	}
}
