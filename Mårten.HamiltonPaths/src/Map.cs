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
		var section = StripOff(new(new(0, 0), new(Width, Height)));
		if (section is null)
		{
			return;
		}
		var pathFrom = HamiltonCycle(section);
		var (first, second) = pathFrom.First();
		var path = new List<Vector2> { first, second };
		while (path[^1] != first)
		{
			path.Add(pathFrom[path[^1]]);
		}
		Path = path;
	}

	private Rect? StripOff(Rect rect)
	{
		var points = new List<Vector2>();
		if (rect.IsIn(Start!))
		{
			points.Add(Start!);
		}
		if (rect.IsIn(End!))
		{
			points.Add(End!);
		}

		var rects = new List<Rect>();

		var leftWidth = points.Append(rect.Offset + rect.Size).Min(p => p.X) - rect.Offset.X;
		Vector2 leftSize;
		if (Int32.IsEvenInteger(leftWidth) || Int32.IsEvenInteger(rect.Size.Y))
		{
			leftSize = new(leftWidth, rect.Size.Y);
		}
		else
		{
			leftSize = new(leftWidth - 1, rect.Size.Y);
		}
		rects.Add(new(rect.Offset, leftSize));

		var topHeight = points.Append(rect.Offset + rect.Size).Min(p => p.Y) - rect.Offset.Y;
		Vector2 topSize;
		if (Int32.IsEvenInteger(topHeight) || Int32.IsEvenInteger(rect.Size.X))
		{
			topSize = new(rect.Size.X, topHeight);
		}
		else
		{
			topSize = new(rect.Size.X, topHeight - 1);
		}
		rects.Add(new(rect.Offset, topSize));

		var rightPoint = points.Append(rect.Offset).Max(p => p.X) + 1;
		var rightWidth = rect.Size.X - (rightPoint - rect.Offset.X);
		Rect right;
		if (Int32.IsEvenInteger(rightWidth) || Int32.IsEvenInteger(rect.Size.Y))
		{
			right = new(new(rightPoint, rect.Offset.Y), new(rightWidth, rect.Size.Y));
		}
		else
		{
			right = new(new(rightPoint + 1, rect.Offset.Y), new(rightWidth - 1, rect.Size.Y));
		}
		rects.Add(right);

		var bottomPoint = points.Append(rect.Offset).Max(p => p.Y) + 1;
		var bottomHeight = rect.Size.Y - (bottomPoint - rect.Offset.Y);
		Rect bottom;
		if (Int32.IsEvenInteger(bottomHeight) || Int32.IsEvenInteger(rect.Size.X))
		{
			bottom = new(new(rect.Offset.X, bottomPoint), new(rect.Size.X, bottomHeight));
		}
		else
		{
			bottom = new(new(rect.Offset.X, bottomPoint + 1), new(rect.Size.X, bottomHeight - 1));
		}
		rects.Add(bottom);

		return rects
			.Where(r => r.IsHamiltonian())
			.MaxBy(r => r.Area);
	}

	private IDictionary<Vector2, Vector2> HamiltonCycle(Rect rect)
	{
		var (offset, size) = rect;
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
			var top = HamiltonCycle(new(offset, new(size.X, 2)));
			var rest = HamiltonCycle(new(new(offset.X, offset.Y + 2), new(size.X, size.Y - 2)));

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
			var top = HamiltonCycle(new(offset, new(2, size.Y)));
			var rest = HamiltonCycle(new(new(offset.X + 2, offset.Y), new(size.X - 2, size.Y)));

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
