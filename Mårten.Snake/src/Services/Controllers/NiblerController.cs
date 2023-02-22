using System;
using System.Linq;
using System.Threading;
using Cygni.Snake.Client.Models;
using Cygni.Snake.Utils;
using Microsoft.Extensions.Logging;
using Mårten.Snake.Utils;

namespace Mårten.Snake.Services.Controllers;

public class NiblerController : FoodController
{	
	private readonly ILogger<NiblerController> logger;

	public NiblerController(MapInfoService mapInfoService, ILoggerFactory loggerFactory) : base(mapInfoService, loggerFactory)
	{
		logger = loggerFactory.CreateLogger<NiblerController>();
	}

	protected override Direction CalculateDirection(MapInfo mapInfo, CancellationToken ct)
	{
		var unviable = mapInfo.PlayerSnake
			.Positions[0]
			.Neighbors
			.Select(p => p.pos)
			.Except(mapInfo.ViableNextPositions())
			.ToHashSet();

		var tailFound = BFS.Search(
			mapInfo.PlayerSnake.Positions[0],
			pos => pos.Neighbors
				.Select(p => p.pos)
				.Where(pos => !unviable.Contains(pos) &&
					mapInfo.IsOpenTo(mapInfo.PlayerId, pos)),
			pos => mapInfo[pos] is SnakeTailTile(Guid otherId, _, _, long protectedFor) &&
				otherId != mapInfo.PlayerId &&
				protectedFor == 0,
			out var path);

		if (!tailFound)
		{
			return base.CalculateDirection(mapInfo, ct);
		}

		return Vector2.ReverseDirections[path.First() - mapInfo.PlayerSnake.Positions[0]];
	}
}
