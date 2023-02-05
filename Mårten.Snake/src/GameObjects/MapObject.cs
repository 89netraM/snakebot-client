using System;
using System.Collections.Generic;
using Cygni.Snake.Client.Messages;
using Cygni.Snake.Client.Models;
using Mårten.Snake.Models;
using Mårten.Snake.Services;
using SkiaSharp;
using Zarya.Silk.NET;
using Zarya.SkiaSharp;
using Zarya;

namespace Mårten.Snake.GameObjects;

public class MapObject : ISkiaSharpRenderable, IDisposable
{
	private static readonly SKColor BackgroundColor = new(0xFF11295A);

	private readonly ClientService client;
	private readonly SilkWindow window;
	private readonly SkiaSharpRenderer renderer;

	private readonly IList<TileObject> tiles = new List<TileObject>();
	private Map? map;
	private int width = 0;

	public Transform2D Transform { get; } = new Transform2D();

	public MapObject(ClientService client, SilkWindow window, SkiaSharpRenderer renderer)
	{
		this.client = client;
		this.client.OnMapUpdateEvent += OnMapUpdateEvent;

		this.window = window;
		this.window.Update += OnUpdate;

		this.renderer = renderer;
		this.renderer.AddRenderable(this);
	}

	private void OnMapUpdateEvent(MapUpdateEvent mapUpdateEvent)
	{
		map = mapUpdateEvent.Map;
		width = map.Width;
	}

	private void OnUpdate(float deltaTime)
	{
		if (map is not null)
		{
			UpdateTiles();
		}
	}

	private void UpdateTiles()
	{
		var tileCount = map!.Width * map.Height;

		while (tiles.Count < tileCount)
		{
			tiles.Add(window.Create<TileObject>()!);
		}
		while (tiles.Count > tileCount)
		{
			window.Destroy(tiles[^1]);
			tiles.RemoveAt(tiles.Count - 1);
		}

		foreach (var tile in tiles)
		{
			tile.Tile = EmptyTile.Instance;
		}
		foreach (var foodPosition in map.FoodPositions)
		{
			tiles[foodPosition].Tile = FoodTile.Instance;
		}
		foreach (var obstaclePosition in map.ObstaclePositions)
		{
			tiles[obstaclePosition].Tile = ObstacleTile.Instance;
		}
		foreach (var snake in map.SnakeInfos)
		{
			if (snake.Positions.Count > 0)
			{
				tiles[snake.Positions[0]].Tile = new SnakeHeadTile(snake.Id, snake.Positions.Count > 1 ? snake.Positions[1] : null, snake.Positions[0]);
				for (int i = 1; i < snake.Positions.Count - 1; i++)
				{
					tiles[snake.Positions[i]].Tile = new SnakeBodyTile(snake.Id, snake.Positions[i - 1], snake.Positions[i], snake.Positions[i + 1]);
				}
				if (snake.Positions.Count > 1)
				{
					tiles[snake.Positions[^1]].Tile = new SnakeTailTile(snake.Id, snake.Positions[^1], snake.Positions[^2]);
				}
			}
		}
	}

	public void Render(SKCanvas canvas)
	{
		var (pixelWidth, pixelHeight) = CalculateMapSize();
		float tileSize = CalculateTileSize();

		using var paint = new SKPaint { Color = BackgroundColor };
		canvas.DrawRect(0, 0, pixelWidth, pixelHeight, paint);

		for (int i = 0; i < tiles.Count; i++)
		{
			tiles[i].Size = tileSize;
			tiles[i].Transform.Position = new(tileSize * (i % width), tileSize * (i / width));
		}
	}

	private (float, float) CalculateMapSize()
	{
		float tileSize = CalculateTileSize();
		if (tileSize is 0.0f or Single.NaN)
		{
			return (0.0f, 0.0f);
		}

		return (tileSize * width, tileSize * (tiles.Count / width));
	}

	private float CalculateTileSize()
	{
		if (width == 0)
		{
			return 0.0f;
		}

		int height = tiles.Count / width;
		return Single.MinNumber((window.Height ?? Single.NaN) / height, (window.Width ?? Single.NaN) / width);
	}

	public void Dispose()
	{
		renderer.RemoveRenderable(this);
		client.OnMapUpdateEvent -= OnMapUpdateEvent;
		window.Update -= OnUpdate;
	}
}
