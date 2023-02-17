using System;
using Cygni.Snake.Client.Messages;
using Cygni.Snake.Client.Models;
using Cygni.Snake.Utils;
using Microsoft.Extensions.Logging;

namespace Mårten.Snake.Services;

public class MapInfoService : IDisposable
{
	private readonly ClientService client;
	private readonly ILogger<MapInfoService> logger;

	private GameSettings? gameSettings;

	public event Action<MapInfo>? OnMapInfoUpdateEvent;

	public MapInfoService(ClientService client, ILogger<MapInfoService> logger)
	{
		this.client = client;
		this.client.OnGameStartingEvent += OnGameStartingEvent;
		this.client.OnMapUpdateEvent += OnMapUpdateEvent;

		this.logger = logger;
	}

	private void OnGameStartingEvent(GameStartingEvent gameStartingEvent)
	{
		gameSettings = gameStartingEvent.GameSettings;
	}

	private void OnMapUpdateEvent(MapUpdateEvent mapUpdateEvent)
	{
		if (gameSettings is not null)
		{
			OnMapInfoUpdateEvent?.Invoke(new MapInfo(mapUpdateEvent, gameSettings));
		}
		else
		{
			logger.LogWarning("No gameSettings available, map update event received before game starting event");
		}
	}

	public void Dispose()
	{
		client.OnMapUpdateEvent -= OnMapUpdateEvent;
		client.OnGameStartingEvent -= OnGameStartingEvent;
	}
}
