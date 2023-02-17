using System;
using System.Collections.Generic;
using System.Linq;
using Cygni.Snake.Client.Messages;
using Cygni.Snake.Client.Models;

namespace Cygni.Snake.Utils;

public class MapInfo
{
	private readonly Map map;
	public GameSettings Settings { get; }
	public Guid GameId { get; }
	public Guid PlayerId { get; }
	public IReadOnlyDictionary<Vector2, ITileDescription> Tiles { get; }
	public IReadOnlyDictionary<Guid, Snake> Snakes { get; }

	public long WorldTick => map.WorldTick;
	public int Width => map.Width;
	public int Height => map.Height;

	public ITileDescription this[Vector2 pos] =>
		IsOnMap(pos)
			? Tiles.GetValueOrDefault(pos, EmptyTile.Instance)
			: OutOfBoundsTile.Instance;

	public Snake PlayerSnake => Snakes[PlayerId];

	public MapInfo(MapUpdateEvent mapUpdateEvent, GameSettings settings)
	{
		this.map = mapUpdateEvent.Map;
		GameId = mapUpdateEvent.GameId;
		PlayerId = mapUpdateEvent.ReceivingPlayerId;
		Settings = settings;
		Snakes = CalculateSnakes();
		Tiles = CalculateTiles();

		IReadOnlyDictionary<Guid, Snake> CalculateSnakes() =>
			map.SnakeInfos
				.Select(si => new Snake(si, this))
				.ToDictionary(s => s.Id, s => s);

		IReadOnlyDictionary<Vector2, ITileDescription> CalculateTiles()
		{
			var tiles = new Dictionary<Vector2, ITileDescription>();

			foreach (var foodIndex in map.FoodPositions)
			{
				tiles[IndexToPos(foodIndex)] = FoodTile.Instance;
			}

			foreach (var obstacleIndex in map.ObstaclePositions)
			{
				tiles[IndexToPos(obstacleIndex)] = ObstacleTile.Instance;
			}

			foreach (var (id, snake) in Snakes)
			{
				if (snake.Positions.Count > 0)
				{
					tiles[snake.Positions[0]] = new SnakeHeadTile(id, snake.Positions.Count > 1 ? snake.Positions[1] : null, snake.Positions[0]);
					for (int i = 1; i < snake.Positions.Count - 1; i++)
					{
						tiles[snake.Positions[i]] = new SnakeBodyTile(id, snake.Positions[i - 1], snake.Positions[i], snake.Positions[i + 1]);
					}
					if (snake.Positions.Count > 1)
					{
						tiles[snake.Positions[^1]] = new SnakeTailTile(id, snake.Positions[^1], snake.Positions[^2], snake.TailProtectedForGameTicks);
					}
				}
			}

			return tiles;
		}
	}

	public Vector2 IndexToPos(int index) =>
		new(index % Width, index / Width);

	public bool IsOnMap(Vector2 pos) =>
		0 <= pos.X && pos.X < Width &&
			0 <= pos.Y && pos.Y < Height;

	public IEnumerable<Guid> PretendersTo(Vector2 pos)
	{
		var neighbors = pos.Neighbors.Select(p => p.pos).ToHashSet();
		return map.SnakeInfos
			.Where(si => si.Positions.Count > 0 && neighbors.Contains(IndexToPos(si.Positions[0])))
			.Select(si => si.Id);
	}

	public bool CanSnakeMoveTo(Guid snakeId, Vector2 target) =>
		this[target].CanMoveTo(Settings.HeadToTailConsumes) &&
			Snakes[snakeId].Positions[0].Distance(target) == 1;
}
