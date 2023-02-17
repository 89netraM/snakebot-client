using System;
using Cygni.Snake.Client.Models;
using Cygni.Snake.Utils;
using Mårten.Snake.Services;
using SkiaSharp;
using Zarya;
using Zarya.SkiaSharp;

namespace Mårten.Snake.GameObjects;

public class TileObject : ISkiaSharpRenderable, IDisposable
{
	private static readonly SKColor ObstacleColor = new(0xFF000000);
	private static readonly string FoodPath = "Mårten.Snake.assets.star.png";

	public Transform2D Transform { get; } = new();
	public float Size { get; set; }
	public ITileDescription Tile { get; set; } = EmptyTile.Instance;

	private readonly SkiaSharpRenderer renderer;
	private readonly SnakeColor snakeColor;
	private readonly SpritePool spritePool;

	public TileObject(SkiaSharpRenderer renderer, SnakeColor snakeColor, SpritePool spritePool)
	{
		this.renderer = renderer;
		this.renderer.AddRenderable(this);

		this.snakeColor = snakeColor;

		this.spritePool = spritePool;
	}

	public void Render(SKCanvas canvas)
	{
		RenderIcon(canvas);
	}

	public void RenderIcon(SKCanvas canvas)
	{
		switch (Tile)
		{
			case ObstacleTile:
				RenderObstacle(canvas);
				break;
			case FoodTile:
				RenderFood(canvas);
				break;
			case SnakeHeadTile tile:
				RenderHead(canvas, tile);
				break;
			case SnakeBodyTile tile:
				RenderBody(canvas, tile);
				break;
			case SnakeTailTile tile:
				RenderTail(canvas, tile);
				break;
		}
	}

	private void RenderObstacle(SKCanvas canvas)
	{
		using var paint = new SKPaint { Color = ObstacleColor };
		canvas.DrawRect(0, 0, Size, Size, paint);
	}

	private void RenderFood(SKCanvas canvas)
	{
		var foodSprite = spritePool[FoodPath];
		canvas.DrawBitmap(foodSprite, new SKRect(0, 0, Size, Size));
	}

	private void RenderHead(SKCanvas canvas, SnakeHeadTile tile)
	{
		canvas.Save();
		canvas.Scale(Size);
		if (tile.Previous is Vector2 prev)
		{
			switch (prev - tile.Current)
			{
				case (1, 0):
					canvas.RotateRadians(-MathF.PI / 2, 0.5f, 0.5f);
					break;
				case (-1, 0):
					canvas.RotateRadians(MathF.PI / 2, 0.5f, 0.5f);
					break;
				case (0, -1):
					canvas.RotateRadians(MathF.PI, 0.5f, 0.5f);
					break;
			}
		}

		using var bodyPaint = new SKPaint
		{
			Color = snakeColor[tile.Id].Body,
			IsAntialias = true,
		};
		canvas.DrawPath(HeadPath.Value, bodyPaint);

		using var eyePaint = new SKPaint
		{
			Color = snakeColor[tile.Id].Eyes,
			IsAntialias = true,
		};
		canvas.DrawPath(EyesPath.Value, eyePaint);

		canvas.Restore();
	}
	private static readonly Lazy<SKPath> HeadPath = new(() =>
	{
		var path = new SKPath();
		path.MoveTo(0.0f, 1.0f);
		path.LineTo(0.0f, 0.6f);
		path.LineTo(0.4f, 0.0f);
		path.LineTo(0.6f, 0.0f);
		path.LineTo(1.0f, 0.6f);
		path.LineTo(1.0f, 1.0f);
		return path;
	});
	private static readonly Lazy<SKPath> EyesPath = new(() =>
	{
		var path = new SKPath();
		path.AddArc(new SKRect(0.2f, 0.3f, 0.5f, 0.6f), 135, 180);
		path.AddArc(new SKRect(0.5f, 0.3f, 0.8f, 0.6f), 225, 180);
		return path;
	});

	private void RenderBody(SKCanvas canvas, SnakeBodyTile tile)
	{
		canvas.Save();
		canvas.Scale(Size);

		if (IsHorizontal(tile) || IsVertical(tile))
		{
			RenderBodyStraight(canvas, tile);
		}
		else
		{
			if (IsNorthEastTurn(tile))
			{
				canvas.RotateRadians(-MathF.PI / 2, 0.5f, 0.5f);
			}
			else if (IsNorthWestTurn(tile))
			{
				canvas.RotateRadians(MathF.PI, 0.5f, 0.5f);
			}
			else if (IsSouthWestTurn(tile))
			{
				canvas.RotateRadians(MathF.PI / 2, 0.5f, 0.5f);
			}

			RenderBodyTurn(canvas, tile);
		}

		canvas.Restore();

		static bool IsHorizontal(SnakeBodyTile tile) =>
			DirectionsFromCurrent(tile) is (Direction.Left, Direction.Right) or (Direction.Right, Direction.Left);
		static bool IsVertical(SnakeBodyTile tile) =>
			DirectionsFromCurrent(tile) is (Direction.Up, Direction.Down) or (Direction.Down, Direction.Up);
		static bool IsNorthEastTurn(SnakeBodyTile tile) =>
			DirectionsFromCurrent(tile) is (Direction.Up, Direction.Right) or (Direction.Right, Direction.Up);
		static bool IsNorthWestTurn(SnakeBodyTile tile) =>
			DirectionsFromCurrent(tile) is (Direction.Up, Direction.Left) or (Direction.Left, Direction.Up);
		static bool IsSouthWestTurn(SnakeBodyTile tile) =>
			DirectionsFromCurrent(tile) is (Direction.Down, Direction.Left) or (Direction.Left, Direction.Down);

		static (Direction, Direction) DirectionsFromCurrent(SnakeBodyTile tile) =>
			(Vector2.ReverseDirections[tile.Next - tile.Current], Vector2.ReverseDirections[tile.Previous - tile.Current]);
	}

	private void RenderBodyStraight(SKCanvas canvas, SnakeBodyTile tile)
	{
		using var paint = new SKPaint { Color = snakeColor[tile.Id].Body };
		canvas.DrawRect(0, 0, 1, 1, paint);
	}

	private void RenderBodyTurn(SKCanvas canvas, SnakeBodyTile tile)
	{
		using var paint = new SKPaint
		{
			Color = snakeColor[tile.Id].Body,
			IsAntialias = true,
		};
		canvas.DrawPath(BodyTurnPath.Value, paint);
	}
	private static readonly Lazy<SKPath> BodyTurnPath = new(() =>
	{
		var path = new SKPath();
		path.MoveTo(0.0f, 1.0f);
		path.ArcTo(0.0f, 0.0f, 1.0f, 0.0f, 1.0f);
		path.LineTo(1.0f, 1.0f);
		return path;
	});

	private void RenderTail(SKCanvas canvas, SnakeTailTile tile)
	{
		canvas.Save();
		canvas.Scale(Size);
		if (tile.Next is Vector2 next)
		{
			switch (tile.Current - next)
			{
				case (1, 0):
					canvas.RotateRadians(-MathF.PI / 2, 0.5f, 0.5f);
					break;
				case (-1, 0):
					canvas.RotateRadians(MathF.PI / 2, 0.5f, 0.5f);
					break;
				case (0, -1):
					canvas.RotateRadians(MathF.PI, 0.5f, 0.5f);
					break;
			}
		}

		using var paint = new SKPaint
		{
			Color = snakeColor[tile.Id].Body,
			IsAntialias = true,
		};
		canvas.DrawPath(TailPath.Value, paint);

		canvas.Restore();
	}
	private static readonly Lazy<SKPath> TailPath = new(() =>
	{
		var path = new SKPath();
		path.LineTo(0.5f, 1.0f);
		path.LineTo(1.0f, 0.0f);
		return path;
	});

	public void Dispose()
	{
		renderer.RemoveRenderable(this);
	}
}
