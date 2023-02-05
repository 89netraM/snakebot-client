using System.Text.Json;

namespace Cygni.Snake.Client.JsonSerializers;

internal class JsonCapitalsNamingPolicy : JsonNamingPolicy
{
	public override string ConvertName(string name) =>
		name.ToUpperInvariant();
}
