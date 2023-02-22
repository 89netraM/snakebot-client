using System.Linq;
using System.Threading;
using Cygni.Snake.Client.Models;
using Cygni.Snake.Utils;
using Microsoft.Extensions.Logging;
using Mårten.Snake.Utils;
using System;
using System.Collections.Generic;

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
		var head = mapInfo.PlayerSnake.Positions[0];

		var sections = mapInfo.Sections();

		var goodSection = sections.MaxBy(s => s.Count)!;
		// if (goodSection.Contains(head))
		if (head.Neighbors.Select(p => p.pos).Any(goodSection.Contains))
		{
			var foundPath = BFS.Search(
				head,
				pos => pos.Neighbors
					.Select(p => p.pos)
					.Where(p => goodSection.Contains(p) && mapInfo.PretendersTo(p).All(id => id == mapInfo.PlayerId)),
				pos => pos != head &&
					pos.Neighbors
						.Select(p => p.pos)
						.Any(pos => !mapInfo.IsOpenTo(mapInfo.PlayerId, pos) &&
							!(mapInfo[pos] is SnakeTile(Guid id, _) && id == mapInfo.PlayerId)),
				out var path);
			
			if (!foundPath)
			{
				logger.LogWarning("Could not find a good path to the edge");
			}
			else
			{
				return Vector2.ReverseDirections[path.First() - head];
			}
		}

		var pathToSections = new Dictionary<int, IEnumerable<Vector2>>();

		BFS.Search(
			head,
			pos => pos.Neighbors
				.Select(p => p.pos)
				.Where(p => mapInfo.IsOpenTo(mapInfo.PlayerId, p)),
			_ => false,
			out _,
			path =>
			{
				var end = path.Last();
				foreach (var section in sections)
				{
					if (section.Contains(end))
					{
						if (pathToSections.TryGetValue(section.Count, out var pathToSection))
						{
							if (pathToSections.Count() >= pathToSection.Count())
							{
								continue;
							}
						}
						pathToSections[section.Count] = path;
						break;
					}
				}
			});

		var bestPath = pathToSections.MaxBy(kvp => kvp.Key).Value;
		return Vector2.ReverseDirections[bestPath.First() - head];
	}
}
