using System;
using Cygni.Snake.Client.Messages;
using Cygni.Snake.Client.Models;
using Mårten.Snake.Services;
using Microsoft.Extensions.Options;
using SkiaSharp;
using Zarya;
using Zarya.SkiaSharp;

namespace Mårten.Snake.GameObjects;

public class TickCountDownObject : ISkiaSharpRenderable, IDisposable
{
	private const float Radius = 20.0f;
	private const float Thickness = 2.5f;
	private const float ActualRadius = Radius - Thickness / 2.0f;
	private static readonly SKRect Rect = new(-ActualRadius, -ActualRadius, ActualRadius, ActualRadius);
	private static readonly SKColor ValidColor = new(0xFF00FF00);
	private static readonly SKColor LateColor = new(0xFFFF0000);

	public Transform2D Transform { get; } = new() { Position = new(Radius, Radius) };

	private readonly IGameManager gameManager;
	private readonly SkiaSharpRenderer renderer;
	private readonly ClientService client;
	private readonly GameSettings gameSettings;
	private readonly GameOptions options;

	private float tickStart = 0.0f;

	private float timePerTick => gameSettings.TimeInMsPerTick / 1000.0f;

	public TickCountDownObject(
		IGameManager gameManager,
		SkiaSharpRenderer renderer,
		ClientService client,
		GameSettings gameSettings,
		IOptions<GameOptions> options)
	{
		this.gameManager = gameManager;

		this.renderer = renderer;
		this.renderer.AddRenderable(this, 10);

		this.client = client;
		this.client.OnMapUpdateEvent += OnMapUpdateEvent;

		this.gameSettings = gameSettings;
		this.options = options.Value;
	}

	private void OnMapUpdateEvent(MapUpdateEvent mapUpdateEvent)
	{
		tickStart = gameManager.Time;
	}

	public void Render(SKCanvas canvas)
	{
		float percentage = (gameManager.Time - tickStart) / timePerTick;
		
		using var validPaint = new SKPaint() { Color = ValidColor, StrokeWidth = Thickness, IsStroke = true, IsAntialias = true };
		canvas.DrawArc(Rect, -90.0f, MathF.Min(percentage, options.TickTimePercentage) * 360.0f, false, validPaint);

		if (percentage > options.TickTimePercentage)
		{
			using var latePaint = new SKPaint() { Color = LateColor, StrokeWidth = Thickness, IsStroke = true, IsAntialias = true };
			canvas.DrawArc(Rect, 90.0f, MathF.Min(percentage - options.TickTimePercentage, 1.0f - options.TickTimePercentage) * 360.0f, false, latePaint);
		}
	}

	public void Dispose()
	{
		renderer.RemoveRenderable(this);
		client.OnMapUpdateEvent -= OnMapUpdateEvent;
	}
}
