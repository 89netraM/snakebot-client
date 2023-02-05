using Cygni.Snake.Client.Models;

namespace Cygni.Snake.Client.Messages;

public record HeartBeatResponse() : Message;

public record PlayerRegistered(
	string Name,
	GameSettings GameSettings,
	GameMode GameMode) : Message;
