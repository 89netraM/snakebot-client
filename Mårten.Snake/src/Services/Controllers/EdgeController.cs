using System;
using System.Linq;
using System.Threading;
using Cygni.Snake.Client.Models;
using Cygni.Snake.Utils;
using Microsoft.Extensions.Logging;

namespace Mårten.Snake.Services.Controllers;

public class EdgeController : ControllerBase
{
	private readonly ILogger<EdgeController> logger;

	public EdgeController(MapInfoService mapInfoService, ILoggerFactory loggerFactory) : base(mapInfoService, loggerFactory)
	{
		logger = loggerFactory.CreateLogger<EdgeController>();
	}

	protected override Direction CalculateDirection(MapInfo mapInfo, CancellationToken ct)
	{
		try
		{
			return Enum.GetValues<Direction>()
				.Concat(Enum.GetValues<Direction>())
				.SkipWhile(dir => dir != mapInfo.PlayerSnake.HeadDirection)
				.Take(4)
				.First(dir => mapInfo.CanSnakeMoveTo(mapInfo.PlayerSnake.Id, mapInfo.PlayerSnake.Positions[0] + dir));
		}
		catch (InvalidOperationException)
		{
			logger.LogWarning("Could not find a possible direction, going up");
			return Direction.Up;
		}
	}
}
