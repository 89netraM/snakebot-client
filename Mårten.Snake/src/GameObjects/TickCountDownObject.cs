using System;
using Cygni.Snake.Client.Messages;
using Cygni.Snake.Client.Models;
using Mårten.Snake.Services;
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

	public Transform2D Transform { get; } = new() { Position = new(Radius, Radius) };

	private readonly IGameManager gameManager;
	private readonly SkiaSharpRenderer renderer;
	private readonly ClientService client;
	private readonly GameSettings gameSettings;

	private float tickStart = 0.0f;

	private float timePerTick => gameSettings.TimeInMsPerTick / 1000.0f;

	public TickCountDownObject(
		IGameManager gameManager,
		SkiaSharpRenderer renderer,
		ClientService client,
		GameSettings gameSettings)
	{
		this.gameManager = gameManager;

		this.renderer = renderer;
		this.renderer.AddRenderable(this, 10);

		this.client = client;
		this.client.OnMapUpdateEvent += OnMapUpdateEvent;

		this.gameSettings = gameSettings;
	}

	private void OnMapUpdateEvent(MapUpdateEvent mapUpdateEvent)
	{
		tickStart = gameManager.Time;
	}

	public void Render(SKCanvas canvas)
	{
		using var paint = new SKPaint() { Color = new(0xFF00FF00), StrokeWidth = Thickness, IsStroke = true, IsAntialias = true };
		canvas.DrawArc(Rect, -90.0f, (gameManager.Time - tickStart) / timePerTick * 360.0f, false, paint);
	}

	public void Dispose()
	{
		renderer.RemoveRenderable(this);
		client.OnMapUpdateEvent -= OnMapUpdateEvent;
	}
}
