using System;
using System.Collections.Generic;
using System.Linq;
using Cygni.Snake.Client.Models;

namespace Cygni.Snake.Utils;

public record Vector2(int X, int Y)
{
	public static IReadOnlyDictionary<Direction, Vector2> Directions = new Dictionary<Direction, Vector2>
	{
		[Direction.Up] = new(0, -1),
		[Direction.Right] = new(1, 0),
		[Direction.Down] = new(0, 1),
		[Direction.Left] = new(-1, 0),
	};
	public static IReadOnlyDictionary<Vector2, Direction> ReverseDirections = new Dictionary<Vector2, Direction>
	{
		[new(0, -1)] = Direction.Up,
		[new(1, 0)] = Direction.Right,
		[new(0, 1)] = Direction.Down,
		[new(-1, 0)] = Direction.Left,
	};

	public int Length => (int)Math.Sqrt(X * X + Y * Y);

	public IEnumerable<(Direction dir, Vector2 pos)> Neighbors =>
		Directions.Select(d => (d.Key, this + d.Value));

	public int Distance(Vector2 other) =>
		(other - this).Length;

	public static Vector2 operator +(Vector2 a, Vector2 b) =>
		new(a.X + b.X, a.Y + b.Y);

	public static Vector2 operator +(Vector2 a, Direction dir) =>
		a + Directions[dir];

	public static Vector2 operator +(Direction dir, Vector2 b) =>
		Directions[dir] + b;

	public static Vector2 operator -(Vector2 a, Vector2 b) =>
		new(a.X - b.X, a.Y - b.Y);

	public static Vector2 operator -(Vector2 a, Direction dir) =>
		a - Directions[dir];

	public static Vector2 operator -(Direction dir, Vector2 b) =>
		Directions[dir] - b;

	public static Vector2 operator -(Vector2 a) =>
		new(-a.X, -a.Y);

	public static implicit operator Vector2((int x, int y) pair) =>
		new(pair.x, pair.y);
}
