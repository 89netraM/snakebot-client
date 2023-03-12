using System;
using System.Collections.Generic;

namespace Mårten.HamiltonPaths;

public record Vector2(int X, int Y)
{
	public int Length => (int)Math.Sqrt(X * X + Y * Y);

	public int ManhattanLength => Math.Abs(X) + Math.Abs(Y);

	public IEnumerable<Vector2> Neighbors => new[]
	{
		this + new Vector2(0, -1),
		this + new Vector2(1, 0),
		this + new Vector2(0, 1),
		this + new Vector2(-1, 0),
	};

	public int Distance(Vector2 other) =>
		(other - this).Length;
	
	public int ManhattanDistance(Vector2 other) =>
		(other - this).ManhattanLength;

	public static Vector2 operator +(Vector2 a, Vector2 b) =>
		new(a.X + b.X, a.Y + b.Y);

	public static Vector2 operator -(Vector2 a, Vector2 b) =>
		new(a.X - b.X, a.Y - b.Y);

	public static Vector2 operator -(Vector2 a) =>
		new(-a.X, -a.Y);

	public static implicit operator Vector2((int x, int y) pair) =>
		new(pair.x, pair.y);

	public override string ToString() =>
		$"({X}, {Y})";
}
