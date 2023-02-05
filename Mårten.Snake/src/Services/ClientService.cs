using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Cygni.Snake.Client;
using Cygni.Snake.Client.Messages;
using Cygni.Snake.Client.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SnakeClient = Cygni.Snake.Client.Client;

namespace Mårten.Snake.Services;

public class ClientService : IDisposable
{
	[MemberNotNullWhen(true, nameof(client))]
	public bool IsConnected => client?.IsConnected ?? false;

	public event Action<ArenaEndedEvent>? OnArenaEndedEvent;
	public event Action<ArenaUpdateEvent>? OnArenaUpdateEvent;
	public event Action<GameAbortedEvent>? OnGameAbortedEvent;
	public event Action<GameChangedEvent>? OnGameChangedEvent;
	public event Action<GameCreatedEvent>? OnGameCreatedEvent;
	public event Action<GameEndedEvent>? OnGameEndedEvent;
	public event Action<GameLinkEvent>? OnGameLinkEvent;
	public event Action<GameResultEvent>? OnGameResultEvent;
	public event Action<GameStartingEvent>? OnGameStartingEvent;
	public event Action<MapUpdateEvent>? OnMapUpdateEvent;
	public event Action<SnakeDeadEvent>? OnSnakeDeadEvent;
	public event Action<TournamentEndedEvent>? OnTournamentEndedEvent;
	public event Action<ArenaIsFull>? OnArenaIsFull;
	public event Action<InvalidArenaName>? OnInvalidArenaName;
	public event Action<InvalidMessage>? OnInvalidMessage;
	public event Action<InvalidPlayerName>? OnInvalidPlayerName;
	public event Action<NoActiveTournament>? OnNoActiveTournament;
	public event Action<HeartBeatResponse>? OnHeartBeatResponse;
	public event Action<PlayerRegistered>? OnPlayerRegistered;

	private readonly GameOptions options;
	private readonly ILogger<ClientService> logger;
	private IClient? client;

	public ClientService(IOptions<GameOptions> options, ILogger<ClientService> logger)
	{
		this.options = options.Value;
		this.logger = logger;
	}

	public async Task Connect(CancellationToken? cancellationToken = null)
	{
		logger.LogInformation($"Connecting to {options.Uri}");
		client = await SnakeClient.Initialize(options.Uri, cancellationToken);
		await SendMessage(client.ClientInfo, cancellationToken);
		await SendMessage(new RegisterPlayer(options.Name, options.Settings), cancellationToken);

		MessageLoop(cancellationToken);
	}

	public async Task Start(CancellationToken? cancellationToken = null)
	{
		if (options.GameMode is not GameMode.Training)
		{
			throw new InvalidOperationException("Cannot start a non-training game");
		}

		logger.LogInformation("Starting game");
		await SendMessage(new StartGame(), cancellationToken);
	}

	public ValueTask SendMessage(Message message, CancellationToken? cancellationToken = null)
	{
		if (!IsConnected)
		{
			throw new InvalidOperationException("Cannot send messages when not connected");
		}

		logger.LogTrace($"Sending message {message}");
		return client.SendMessage(message, cancellationToken);
	}

	private async void MessageLoop(CancellationToken? cancellationToken = null)
	{
		CancellationToken concreteCancellationToken = cancellationToken ?? CancellationToken.None;

		while (IsConnected && !concreteCancellationToken.IsCancellationRequested)
		{
			Message? message = await client.ReceiveMessage(concreteCancellationToken);
			LogMessage(message);
			if (message is not null)
			{
				HandleMessage(message);
			}
		}
	}

	private void LogMessage(Message? message)
	{
		if (message is null)
		{
			logger.LogWarning("Received null message");
		}
		else
		{
			logger.LogTrace($"Received message {message}");
		}
	}

	private void HandleMessage(Message message)
	{
		switch (message)
		{
			case ArenaEndedEvent arenaEndedEvent:
				OnArenaEndedEvent?.Invoke(arenaEndedEvent);
				break;
			case ArenaUpdateEvent arenaUpdateEvent:
				OnArenaUpdateEvent?.Invoke(arenaUpdateEvent);
				break;
			case GameAbortedEvent gameAbortedEvent:
				OnGameAbortedEvent?.Invoke(gameAbortedEvent);
				break;
			case GameChangedEvent gameChangedEvent:
				OnGameChangedEvent?.Invoke(gameChangedEvent);
				break;
			case GameCreatedEvent gameCreatedEvent:
				OnGameCreatedEvent?.Invoke(gameCreatedEvent);
				break;
			case GameEndedEvent gameEndedEvent:
				OnGameEndedEvent?.Invoke(gameEndedEvent);
				break;
			case GameLinkEvent gameLinkEvent:
				OnGameLinkEvent?.Invoke(gameLinkEvent);
				break;
			case GameResultEvent gameResultEvent:
				OnGameResultEvent?.Invoke(gameResultEvent);
				break;
			case GameStartingEvent gameStartingEvent:
				OnGameStartingEvent?.Invoke(gameStartingEvent);
				break;
			case MapUpdateEvent mapUpdateEvent:
				OnMapUpdateEvent?.Invoke(mapUpdateEvent);
				break;
			case SnakeDeadEvent snakeDeadEvent:
				OnSnakeDeadEvent?.Invoke(snakeDeadEvent);
				break;
			case TournamentEndedEvent tournamentEndedEvent:
				OnTournamentEndedEvent?.Invoke(tournamentEndedEvent);
				break;
			case ArenaIsFull arenaIsFull:
				OnArenaIsFull?.Invoke(arenaIsFull);
				break;
			case InvalidArenaName invalidArenaName:
				OnInvalidArenaName?.Invoke(invalidArenaName);
				break;
			case InvalidMessage invalidMessage:
				OnInvalidMessage?.Invoke(invalidMessage);
				break;
			case InvalidPlayerName invalidPlayerName:
				OnInvalidPlayerName?.Invoke(invalidPlayerName);
				break;
			case NoActiveTournament noActiveTournament:
				OnNoActiveTournament?.Invoke(noActiveTournament);
				break;
			case HeartBeatResponse heartBeatResponse:
				OnHeartBeatResponse?.Invoke(heartBeatResponse);
				break;
			case PlayerRegistered playerRegistered:
				OnPlayerRegistered?.Invoke(playerRegistered);
				break;
		}
	}

	public void Dispose()
	{
		client?.Dispose();
	}
}
