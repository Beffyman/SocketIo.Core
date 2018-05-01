using Newtonsoft.Json;

namespace SocketIo.Core.Serializers
{
	public sealed class JsonSerializer : ISerializer
	{
		private readonly JsonSerializerSettings Settings = new JsonSerializerSettings
		{
			Formatting = Formatting.Indented,
			TypeNameHandling = TypeNameHandling.All,
			TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
		};

        public T Deserialize<T>(byte[] array)
		{
			string json = System.Text.Encoding.UTF8.GetString(array);

			T obj = JsonConvert.DeserializeObject<T>(json, Settings);
			return obj;
		}

		public byte[] Serialize<T>(T obj)
		{
			string json = JsonConvert.SerializeObject(obj, Settings);
			byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);

			return bytes;
		}
	}
}
