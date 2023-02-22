using System;

namespace Cygni.Snake.Utils;

public interface ITileDescription { }

public record OutOfBoundsTile() : ITileDescription
{
	public static OutOfBoundsTile Instance = new OutOfBoundsTile();
}

public record EmptyTile() : ITileDescription
{
	public static EmptyTile Instance = new EmptyTile();
}

public record ObstacleTile() : ITileDescription
{
	public static ObstacleTile Instance = new ObstacleTile();
}

public record FoodTile() : ITileDescription
{
	public static FoodTile Instance = new FoodTile();
}

public record SnakeTile(Guid Id, Vector2 Current) : ITileDescription;
public record SnakeHeadTile(Guid Id, Vector2? Previous, Vector2 Current) : SnakeTile(Id, Current);
public record SnakeBodyTile(Guid Id, Vector2 Previous, Vector2 Current, Vector2 Next) : SnakeTile(Id, Current);
public record SnakeTailTile(Guid Id, Vector2 Current, Vector2 Next, long ProtectedForTicks) : SnakeTile(Id, Current);
