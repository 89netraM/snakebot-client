using System.Text.Json.Serialization;

namespace Cygni.Snake.Client.JsonSerializers;

internal class JsonCapitalsStringEnumConverter : JsonStringEnumConverter
{
	public JsonCapitalsStringEnumConverter() : base(new JsonCapitalsNamingPolicy()) { }
}
