using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Cygni.Snake.Client.Messages;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
// Events
[JsonDerivedType(typeof(ArenaEndedEvent), "se.cygni.snake.api.event.ArenaEndedEvent")]
[JsonDerivedType(typeof(ArenaUpdateEvent), "se.cygni.snake.api.event.ArenaUpdateEvent")]
[JsonDerivedType(typeof(GameAbortedEvent), "se.cygni.snake.api.event.GameAbortedEvent")]
[JsonDerivedType(typeof(GameChangedEvent), "se.cygni.snake.api.event.GameChangedEvent")]
[JsonDerivedType(typeof(GameCreatedEvent), "se.cygni.snake.api.event.GameCreatedEvent")]
[JsonDerivedType(typeof(GameEndedEvent), "se.cygni.snake.api.event.GameEndedEvent")]
[JsonDerivedType(typeof(GameLinkEvent), "se.cygni.snake.api.event.GameLinkEvent")]
[JsonDerivedType(typeof(GameResultEvent), "se.cygni.snake.api.event.GameResultEvent")]
[JsonDerivedType(typeof(GameStartingEvent), "se.cygni.snake.api.event.GameStartingEvent")]
[JsonDerivedType(typeof(MapUpdateEvent), "se.cygni.snake.api.event.MapUpdateEvent")]
[JsonDerivedType(typeof(SnakeDeadEvent), "se.cygni.snake.api.event.SnakeDeadEvent")]
[JsonDerivedType(typeof(TournamentEndedEvent), "se.cygni.snake.api.event.TournamentEndedEvent")]
// Exceptions
[JsonDerivedType(typeof(ArenaIsFull), "se.cygni.snake.api.exception.ArenaIsFull")]
[JsonDerivedType(typeof(InvalidArenaName), "se.cygni.snake.api.exception.InvalidArenaName")]
[JsonDerivedType(typeof(InvalidMessage), "se.cygni.snake.api.exception.InvalidMessage")]
[JsonDerivedType(typeof(InvalidPlayerName), "se.cygni.snake.api.exception.InvalidPlayerName")]
[JsonDerivedType(typeof(NoActiveTournament), "se.cygni.snake.api.exception.NoActiveTournament")]
// Requests
[JsonDerivedType(typeof(ClientInfo), "se.cygni.snake.api.request.ClientInfo")]
[JsonDerivedType(typeof(HeartBeatRequest), "se.cygni.snake.api.request.HeartBeatRequest")]
[JsonDerivedType(typeof(RegisterMove), "se.cygni.snake.api.request.RegisterMove")]
[JsonDerivedType(typeof(RegisterPlayer), "se.cygni.snake.api.request.RegisterPlayer")]
[JsonDerivedType(typeof(StartGame), "se.cygni.snake.api.request.StartGame")]
// Responses
[JsonDerivedType(typeof(HeartBeatResponse), "se.cygni.snake.api.response.HeartBeatResponse")]
[JsonDerivedType(typeof(PlayerRegistered), "se.cygni.snake.api.response.PlayerRegistered")]
public abstract record Message
{
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public Guid ReceivingPlayerId { get; set; }
}

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(Message))]
public partial class MessageJsonSerializer : JsonSerializerContext
{
	public static Message? Deserialize(string json) =>
		JsonSerializer.Deserialize(json, MessageJsonSerializer.Default.Message);

	public static string Serialize(Message message) =>
		JsonSerializer.Serialize(message, MessageJsonSerializer.Default.Message);
}
