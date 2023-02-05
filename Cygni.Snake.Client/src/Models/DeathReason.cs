using System.Text.Json.Serialization;

namespace Cygni.Snake.Client.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DeathReason
{
	CollisionWithWall,
	CollisionWithObstacle,
	CollisionWithSnake,
	CollisionWithSelf,
}
