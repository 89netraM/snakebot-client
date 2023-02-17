using System;
using System.Collections.Generic;
using Cygni.Snake.Client.Messages;
using Cygni.Snake.Utils;
using Mårten.Snake.Services;
using SkiaSharp;
using Zarya.Silk.NET;
using Zarya.SkiaSharp;
using Zarya;
using System.Linq;

namespace Mårten.Snake.GameObjects;

public class MapObject : ISkiaSharpRenderable, IDisposable
{
	private static readonly SKColor BackgroundColor = new(0xFF11295A);
	private static readonly SKColor BorderColor = new(0xFF000000);

	private readonly MapInfoService mapInfoService;
	private readonly SilkWindow window;
	private readonly SkiaSharpRenderer renderer;

	private readonly IDictionary<Vector2, TileObject> tiles = new Dictionary<Vector2, TileObject>();
	private long lastUpdateTick = -1;
	private MapInfo? mapInfo;
	private int width = 0;
	private int height = 0;

	public Transform2D Transform { get; } = new Transform2D();

	public MapObject(MapInfoService mapInfoService, SilkWindow window, SkiaSharpRenderer renderer)
	{
		this.mapInfoService = mapInfoService;
		this.mapInfoService.OnMapInfoUpdateEvent += OnMapInfoUpdateEvent;

		this.window = window;
		this.window.Update += OnUpdate;

		this.renderer = renderer;
		this.renderer.AddRenderable(this);
	}

	private void OnMapInfoUpdateEvent(MapInfo mapInfo)
	{
		width = mapInfo.Width;
		height = mapInfo.Height;
		this.mapInfo = mapInfo;
	}

	private void OnUpdate(float deltaTime)
	{
		if (mapInfo is not null && lastUpdateTick != mapInfo.WorldTick)
		{
			UpdateTiles(mapInfo);
		}
	}

	private void UpdateTiles(MapInfo mapInfo)
	{
		var spares = new Stack<TileObject>();

		foreach (var (pos, tile) in tiles.ToArray())
		{
			if (mapInfo[pos] is EmptyTile)
			{
				spares.Push(tile);
				tiles.Remove(pos);
			}
		}

		foreach (var (pos, tileDescription) in mapInfo.Tiles)
		{
			if (!tiles.TryGetValue(pos, out var tile))
			{
				if (!spares.TryPop(out tile))
				{
					tile = window.Create<TileObject>()!;
				}
				tiles[pos] = tile;
			}
			tile.Tile = tileDescription;
		}


		foreach (var spare in spares)
		{
			window.Destroy(spare);
		}
	}

	public void Render(SKCanvas canvas)
	{
		RenderBackground(canvas);
		RenderTiles(canvas);
		RenderGrid(canvas);
	}

	private void RenderBackground(SKCanvas canvas)
	{
		var (pixelWidth, pixelHeight) = CalculateMapSize();
		using var paint = new SKPaint { Color = BackgroundColor };
		canvas.DrawRect(0, 0, pixelWidth, pixelHeight, paint);
	}

	private void RenderTiles(SKCanvas canvas)
	{
		float tileSize = CalculateTileSize();
		foreach (var (pos, tile) in tiles)
		{
			tile.Size = tileSize;
			tile.Transform.Position = new(pos.X * tileSize, pos.Y * tileSize);
		}
	}

	private void RenderGrid(SKCanvas canvas)
	{
		if (mapInfo is not null)
		{
			using var paint = new SKPaint { Color = BorderColor, IsStroke = true, IsAntialias = true };
			float tileSize = CalculateTileSize();
			for (int x = 1; x < width; x++)
				canvas.DrawLine(x * tileSize, 0, x * tileSize, height * tileSize, paint);
			for (int y = 1; y < width; y++)
				canvas.DrawLine(0, y * tileSize, width * tileSize, y * tileSize, paint);
		}
	}

	private (float, float) CalculateMapSize()
	{
		float tileSize = CalculateTileSize();
		if (tileSize is 0.0f or Single.NaN)
		{
			return (0.0f, 0.0f);
		}

		return (tileSize * width, tileSize * height);
	}

	private float CalculateTileSize()
	{
		if (width == 0)
		{
			return 0.0f;
		}

		return Single.MinNumber((window.Height ?? Single.NaN) / height, (window.Width ?? Single.NaN) / width);
	}

	public void Dispose()
	{
		renderer.RemoveRenderable(this);
		mapInfoService.OnMapInfoUpdateEvent -= OnMapInfoUpdateEvent;
		window.Update -= OnUpdate;
	}
}
