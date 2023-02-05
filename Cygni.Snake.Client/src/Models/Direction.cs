using System.Text.Json.Serialization;
using Cygni.Snake.Client.JsonSerializers;

namespace Cygni.Snake.Client.Models;

[JsonConverter(typeof(JsonCapitalsStringEnumConverter))]
public enum Direction
{
	Up,
	Right,
	Down,
	Left,
}
