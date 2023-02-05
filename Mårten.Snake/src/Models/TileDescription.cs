using System;

namespace Mårten.Snake.Models;

public interface ITileDescription { }

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

public record SnakeHeadTile(Guid Id, int? Previous, int Current) : ITileDescription;
public record SnakeBodyTile(Guid Id, int Previous, int Current, int Next) : ITileDescription;
public record SnakeTailTile(Guid Id, int Current, int Next) : ITileDescription;
