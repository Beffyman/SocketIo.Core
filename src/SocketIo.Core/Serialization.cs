using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using System.Text;

namespace SocketIo
{
	internal static class Serialization
	{
		private static readonly JsonSerializerSettings SETTINGS = new JsonSerializerSettings
		{
			Formatting = Formatting.Indented,
			TypeNameHandling = TypeNameHandling.All,
			TypeNameAssemblyFormat = FormatterAssemblyStyle.Full
		};


		public static byte[] Serialize<T>(this T obj)
		{
			string json = JsonConvert.SerializeObject(obj, SETTINGS);
			byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);

			return bytes;
		}

		public static T Deserialize<T>(this byte[] array)
		{
			string json = System.Text.Encoding.UTF8.GetString(array);

			T obj = JsonConvert.DeserializeObject<T>(json, SETTINGS);
			
			return obj;
		}


	}
}
