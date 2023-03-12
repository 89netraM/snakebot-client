using System;
using System.Linq;
using SkiaSharp;
using Zarya;
using Zarya.Silk.NET;
using Zarya.SkiaSharp;

namespace Mårten.HamiltonPaths;

public class MapObject : ISkiaSharpRenderable, IDisposable
{
	private readonly SKColor BackgroundColor = new SKColor(0xFF11295A);
	private readonly SKColor BorderColor = new SKColor(0xFF000000);
	private readonly SKColor StartColor = new SKColor(0xFF00FF00);
	private readonly SKColor PathColor = new SKColor(0xFFFFFFFF);
	private readonly SKColor EndColor = new SKColor(0xFFFF0000);
	private readonly float PointRadius = 0.375f;

	private readonly SilkWindow window;
	private readonly SkiaSharpRenderer renderer;
	private readonly Map map;

	public Transform2D Transform { get; } = new();

	public MapObject(SilkWindow window, SkiaSharpRenderer renderer, Map map)
	{
		this.window = window;
		this.window.Update += OnUpdate;

		this.renderer = renderer;
		this.renderer.AddRenderable(this);

		this.map = map;
	}

	public void OnUpdate(float deltaTime)
	{
		if (window.GetMousePosition() is System.Numerics.Vector2 mousePos)
		{
			var tileSize = CalculateTileSize();
			var pos = new Vector2((int)MathF.Round(mousePos.X / tileSize - 0.5f), (int)MathF.Round(mousePos.Y / tileSize - 0.5f));
			if (window.IsMouseButtonDown(MouseButton.Left) && pos != map.End)
			{
				map.Start = map.Start != pos && map.IsOpen(pos) ? pos : null;
				map.CalculateRoute();
			}
			else if (window.IsMouseButtonDown(MouseButton.Right) && pos != map.Start)
			{
				map.End = map.End != pos && map.IsOpen(pos) ? pos : null;
				map.CalculateRoute();
			}
		}
	}

	public void Render(SKCanvas canvas)
	{
		var tileSize = CalculateTileSize();
		canvas.Save();
		canvas.Scale(tileSize);

		RenderBackground(canvas);
		RenderGrid(canvas);

		canvas.Translate(0.5f, 0.5f);
		RenderPath(canvas);
		RenderPoints(canvas);

		canvas.Restore();
	}

	private void RenderBackground(SKCanvas canvas)
	{
		using var paint = new SKPaint { Color = BackgroundColor };
		canvas.DrawRect(0, 0, map.Width, map.Height, paint);
	}

	private void RenderGrid(SKCanvas canvas)
	{
		using var paint = new SKPaint { Color = BorderColor, IsStroke = true, IsAntialias = true };
		for (int x = 1; x < map.Width; x++)
			canvas.DrawLine(x, 0, x, map.Height, paint);
		for (int y = 1; y < map.Height; y++)
			canvas.DrawLine(0, y, map.Width, y, paint);
	}

	private void RenderPath(SKCanvas canvas)
	{
		if (!map.Path.Any())
		{
			return;
		}

		var path = new SKPath();
		var startPos = map.Path.First();
		path.MoveTo(startPos.X, startPos.Y);
		foreach (var pos in map.Path.Skip(1))
		{
			path.LineTo(pos.X, pos.Y);
		}

		using var paint = new SKPaint { Color = PathColor, StrokeWidth = 0.1f, IsStroke = true, IsAntialias = true };
		canvas.DrawPath(path, paint);
	}

	private void RenderPoints(SKCanvas canvas)
	{
		using var borderPaint = new SKPaint { Color = BorderColor, IsStroke = true, IsAntialias = true };

		if (map.Start is Vector2 start)
		{
			using var paint = new SKPaint { Color = StartColor, IsAntialias = true };
			canvas.DrawCircle(start.X, start.Y, PointRadius, paint);
			canvas.DrawCircle(start.X, start.Y, PointRadius, borderPaint);
		}

		if (map.End is Vector2 end)
		{
			using var paint = new SKPaint { Color = EndColor, IsAntialias = true };
			canvas.DrawCircle(end.X, end.Y, PointRadius, paint);
			canvas.DrawCircle(end.X, end.Y, PointRadius, borderPaint);
		}
	}

	private float CalculateTileSize()
	{
		if (map.Width == 0)
		{
			return 0.0f;
		}

		return Single.MinNumber((window.Height ?? Single.NaN) / map.Height, (window.Width ?? Single.NaN) / map.Width);
	}

	public void Dispose()
	{
		window.Update -= OnUpdate;
		renderer.RemoveRenderable(this);
	}
}
