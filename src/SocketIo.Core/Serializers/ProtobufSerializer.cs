//using System.IO;

//namespace SocketIo.Core.Serializers
//{
//	public sealed class ProtobufSerializer : ISerializer
//	{
//		public T Deserialize<T>(byte[] array)
//		{
//			using (var memoryStream = new MemoryStream(array))
//			{
//				return ProtoBuf.Serializer.Deserialize<T>(memoryStream);
//			}
//		}

//		public byte[] Serialize<T>(T obj)
//		{
//			using (var memoryStream = new MemoryStream())
//			{
//				ProtoBuf.Serializer.Serialize<T>(memoryStream, obj);
//				byte[] data = memoryStream.ToArray();
//				return data;
//			}
//		}
//	}
//}
