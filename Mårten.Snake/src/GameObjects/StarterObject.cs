using System;
using Cygni.Snake.Client.Messages;
using Cygni.Snake.Client.Models;
using Mårten.Snake.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zarya;

namespace Mårten.Snake.GameObjects;

public class StarterObject : IDisposable
{
	private readonly IGameManager gameManager;
	private readonly ClientService client;
	private readonly GameOptions options;
	private readonly InputService input;
	private readonly ILogger<StarterObject> logger;

	private bool hasRequestedStart = false;

	public StarterObject(
		IGameManager gameManager,
		ClientService client,
		IOptions<GameOptions> options,
		PlayerInfoService playerInfoService,
		InputService input,
		ILogger<StarterObject> logger)
	{
		this.gameManager = gameManager;
		this.gameManager.Update += OnUpdate;

		this.client = client;
		this.client.OnGameStartingEvent += OnGameStartingEvent;

		this.options = options.Value;

		this.input = input;

		this.logger = logger;

		this.gameManager.Initialize += OnInitialize;
	}

	private void OnInitialize()
	{
		logger.LogInformation("Starting starter!");
		if (!client.IsConnected)
		{
			_ = client.Connect();
		}
	}

	private void OnGameStartingEvent(GameStartingEvent gameStartingEvent)
	{
		gameManager.Create<ControllerObject>(gameStartingEvent.GameId, gameStartingEvent.GameSettings);
		gameManager.Create<TickCountDownObject>(gameStartingEvent.GameSettings);
		logger.LogInformation($"Game started ({gameStartingEvent.GameId})");
		gameManager.Destroy(this);
	}

	private async void OnUpdate(float deltaTime)
	{
		if (!hasRequestedStart && options.GameMode == GameMode.Training && input.Start())
		{
			hasRequestedStart = true;
			logger.LogInformation("Starting game");
			if (!await client.Start())
			{
				hasRequestedStart = false;
			}
		}
	}

	public void Dispose()
	{
		this.client.OnGameStartingEvent -= OnGameStartingEvent;
		this.gameManager.Update -= OnUpdate;
		this.gameManager.Initialize -= OnInitialize;
	}
}
