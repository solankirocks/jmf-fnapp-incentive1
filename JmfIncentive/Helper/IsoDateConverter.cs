using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Helper
{
	public class IsoDateConverter : JsonConverter<DateTime>
	{
		public override DateTime ReadJson(JsonReader reader, Type objectType, DateTime existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			var token = JToken.Load(reader);
			if (token.Type == JTokenType.String && token.ToString().StartsWith("ISODate"))
			{
				var isoDate = token.ToString().Replace("ISODate(", "").Replace(")", "").Replace("\"", "");
				return DateTime.Parse(isoDate);
			}
			return token.ToObject<DateTime>();
		}

		public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer)
		{
			writer.WriteValue(value.ToString("o"));
		}
	}
}
