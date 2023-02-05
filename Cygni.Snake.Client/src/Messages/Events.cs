using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Cygni.Snake.Client.Models;

namespace Cygni.Snake.Client.Messages;

public record ArenaEndedEvent(string ArenaName) : Message;

public record ArenaUpdateEvent(
	string ArenaName,
	string GameId,
	bool Ranked,
	IDictionary<string, long> Rating,
	IList<string> OnlinePlayers,
	IList<ArenaHistory> GameHistory) : Message;

public record GameAbortedEvent(string GameId) : Message;

public record GameChangedEvent(string GameId) : Message;

public record GameCreatedEvent(string GameId) : Message;

public record GameEndedEvent(
	string PlayerWinnerId,
	string PlayerWinnerName,
	string GameId,
	long gameTick,
	Map Map) : Message;

public record GameLinkEvent(string GameId, string Url) : Message;

public record GameResultEvent(string GameId, IReadOnlyList<PlayerRank> PlayerRanks) : Message;

public record GameStartingEvent(
	Guid GameId,
	[property: JsonPropertyName("noofPlayers")]
	int NoOfPlayers,
	int Width,
	int Height,
	GameSettings GameSettings) : Message;

public record MapUpdateEvent(long GameTick, Guid GameId, Map Map) : Message;

public record SnakeDeadEvent(
	DeathReason DeathReason,
	Guid PlayerId,
	int X,
	int Y,
	string GameId,
	long GameTick) : Message;

public record TournamentEndedEvent(
	Guid PlayerWinnerId,
	Guid GameId,
	IReadOnlyList<PlayerPoints> GameResult,
	string tournamentName,
	string tournamentId) : Message;
