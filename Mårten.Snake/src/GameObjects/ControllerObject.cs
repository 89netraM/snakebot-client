using System;
using System.Collections.Generic;
using Cygni.Snake.Client.Messages;
using Cygni.Snake.Client.Models;
using Mårten.Snake.Services;
using Mårten.Snake.Services.Controllers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zarya;

namespace Mårten.Snake.GameObjects;

public class ControllerObject : IDisposable
{
	private readonly IGameManager gameManager;
	private readonly ClientService client;
	private readonly InputService input;
	private readonly ControllerBase controller;
	private readonly ILogger<ControllerObject> logger;

	private readonly Guid gameId;
	private readonly GameSettings gameSettings;
	private readonly GameOptions options;
	private (long tick, float time)? tickAndTime = null;
	private Queue<Direction> directionQueue = new();

	private float timePerTick => gameSettings.TimeInMsPerTick / 1000.0f;

	public ControllerObject(
		IGameManager gameManager,
		ClientService client,
		InputService input,
		ControllerBase controller,
		ILogger<ControllerObject> logger,
		Guid gameId,
		GameSettings gameSettings,
		IOptions<GameOptions> options)
	{
		this.gameManager = gameManager;
		this.gameManager.Update += OnUpdate;

		this.client = client;
		this.client.OnMapUpdateEvent += OnMapUpdateEvent;

		this.input = input;

		this.controller = controller;

		this.logger = logger;

		this.gameId = gameId;
		this.gameSettings = gameSettings;
		this.options = options.Value;
	}

	private void OnMapUpdateEvent(MapUpdateEvent mapUpdateEvent)
	{
		tickAndTime = (mapUpdateEvent.GameTick, gameManager.Time);
		logger.LogDebug($"Map update {tickAndTime}");
	}

	private async void OnUpdate(float deltaTime)
	{
		if (input.Up())
		{
			directionQueue.Enqueue(Direction.Up);
			logger.LogDebug("Input Up");
		}
		else if (input.Left())
		{
			directionQueue.Enqueue(Direction.Left);
			logger.LogDebug("Input Left");
		}
		else if (input.Right())
		{
			directionQueue.Enqueue(Direction.Right);
			logger.LogDebug("Input Right");
		}
		else if (input.Down())
		{
			directionQueue.Enqueue(Direction.Down);
			logger.LogDebug("Input Down");
		}
		while (directionQueue.Count > 2)
		{
			directionQueue.Dequeue();
		}

		if (tickAndTime is (long tick, float time) && gameManager.Time > time + timePerTick * options.TickTimePercentage)
		{
			if (GetMove(tick) is Direction dir)
			{
				await client.SendMessage(new RegisterMove(gameId, tick, dir));
				if (gameManager.Time > time + timePerTick)
				{
					logger.LogWarning($"Move for tick {tick} might have been registered too late");
				}
			}
			else
			{
				logger.LogDebug($"No move registered for tick {tick}");
			}
			tickAndTime = null;
		}
	}

	private Direction? GetMove(long tick)
	{
		if (directionQueue.TryDequeue(out Direction dir))
		{
			logger.LogDebug($"Register user move {dir} for tick {tick}");
			return dir;
		}
		else if (controller.ProposedDirection is Direction proposedDir)
		{
			logger.LogDebug($"Register calculated move {proposedDir} for tick {tick}");
			return proposedDir;
		}
		else
		{
			return null;
		}
	}

	public void Dispose()
	{
		this.client.OnMapUpdateEvent -= OnMapUpdateEvent;
		this.gameManager.Update -= OnUpdate;
	}
}
