using System;
using System.Collections.Generic;
using Cygni.Snake.Client.Messages;
using Cygni.Snake.Client.Models;
using Cygni.Snake.Utils;
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
	private readonly MapInfoService mapInfoService;
	private readonly InputService input;
	private readonly ControllerBase controller;
	private readonly ILogger<ControllerObject> logger;

	private readonly Guid gameId;
	private readonly GameSettings gameSettings;
	private readonly GameOptions options;
	private MapInfo? mapInfo;
	private (long tick, float time)? tickAndTime = null;

	private float timePerTick => gameSettings.TimeInMsPerTick / 1000.0f;

	public ControllerObject(
		IGameManager gameManager,
		ClientService client,
		MapInfoService mapInfoService,
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
		this.client.OnGameEndedEvent += OnGameEndedEvent;

		this.mapInfoService = mapInfoService;
		this.mapInfoService.OnMapInfoUpdateEvent += OnMapInfoUpdateEvent;

		this.input = input;

		this.controller = controller;

		this.logger = logger;

		this.gameId = gameId;
		this.gameSettings = gameSettings;
		this.options = options.Value;
	}

	private void OnMapInfoUpdateEvent(MapInfo mapInfo)
	{
		tickAndTime = (mapInfo.WorldTick, gameManager.Time);
		this.mapInfo = mapInfo;
		logger.LogDebug($"Map update {tickAndTime}");
	}

	private async void OnUpdate(float deltaTime)
	{
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

	private void OnGameEndedEvent(GameEndedEvent _)
	{
		gameManager.Create<StarterObject>();
		gameManager.Destroy(this);
	}

	private Direction? GetMove(long tick)
	{
		if (GetUserMove() is Direction dir)
		{
			if (mapInfo?.IsOpen(mapInfo.PlayerSnake.Positions[0] + dir) ?? false)
			{
				logger.LogDebug($"Register user move {dir} for tick {tick}");
				return dir;
			}
			else
			{
				logger.LogWarning($"User move ({dir}) for tick {tick} is invalid");
			}
		}
		if (controller.ProposedDirection is Direction proposedDir)
		{
			logger.LogDebug($"Register calculated move {proposedDir} for tick {tick}");
			return proposedDir;
		}
		else
		{
			return null;
		}
	}

	private Direction? GetUserMove()
	{
		if (input.Up())
		{
			return Direction.Up;
		}
		else if (input.Left())
		{
			return Direction.Left;
		}
		else if (input.Right())
		{
			return Direction.Right;
		}
		else if (input.Down())
		{
			return Direction.Down;
		}
		return null;
	}

	public void Dispose()
	{
		this.mapInfoService.OnMapInfoUpdateEvent -= OnMapInfoUpdateEvent;
		this.gameManager.Update -= OnUpdate;
	}
}
