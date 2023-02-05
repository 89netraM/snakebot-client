using System.Collections.Generic;

namespace Cygni.Snake.Client.Models;

public record Map(
	int Width,
	int Height,
	long WorldTick,
	IReadOnlyList<SnakeInfo> SnakeInfos,
	IReadOnlyList<int> FoodPositions,
	IReadOnlyList<int> ObstaclePositions);
