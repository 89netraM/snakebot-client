using System;
using System.Collections.Generic;
using System.Linq;
using Cygni.Snake.Utils;

namespace Mårten.Snake.Utils;

public static class Analyze
{
	// private const int DangerZone = 5;

	public static bool AreConnected(this MapInfo mapInfo, Vector2 a, Vector2 b) =>
		BFS.Search(
			a,
			pos => pos.Neighbors.Select(p => p.pos).Where(mapInfo.IsOpen),
			pos => pos == b,
			out _);

	public static IEnumerable<Vector2> ViableNextPositions(this MapInfo mapInfo) =>
		mapInfo.ViableNextPositions(mapInfo.PlayerSnake);
	public static IEnumerable<Vector2> ViableNextPositions(this MapInfo mapInfo, Cygni.Snake.Utils.Snake snake)
	{
		var openPositions = new Stack<Vector2>(
			snake.Positions[0]
				.Neighbors
				.Select(p => p.pos)
				.Where(pos => mapInfo.IsOpenTo(snake.Id, pos) && mapInfo.PretendersTo(pos).All(id => id == snake.Id)));
		if (openPositions.Count == 0)
		{
			return Enumerable.Empty<Vector2>();
		}
		var groups = new Dictionary<Vector2, IList<Vector2>>();
		var first = openPositions.Pop();
		groups[first] = new List<Vector2> { first };
		while (openPositions.TryPop(out var pos))
		{
			if (!AddToExistingGroup(pos))
			{
				groups[pos] = new List<Vector2> { pos };
			}
		}
		return groups.MaxBy(g => mapInfo.ReachableArea(g.Key)).Value;

		bool AddToExistingGroup(Vector2 pos)
		{
			foreach (var group in groups!)
			{
				if (mapInfo.AreConnected(group.Key, pos))
				{
					group.Value.Add(pos);
				}
			}
			return false;
		}
	}

	public static int ReachableArea(this MapInfo mapInfo, Vector2 pos)
	{
		int area = 0;
		BFS.Search(
			pos,
			pos => pos.Neighbors.Select(p => p.pos).Where(mapInfo.IsOpen),
			_ => false,
			out _,
			_ => area++);
		return area;
	}

	public static ISet<Vector2> GoodSection(this MapInfo mapInfo)
	{
		var sections = mapInfo.Sections();
		return sections.Count > 0
			? sections.MaxBy(set => set.Count) ?? new HashSet<Vector2>()
			: new HashSet<Vector2>();
	}

	public static IList<ISet<Vector2>> Sections(this MapInfo mapInfo)
	{
		var sections = new List<ISet<Vector2>>();

		foreach (var pos in mapInfo.GoodPositions())
		{
			var added = false;
			foreach (var section in sections)
			{
				if (pos.Neighbors.Select(p => p.pos).Any(section.Contains))
				{
					section.Add(pos);
					added = true;
					break;
				}
			}
			if (!added)
			{
				sections.Add(new HashSet<Vector2> { pos });
			}
		}

	merge:
		foreach (var section in sections)
		{
			foreach (var otherSection in sections)
			{
				if (section != otherSection)
				{
					foreach (var pos in section)
					{
						if (pos.Neighbors.Select(p => p.pos).Any(otherSection.Contains))
						{
							section.UnionWith(otherSection);
							sections.Remove(otherSection);
							goto merge;
						}
					}
				}
			}
		}

		return sections;
	}

	public static ISet<Vector2> GoodPositions(this MapInfo mapInfo)
	{
		var count = new HashSet<Vector2>();

		for (int y = 0; y < mapInfo.Height; y++)
		{
			for (int x = 0; x < mapInfo.Width; x++)
			{
				var pos = new Vector2(x, y);
				if (mapInfo.IsOpen(pos) && mapInfo.OpenNeighbors(pos) > 2)// && /*PretendedByOthers(pos) &&*/ IsClosest(pos))
				{
					count.Add(pos);
				}
			}
		}

		return count;

		// bool PretendedByOthers(Vector2 pos) =>
		// 	pos.Neighbors
		// 		.Select(p => p.pos)
		// 		.All(p => !mapInfo.Snakes
		// 			.Values
		// 			.Where(si => si.Id != mapInfo.PlayerId && si.Positions.Count > 0)
		// 			.Select(si => si.Positions[0])
		// 			.Contains(p));

		// bool IsClosest(Vector2 pos)
		// {
		// 	var me = mapInfo.PlayerSnake.Positions[0];
		// 	var distToMe = pos.Distance(me);
		// 	var realDistToMe = new Lazy<int>(() => mapInfo.DistanceBetween(pos, me));
		// 	return mapInfo.Snakes
		// 		.Values
		// 		.Where(si => si.Id != mapInfo.PlayerId && si.Positions.Count > 0)
		// 		.Select(si => si.Positions[0])
		// 		.Select(o => (pos, d: pos.Distance(o)))
		// 		.All(p => p.d >= DangerZone || p.d >= distToMe || mapInfo.DistanceBetween(pos, p.pos) >= realDistToMe.Value);
		// }
	}

	// private static int DistanceBetween(this MapInfo mapInfo, Vector2 a, Vector2 b)
	// {
	// 	var found = BFS.Search(
	// 		a,
	// 		pos => pos.Neighbors
	// 			.Select(p => p.pos)
	// 			.Where(pos => a.Distance(pos) < DangerZone && mapInfo.IsOpen(pos)),
	// 		pos => pos == b,
	// 		out var path);
	// 	return found ? path.Count() : int.MaxValue;
	// }

	private static int OpenNeighbors(this MapInfo mapInfo, Vector2 pos)
	{
		var neighbors = pos.Neighbors.Select(p => p.pos).ToArray();
		var head = mapInfo.PlayerSnake.Positions[0];
		return neighbors.Count(p => mapInfo.IsOpen(p) || head == p) +
			neighbors.Zip(neighbors.Skip(1).Append(neighbors[0]))
				.Count(p => (mapInfo.IsOpen(p.First) || head == p.First) &&
					(mapInfo.IsOpen(p.Second) || head == p.Second));
	}
}
