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
		if (head.Neighbors.Select(p => p.pos).Any(goodSection.Contains))
		{
			var foundPath = BFS.Search(
				head,
				pos => pos.Neighbors
					.Select(p => p.pos)
					.Where(p => goodSection.Contains(p) && mapInfo.PretendersTo(p).All(id => id == mapInfo.PlayerId)),
				pos => pos != head &&
					pos.MooreNeighbors
						.Any(pos => !mapInfo.IsOpenTo(mapInfo.PlayerId, pos) &&
							!(mapInfo[pos] is SnakeTile(Guid id, _) && id == mapInfo.PlayerId)),
				out var path);
			
			if (foundPath)
			{
				logger.LogInformation("Gooing to edge");
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
							if (path.Count() >= pathToSection.Count())
							{
								continue;
							}
						}
						pathToSections[section.Count] = path;
						break;
					}
				}
			});

		if (pathToSections.Any())
		{
			var bestPath = pathToSections.MaxBy(kvp => kvp.Key).Value;
			logger.LogInformation("Taking shortest to good");
			return Vector2.ReverseDirections[bestPath.First() - head];
		}

		var avaliablePaths = new List<IEnumerable<Vector2>>();
		BFS.Search(
			head,
			pos => pos.Neighbors
				.Select(p => p.pos)
				.Where(mapInfo.IsOpen),
			_ => false,
			out _,
			path =>
			{
				avaliablePaths.Add(path);
			});

		var longestPath = avaliablePaths.MaxBy(path => path.Count())!;
		logger.LogInformation("Taking longest path");
		return Vector2.ReverseDirections[longestPath.First() - head];
	}
}
