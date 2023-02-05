using System;
using Cygni.Snake.Client.Messages;
using Mårten.Snake.Services;
using Microsoft.Extensions.Logging;
using Zarya;

namespace Mårten.Snake.GameObjects;

public class StarterObject : IDisposable
{
	private readonly IGameManager gameManager;
	private readonly ClientService client;
	private readonly InputService input;
	private readonly ILogger<StarterObject> logger;

	private bool hasRequestedStart = false;

	public StarterObject(
		IGameManager gameManager,
		ClientService client,
		PlayerInfoService playerInfoService,
		InputService input,
		ILogger<StarterObject> logger)
	{
		this.gameManager = gameManager;
		this.gameManager.Initialize += OnInitialize;
		this.gameManager.Update += OnUpdate;

		this.client = client;
		this.client.OnGameStartingEvent += OnGameStartingEvent;

		this.input = input;

		this.logger = logger;
	}

	private void OnInitialize()
	{
		_ = client.Connect();
	}

	private void OnGameStartingEvent(GameStartingEvent gameStartingEvent)
	{
		gameManager.Create<ControllerObject>(gameStartingEvent.GameId, gameStartingEvent.GameSettings);
		gameManager.Create<TickCountDownObject>(gameStartingEvent.GameSettings);
		logger.LogInformation($"Game started ({gameStartingEvent.GameId})");
		gameManager.Destroy(this);
	}

	private void OnUpdate(float deltaTime)
	{
		if (!hasRequestedStart && input.Start())
		{
			client.SendMessage(new StartGame());
			hasRequestedStart = true;
			logger.LogInformation("Starting game");
		}
	}

	public void Dispose()
	{
		this.client.OnGameStartingEvent -= OnGameStartingEvent;
		this.gameManager.Update -= OnUpdate;
		this.gameManager.Initialize -= OnInitialize;
	}
}
