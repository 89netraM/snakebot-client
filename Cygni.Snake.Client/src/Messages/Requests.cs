using System;
using Cygni.Snake.Client.Models;

namespace Cygni.Snake.Client.Messages;

public record ClientInfo(
	string Language,
	string LanguageVersion,
	string OperatingSystem,
	string OperatingSystemVersion,
	string ClientVersion) : Message;

public record HeartBeatRequest() : Message;

public record RegisterMove(Guid GameId, long GameTick, Direction Direction) : Message;

public record RegisterPlayer(string PlayerName, GameSettings GameSettings) : Message;

public record StartGame() : Message;
