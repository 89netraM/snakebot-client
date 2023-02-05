using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cygni.Snake.Client.Messages;
using Cygni.Snake.Client.Models;
using Microsoft.Extensions.Logging;

namespace Mårten.Snake.Services.Controllers;

public class EdgeController : ControllerBase
{
	private readonly ILogger<EdgeController> logger;

	public EdgeController(ClientService client, ILoggerFactory loggerFactory) : base(client, loggerFactory)
	{
		logger = loggerFactory.CreateLogger<EdgeController>();
	}

	protected override Direction CalculateDirection(MapUpdateEvent mapUpdateEvent, CancellationToken ct)
	{
		var playerInfo = GetPlayerInfo(mapUpdateEvent.ReceivingPlayerId, mapUpdateEvent.Map.SnakeInfos);
		var playerDir = GetHeadDirection(playerInfo);
		var playerPos = GetHeadPosition(playerInfo);
		var directions = Enum.GetValues<Direction>()
			.Concat(Enum.GetValues<Direction>())
			.SkipWhile(dir => dir != playerDir)
			.Take(4)
			.Where(dir => CanMoveInDirection(playerPos, dir, mapUpdateEvent.Map));
		try
		{
			return directions.First(dir =>
				{
					logger.LogDebug($"Testing direction {dir}");
					return IsPositionAvailable(PositionInDirection(playerPos, dir, mapUpdateEvent.Map), mapUpdateEvent.Map);
				});
		}
		catch (InvalidOperationException)
		{
			logger.LogWarning("Could not find a possible direction, going up");
			return Direction.Up;
		}
	}

	private SnakeInfo GetPlayerInfo(Guid playerId, IEnumerable<SnakeInfo> snakeInfos)
	{
		try
		{
			return snakeInfos.First(si => si.Id == playerId);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, $"Could not find player info (playerId={playerId})");
			throw;
		}
	}

	private int GetHeadPosition(SnakeInfo snakeInfo) =>
		snakeInfo.Positions[0];

	private Direction GetHeadDirection(SnakeInfo snakeInfo) => snakeInfo.Positions switch
	{
		[int head, int prev, ..] when prev + 1 == head => Direction.Right,
		[int head, int prev, ..] when prev - 1 == head => Direction.Left,
		[int head, int prev, ..] when prev < head => Direction.Down,
		_ => Direction.Up,
	};

#pragma warning disable CS8524
	private bool CanMoveInDirection(int position, Direction direction, Map map) => direction switch
	{
		Direction.Up => position >= map.Width,
		Direction.Right => (position + 1) % map.Width != 0,
		Direction.Down => position < map.Width * (map.Height - 1),
		Direction.Left => position % map.Width != 0,
	};
#pragma warning restore CS8524

#pragma warning disable CS8524
	private int PositionInDirection(int position, Direction direction, Map map) => direction switch
	{
		Direction.Up => position - map.Width,
		Direction.Right => position + 1,
		Direction.Down => position + map.Width,
		Direction.Left => position - 1,
	};
#pragma warning restore CS8524

	private bool IsPositionAvailable(int position, Map map)
	{
		if (position < 0 || map.Width * map.Height <= position)
		{
			logger.LogDebug($"Position {position} is out of bounds");
			return false;
		}

		if (map.ObstaclePositions.Contains(position))
		{
			logger.LogDebug($"Position {position} is occupied by an obstacle");
			return false;
		}

		if (map.SnakeInfos.Any(si => si.Positions.Contains(position)))
		{
			logger.LogDebug($"Position {position} is occupied by a snake");
			return false;
		}

		logger.LogDebug($"Position {position} is available");
		return true;
	}
}
