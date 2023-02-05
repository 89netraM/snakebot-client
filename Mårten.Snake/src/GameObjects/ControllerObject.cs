using System;
using System.Collections.Generic;
using Cygni.Snake.Client.Messages;
using Cygni.Snake.Client.Models;
using Mårten.Snake.Services;
using Mårten.Snake.Services.Controllers;
using Microsoft.Extensions.Logging;
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
		GameSettings gameSettings)
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
	}

	private void OnMapUpdateEvent(MapUpdateEvent mapUpdateEvent)
	{
		tickAndTime = (mapUpdateEvent.GameTick, gameManager.Time);
		logger.LogDebug($"Map update {tickAndTime}");
	}

	private void OnUpdate(float deltaTime)
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

		if (tickAndTime is (long tick, float time) && gameManager.Time > time + timePerTick * 0.75f)
		{
			if (directionQueue.TryDequeue(out Direction dir))
			{
				client.SendMessage(new RegisterMove(gameId, tick, dir));
				logger.LogDebug($"Register user move {dir} for tick {tick}");
			}
			else if (controller.ProposedDirection is Direction proposedDir)
			{
				client.SendMessage(new RegisterMove(gameId, tick, proposedDir));
				logger.LogDebug($"Register calculated move {proposedDir} for tick {tick}");
			}
			else
			{
				logger.LogDebug($"No move registered for tick {tick}");
			}
			tickAndTime = null;
		}
	}

	public void Dispose()
	{
		this.client.OnMapUpdateEvent -= OnMapUpdateEvent;
		this.gameManager.Update -= OnUpdate;
	}
}
