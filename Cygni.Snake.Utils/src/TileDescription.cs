using System;

namespace Cygni.Snake.Utils;

public interface ITileDescription
{
	bool CanMoveTo(bool canEatTails);
}

public record OutOfBoundsTile() : ITileDescription
{
	public static OutOfBoundsTile Instance = new OutOfBoundsTile();

	public bool CanMoveTo(bool canEatTails) => false;
}

public record EmptyTile() : ITileDescription
{
	public static EmptyTile Instance = new EmptyTile();

	public bool CanMoveTo(bool canEatTails) => true;
}

public record ObstacleTile() : ITileDescription
{
	public static ObstacleTile Instance = new ObstacleTile();

	public bool CanMoveTo(bool canEatTails) => false;
}

public record FoodTile() : ITileDescription
{
	public static FoodTile Instance = new FoodTile();

	public bool CanMoveTo(bool canEatTails) => true;
}

public record SnakeHeadTile(Guid Id, Vector2? Previous, Vector2 Current) : ITileDescription
{
	public bool CanMoveTo(bool canEatTails) => false;
}
public record SnakeBodyTile(Guid Id, Vector2 Previous, Vector2 Current, Vector2 Next) : ITileDescription
{
	public bool CanMoveTo(bool canEatTails) => false;
}
public record SnakeTailTile(Guid Id, Vector2 Current, Vector2 Next, long ProtectedForTicks) : ITileDescription
{
	public bool CanMoveTo(bool canEatTails) => canEatTails && ProtectedForTicks == 0;
}
