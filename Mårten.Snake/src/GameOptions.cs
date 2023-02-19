using System;
using Cygni.Snake.Client.Models;
using ModelGameMode = Cygni.Snake.Client.Models.GameMode;

namespace Mårten.Snake;

public class GameOptions
{
	public Uri BaseUri { get; set; } = new Uri("ws://snake.cygni.se/");
	public string Name { get; set; } = "Unnamed";
	public GameMode GameMode { get; set; } = GameMode.Training;
	public string? ArenaId { get; set; } = null;
	public string Controller { get; set; } = "Edge";
	public GameSettings Settings { get; set; } = new GameSettings();
	public float TickTimePercentage { get; set; } = 0.75f;

	public Uri Uri => GameMode switch
	{
		ModelGameMode.Arena => new Uri(BaseUri, GameMode.ToString().ToLowerInvariant() + $"/{ArenaId}"),
		_ => new Uri(BaseUri, GameMode.ToString().ToLowerInvariant()),
	};
}
