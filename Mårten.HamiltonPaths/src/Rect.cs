using System;

namespace Mårten.HamiltonPaths;

public record Rect(Vector2 Offset, Vector2 Size)
{
	public int Area => Size.X * Size.Y;

	public bool IsIn(Vector2 point) =>
		Offset.X <= point.X && point.X < Offset.X + Size.X &&
			Offset.Y <= point.Y && point.Y < Offset.Y + Size.Y;

	public bool IsHamiltonian()
	{
		if (Size.X == 1 && Size.Y == 1)
		{
			return true;
		}
		if ((Size.X > 1 && Size.Y > 1) && (Int32.IsEvenInteger(Size.X) || Int32.IsEvenInteger(Size.Y)))
		{
			return true;
		}
		return false;
	}

	public override string ToString() =>
		$"[{Offset}, {Size}]";
}
