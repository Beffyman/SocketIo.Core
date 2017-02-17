# Socket.Io.Core
[![SocketIo.Core](https://img.shields.io/nuget/v/SocketIo.Core.svg?maxAge=2592000)](https://www.nuget.org/packages/SocketIo.Core/)

Abstraction of TCP/UDP sockets that writes like Socket.IO

## Example

```csharp

bool hit1 = false;
bool hit2 = false;


var socket = Io.Connect("127.0.0.1", 4333, 4333, SocketHandlerType.Udp);

socket.On("connect", () =>
{
	hit1 = true;
	socket.On<int>("test", (package) =>
	{
		if(package == 5)
		{
			hit2 = true;
		}
	});

	socket.Emit("test",5);

});

socket.Emit("connect");

while (!hit1 || !hit2)
{
	Thread.Sleep(100);
}
socket.Close();

```

[Nuget](https://www.nuget.org/packages/SocketIo.Core)