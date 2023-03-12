using System;
using System.Collections.Generic;
using System.Linq;

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
			RouteCalculation();
		}
	}

	private void RouteCalculation()
	{
		var zero = new Vector2(0, 0);
		var pathFrom = HamiltonCycle(zero, new(Width, Height));
		var path = new List<Vector2> { zero, pathFrom[zero] };
		while (path[^1] != zero)
		{
			path.Add(pathFrom[path[^1]]);
		}
		Path = path;
	}

	private IDictionary<Vector2, Vector2> HamiltonCycle(Vector2 offset, Vector2 size)
	{
		if (size.X == 1 && size.Y == 1)
		{
			return new Dictionary<Vector2, Vector2> { [offset] = offset };
		}
		if (size.Y == 2 && size.X > 1)
		{
			var path = new Dictionary<Vector2, Vector2>();
			for (int x = 0; x < size.X - 1; x++)
			{
				path[new(offset.X + x, offset.Y)] = new(offset.X + x + 1, offset.Y);
			}
			path[new(offset.X + size.X - 1, offset.Y)] = new(offset.X + size.X - 1, offset.Y + 1);
			for (int x = size.X - 1; x > 0; x--)
			{
				path[new(offset.X + x, offset.Y + 1)] = new(offset.X + x - 1, offset.Y + 1);
			}
			path[new(offset.X, offset.Y + 1)] = new(offset.X, offset.Y);
			return path;
		}
		if (size.X == 2 && size.Y > 1)
		{
			var path = new Dictionary<Vector2, Vector2>();
			for (int y = 0; y < size.Y - 1; y++)
			{
				path[new(offset.X, offset.Y + y)] = new(offset.X, offset.Y + y + 1);
			}
			path[new(offset.X, offset.Y + size.Y - 1)] = new(offset.X + 1, offset.Y + size.Y - 1);
			for (int y = size.Y - 1; y > 0; y--)
			{
				path[new(offset.X + 1, offset.Y + y)] = new(offset.X + 1, offset.Y + y - 1);
			}
			path[new(offset.X + 1, offset.Y)] = new(offset.X, offset.Y);
			return path;
		}
		if (Int32.IsEvenInteger(size.Y) && size.X > 1)
		{
			var top = HamiltonCycle(offset, new(size.X, 2));
			var rest = HamiltonCycle(new(offset.X, offset.Y + 2), new(size.X, size.Y - 2));

			foreach (var (from, to) in top)
			{
				rest[from] = to;
			}

			rest[new(offset.X, offset.Y + 2)] = new(offset.X, offset.Y + 1);
			rest[new(offset.X + 1, offset.Y + 1)] = new(offset.X + 1, offset.Y + 2);

			return rest;
		}
		if (Int32.IsEvenInteger(size.X) && size.Y > 1)
		{
			var top = HamiltonCycle(offset, new(2, size.Y));
			var rest = HamiltonCycle(new(offset.X + 2, offset.Y), new(size.X - 2, size.Y));

			foreach (var (from, to) in top)
			{
				rest[from] = to;
			}

			rest[new(offset.X + 2, offset.Y)] = new(offset.X + 1, offset.Y);
			rest[new(offset.X + 1, offset.Y + 1)] = new(offset.X + 2, offset.Y + 1);

			return rest;
		}
		throw new Exception("Not Hamiltonian");
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
