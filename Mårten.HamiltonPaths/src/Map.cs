using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Mårten.HamiltonPaths;

public class Map
{
	public int Width { get; private set; } = 46;
	public int Height { get; private set; } = 34;
	public IEnumerable<Vector2> Path { get; private set; } = Enumerable.Empty<Vector2>();

	public Vector2? Start { get; set; } = null;
	public Vector2? End { get; set; } = null;

	public void CalculateRoute()
	{
		Path = Enumerable.Empty<Vector2>();
		if (Start is not null && End is not null)
		{
			Task.Run(RouteCalculation);
		}
	}

	private void RouteCalculation()
	{
		var pathTo = new Dictionary<Vector2, IImmutableList<Vector2>>();
		pathTo.Add(Start!, ImmutableList.Create<Vector2>(Start!));
		var toVisit = new Queue<Vector2>();
		toVisit.Enqueue(Start!);

		while (toVisit.TryDequeue(out var current))
		{
			var path = pathTo[current];
			if (current == End)
			{
				Path = path;
				return;
			}

			foreach (var next in current.Neighbors)
			{
				if (!pathTo.ContainsKey(next) && IsOpen(next))
				{
					pathTo[next] = path.Add(next);
					toVisit.Enqueue(next);
				}
			}
		}
	}

	public bool IsOpen(Vector2 pos)
	{
		if (pos.X < 0 || Width <= pos.X)
		{
			return false;
		}

		if (pos.Y < 0 || Height <= pos.Y)
		{
			return false;
		}

		return true;
	}
}
