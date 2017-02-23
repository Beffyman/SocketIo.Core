# Socket.Io.Core
[![SocketIo.Core](https://img.shields.io/nuget/v/SocketIo.Core.svg?maxAge=2592000)](https://www.nuget.org/packages/SocketIo.Core/)

Abstraction of TCP/UDP sockets that writes like Socket.IO

## Example

```csharp

bool hit1 = false;
bool hit2 = false;


var socket = Io.Create("127.0.0.1", 4533, 4533, SocketHandlerType.Udp);

socket.On("connect", () =>
{
	hit1 = true;
	socket.On("test", (int package) =>
	{
		if (package == 5)
		{
			hit2 = true;
		}
	});

	socket.Emit("test", 5);

});

socket.Emit("connect");

int timer = 0;
int timeout = 5000;
while ((!hit1 || !hit2)
	&& timer < timeout)
{
	Thread.Sleep(100);
	timer += 100;
}
socket.Close();

Assert.IsTrue(hit1 && hit2);

```