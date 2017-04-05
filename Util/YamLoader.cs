using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Foregunners
{
	public static class YamLoader
	{
		public static T Load<T>(string path)
		{
			StreamReader reader = new StreamReader(path);

			DeserializerBuilder data = new DeserializerBuilder();
			data.WithNamingConvention(new PascalCaseNamingConvention());

			Deserializer res = data.Build();
			T obj = res.Deserialize<T>(reader);
			
			if (obj != null)
				return obj;
			else
				throw new InvalidDataException("Path " + path + " does not return a valid " + typeof(T));
		}
	}
}
