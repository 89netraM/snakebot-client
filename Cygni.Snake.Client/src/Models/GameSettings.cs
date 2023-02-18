using System.Text.Json.Serialization;

namespace Cygni.Snake.Client.Models;

public record GameSettings(
	[property: JsonPropertyName("maxNoofPlayers")]
	int MaxNumberOfPlayers = 5,
	int StartSnakeLength = 1,
	int TimeInMsPerTick = 250,
	bool ObstaclesEnabled = true,
	bool FoodEnabled = true,
	bool HeadToTailConsumes = true,
	bool TailConsumeGrows = false,
	int AddFoodLikelihood = 15,
	int RemoveFoodLikelihood = 5,
	int SpontaneousGrowthEveryNWorldTick = 3,
	bool TrainingGame = true,
	int PointsPerLength = 1,
	int PointsPerFood = 2,
	int PointsPerCausedDeath = 5,
	int PointsPerNibble = 10,
	[property: JsonPropertyName("noofRoundsTailProtectedAfterNibble")]
	int NumberOfRoundsTailProtectedAfterNibble = 3,
	int StartFood = 0,
	int StartObstacles = 5);
