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
		var pathFrom = LongestPath(new(new(0, 0), new(Width, Height)), Start!, End!);

		var pos = FindBeginning(pathFrom);
		if (pos is null)
		{
			return;
		}
		var path = new List<Vector2> { pos };
		while (pathFrom.TryGetValue(pos, out pos))
		{
			path.Add(pos);
		}
		Path = path;
	}

	private Vector2? FindBeginning(IDictionary<Vector2, Vector2> pathFrom)
	{
		var tos = pathFrom.Values.ToHashSet();
		foreach (var from in pathFrom.Keys)
		{
			if (!tos.Contains(from))
			{
				return from;
			}
		}
		return null;
	}

	private IDictionary<Vector2, Vector2> LongestPath(Rect rect, Vector2 start, Vector2 end)
	{
		Console.WriteLine($"{rect}, {start}, {end}");
		if (rect.Size.X == 1 || rect.Size.Y == 1)
		{
			return Rect1Path(start, end);
		}
		if (rect.Size.X == 2 || rect.Size.Y == 2)
		{
			return Rect2Path(rect, start, end);
		}
		return new Dictionary<Vector2, Vector2>();
	}

	private IDictionary<Vector2, Vector2> Rect1Path(Vector2 start, Vector2 end)
	{
		var diff = end - start;
		var step = new Vector2(Int32.Sign(diff.X), Int32.Sign(diff.Y));
		var path = new Dictionary<Vector2, Vector2>();
		for (var pos = start; pos != end; pos += step)
		{
			path[pos] = pos + step;
		}
		return path;
	}

	private IDictionary<Vector2, Vector2> Rect2Path(Rect rect, Vector2 start, Vector2 end)
	{
		var (offset, size) = rect;
		if (size.Y == 2)
		{
			if (start.Y == end.Y)
			{
				var path = new Dictionary<Vector2, Vector2>();
				var step = new Vector2(Int32.Sign(start.X - end.X), 0);
				var pos = start;
				for (; rect.IsIn(pos + step); pos += step)
				{
					path[pos] = pos + step;
				}
				path[pos] = pos = new(pos.X, (pos.Y - offset.Y + 1) % size.Y + offset.Y);
				step = -step;
				for (; pos.X != start.X; pos += step)
				{
					path[pos] = pos + step;
				}
				path[pos] = pos + step;

				var centerDistance = Int32.Abs(end.X - start.X);
				if (centerDistance == 2)
				{
					pos += step;
					path[pos] = pos + step;
				}
				else if (centerDistance > 2)
				{
					var innerStart = pos + step;
					var innerEnd = new Vector2(end.X - step.X, pos.Y);
					var centerRect = new Rect(new(Int32.Min(innerStart.X, innerEnd.X), offset.Y), new(Int32.Abs(innerEnd.X - innerStart.X), size.Y));
					var centerPath = Rect2Path(centerRect, innerStart, innerEnd);
					path.Union(centerPath);

					path[innerEnd] = innerEnd + step;
				}

				for (pos = new(end.X, pos.Y); rect.IsIn(pos + step); pos += step)
				{
					path[pos] = pos + step;
				}
				path[pos] = pos = new(pos.X, (pos.Y - offset.Y + 1) % size.Y + offset.Y);
				step = -step;
				for (; pos.X != end.X; pos += step)
				{
					path[pos] = pos + step;
				}

				return path;
			}
			else if (Int32.Abs(end.X - start.X) <= 1)
			{
				var leftLength = start.X - offset.X + end.X - offset.X + 1;
				var rightLength = size.X - (start.X - offset.X + 1) + size.X - (end.X - offset.X + 1) + 1;
				if (rightLength > leftLength)
				{
					var path = new Dictionary<Vector2, Vector2>();
					for (int x = start.X; x < offset.X + size.X - 1; x++)
					{
						path[new(x, start.Y)] = new(x + 1, start.Y);
					}
					path[new(offset.X + size.X - 1, start.Y)] = new(offset.X + size.X - 1, end.Y);
					for (int x = offset.X + size.X - 1; x > end.X; x--)
					{
						path[new(x, end.Y)] = new(x - 1, end.Y);
					}
					return path;
				}
				else
				{
					var path = new Dictionary<Vector2, Vector2>();
					for (int x = start.X; x > offset.X; x--)
					{
						path[new(x, start.Y)] = new(x - 1, start.Y);
					}
					path[new(offset.X, start.Y)] = new(offset.X, end.Y);
					for (int x = offset.X; x < end.X; x++)
					{
						path[new(x, end.Y)] = new(x + 1, end.Y);
					}
					return path;
				}
			}
			else
			{
				var startFlip = new Vector2(start.X, end.Y);
				var endFlip = new Vector2(end.X, start.Y);

				Rect startRect;
				if (start.X < end.X)
				{
					startRect = new(offset, new(start.X - offset.X + 1, size.Y));
				}
				else
				{
					startRect = new(new(start.X, offset.Y), new(size.X - (start.X - offset.X), size.Y));
				}
				var path = Rect2Path(startRect, start, startFlip);

				var step = new Vector2(Int32.Sign(end.X - start.X), 0);
				path[startFlip] = startFlip + step;

				var centerDistance = Int32.Abs(end.X - start.X);
				if (centerDistance == 2)
				{
					path[startFlip + step] = endFlip - step;
					path[endFlip - step] = endFlip;
				}
				else if (centerDistance > 2)
				{
					var innerStart = startFlip + step;
					var innerEnd = endFlip - step;
					var centerRect = new Rect(new(Int32.Min(innerStart.X, innerEnd.X), offset.Y), new(Int32.Abs(innerEnd.X - innerStart.X) + 1, size.Y));
					var centerPath = Rect2Path(centerRect, innerStart, innerEnd);
					path.Union(centerPath);

					path[innerEnd] = innerEnd + step;
				}

				Rect endRect;
				if (end.X < start.X)
				{
					endRect = new(offset, new(end.X - offset.X + 1, size.Y));
				}
				else
				{
					endRect = new(new(end.X, offset.Y), new(size.X - (end.X - offset.X), size.Y));
				}
				path.Union(Rect2Path(endRect, endFlip, end));

				return path;
			}
		}
		else
		{
			if (start.X == end.X)
			{
				var path = new Dictionary<Vector2, Vector2>();
				var step = new Vector2(0, Int32.Sign(start.Y - end.Y));
				var pos = start;
				for (; rect.IsIn(pos + step); pos += step)
				{
					path[pos] = pos + step;
				}
				path[pos] = pos = new((pos.X - offset.X + 1) % size.X + offset.X, pos.Y);
				step = -step;
				for (; pos.Y != start.Y; pos += step)
				{
					path[pos] = pos + step;
				}
				path[pos] = pos + step;

				var centerDistance = Int32.Abs(end.Y - start.Y);
				if (centerDistance == 2)
				{
					pos += step;
					path[pos] = pos + step;
				}
				else if (centerDistance > 2)
				{
					var innerStart = pos + step;
					var innerEnd = new Vector2(pos.X, end.Y - step.Y);
					var centerRect = new Rect(new(offset.X, Int32.Min(innerStart.Y, innerEnd.Y)), new(size.X, Int32.Abs(innerEnd.Y - innerStart.Y)));
					var centerPath = Rect2Path(centerRect, innerStart, innerEnd);
					path.Union(centerPath);

					path[innerEnd] = innerEnd + step;
				}

				for (pos = new(pos.X, end.Y); rect.IsIn(pos + step); pos += step)
				{
					path[pos] = pos + step;
				}
				path[pos] = pos = new((pos.X - offset.X + 1) % size.X + offset.X, pos.Y);
				step = -step;
				for (; pos.Y != end.Y; pos += step)
				{
					path[pos] = pos + step;
				}

				return path;
			}
			else if (Int32.Abs(end.Y - start.Y) <= 1)
			{
				var topLength = start.Y - offset.Y + end.Y - offset.Y + 1;
				var bottomLength = size.Y - (start.Y - offset.Y + 1) + size.Y - (end.Y - offset.Y + 1) + 1;
				if (bottomLength > topLength)
				{
					var path = new Dictionary<Vector2, Vector2>();
					for (int y = start.Y; y < offset.Y + size.Y - 1; y++)
					{
						path[new(start.X, y)] = new(start.X, y + 1);
					}
					path[new(start.X, offset.Y + size.Y - 1)] = new(end.X, offset.Y + size.Y - 1);
					for (int y = offset.Y + size.Y - 1; y > end.Y; y--)
					{
						path[new(end.X, y)] = new(end.X, y - 1);
					}
					return path;
				}
				else
				{
					var path = new Dictionary<Vector2, Vector2>();
					for (int y = start.Y; y > offset.Y; y--)
					{
						path[new(start.X, y)] = new(start.X, y - 1);
					}
					path[new(start.X, offset.Y)] = new(end.X, offset.Y);
					for (int y = offset.Y; y < end.Y; y++)
					{
						path[new(end.X, y)] = new(end.X, y + 1);
					}
					return path;
				}
			}
			else
			{
				var startFlip = new Vector2(end.X, start.Y);
				var endFlip = new Vector2(start.X, end.Y);

				Rect startRect;
				if (start.Y < end.Y)
				{
					startRect = new(offset, new(size.X, start.Y - offset.Y + 1));
				}
				else
				{
					startRect = new(new(offset.X, start.Y), new(size.X, size.Y - (start.Y - offset.Y)));
				}
				var path = Rect2Path(startRect, start, startFlip);

				var step = new Vector2(0, Int32.Sign(end.Y - start.Y));
				path[startFlip] = startFlip + step;

				var centerDistance = Int32.Abs(end.Y - start.Y);
				if (centerDistance == 2)
				{
					path[startFlip + step] = endFlip - step;
					path[endFlip - step] = endFlip;
				}
				else if (centerDistance > 2)
				{
					var innerStart = startFlip + step;
					var innerEnd = endFlip - step;
					var centerRect = new Rect(new(offset.X, Int32.Min(innerStart.Y, innerEnd.Y)), new(size.X, Int32.Abs(innerEnd.Y - innerStart.Y) + 1));
					var centerPath = Rect2Path(centerRect, innerStart, innerEnd);
					path.Union(centerPath);

					path[innerEnd] = innerEnd + step;
				}

				Rect endRect;
				if (end.Y < start.Y)
				{
					endRect = new(offset, new(size.X, end.Y - offset.Y + 1));
				}
				else
				{
					endRect = new(new(offset.X, end.Y), new(size.X, size.Y - (end.Y - offset.Y)));
				}
				path.Union(Rect2Path(endRect, endFlip, end));

				return path;
			}
		}
	}

	private Rect? StripOff(Rect rect, Vector2 start, Vector2 end)
	{
		var points = new List<Vector2>();
		if (rect.IsIn(start))
		{
			points.Add(start);
		}
		if (rect.IsIn(end))
		{
			points.Add(end);
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

			rest.Union(top);
			rest[new(offset.X, offset.Y + 2)] = new(offset.X, offset.Y + 1);
			rest[new(offset.X + 1, offset.Y + 1)] = new(offset.X + 1, offset.Y + 2);

			return rest;
		}
		if (Int32.IsEvenInteger(size.X) && size.Y > 1)
		{
			var left = HamiltonCycle(new(offset, new(2, size.Y)));
			var rest = HamiltonCycle(new(new(offset.X + 2, offset.Y), new(size.X - 2, size.Y)));

			rest.Union(left);
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
