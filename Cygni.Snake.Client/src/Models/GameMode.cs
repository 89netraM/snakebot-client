using System.Text.Json.Serialization;

namespace Cygni.Snake.Client.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GameMode
{
	Training,
	Tournament,
	HighScore,
	Arena,
}
