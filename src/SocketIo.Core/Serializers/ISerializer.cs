namespace SocketIo.Core.Serializers
{
	public interface ISerializer
	{
		byte[] Serialize<T>(T obj);

		T Deserialize<T>(byte[] array);
	}
}
