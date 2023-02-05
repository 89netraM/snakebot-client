using System.Text.Json.Serialization;

namespace Cygni.Snake.Client.Models;

public record GameSettings(
	[property: JsonPropertyName("maxNoofPlayers")]
	int MaxNoOfPlayers = 5,
	int StartSnakeLength = 1,
	int TimeInMsPerTick = 250,
	bool ObstaclesEnabled = false,
	bool FoodEnabled = true,
	bool HeadToTailConsumes = false,
	bool TailConsumeGrows = false,
	int AddFoodLikelihood = 15,
	int RemoveFoodLikelihood = 5);
