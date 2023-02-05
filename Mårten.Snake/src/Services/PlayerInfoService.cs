using System;
using Cygni.Snake.Client.Messages;
using Microsoft.Extensions.Logging;

namespace Mårten.Snake.Services;

public class PlayerInfoService : IDisposable
{
	public Guid? PlayerId { get; private set; }

	private readonly ClientService client;
	private readonly ILogger<PlayerInfoService> logger;

	public PlayerInfoService(ClientService client, ILogger<PlayerInfoService> logger)
	{
		this.client = client;
		this.client.OnPlayerRegistered += OnPlayerRegistered;

		this.logger = logger;
	}

	private void OnPlayerRegistered(PlayerRegistered playerRegistered)
	{
		PlayerId = playerRegistered.ReceivingPlayerId;
		logger.LogInformation($"Player registered with id {PlayerId}");
	}

	public void Dispose()
	{
		client.OnPlayerRegistered -= OnPlayerRegistered;
	}
}
