using System;
using System.Collections.Generic;
using System.Linq;
using Cygni.Snake.Client.Models;

namespace Cygni.Snake.Utils;

public class Snake
{
	private readonly SnakeInfo snakeInfo;

	public Guid Id => snakeInfo.Id;
	public string Name => snakeInfo.Name;
	public int Points => snakeInfo.Points;
	public long TailProtectedForGameTicks => snakeInfo.TailProtectedForGameTicks;

	public Direction HeadDirection { get; } = Direction.Up;

	public IReadOnlyList<Vector2> Positions { get; }

	public Snake(SnakeInfo snakeInfo, MapInfo mapInfo)
	{
		this.snakeInfo = snakeInfo;

		Positions = this.snakeInfo.Positions.Select(mapInfo.IndexToPos).ToArray();

		if (Positions.Count > 1)
		{
			HeadDirection = Vector2.ReverseDirections[Positions[0] - Positions[1]];
		}
	}
}
