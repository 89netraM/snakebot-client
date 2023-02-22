using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cygni.Snake.Client.Models;
using Cygni.Snake.Utils;
using Mårten.Snake.Utils;
using Microsoft.Extensions.Logging;

namespace Mårten.Snake.Services.Controllers;

public class FoodController : EdgeController
{
	private readonly ILogger<FoodController> logger;

	public FoodController(
		MapInfoService mapInfoService,
		ILoggerFactory loggerFactory)
		: base(mapInfoService, loggerFactory)
	{
		logger = loggerFactory.CreateLogger<FoodController>();
	}

	protected override Direction CalculateDirection(MapInfo mapInfo, CancellationToken ct)
	{
		var head = mapInfo.PlayerSnake.Positions[0];

		var sections = mapInfo.Sections();

		var goodSection = sections.MaxBy(s => s.Count)!;
		// if (goodSection.Contains(head))
		if (head.Neighbors.Select(p => p.pos).Any(goodSection.Contains))
		{
			var foodFound = BFS.Search(
				head,
				pos => pos.Neighbors
					.Select(p => p.pos)
					.Where(p => goodSection.Contains(p) && mapInfo.PretendersTo(p).All(id => id == mapInfo.PlayerId)),
				pos => mapInfo[pos] is FoodTile,
				out var path);

			if (!foodFound)
			{
				logger.LogWarning("Could not find a good path to food");
			}
			else
			{
				return Vector2.ReverseDirections[path.First() - head];
			}
		}

		return base.CalculateDirection(mapInfo, ct);
	}
}
