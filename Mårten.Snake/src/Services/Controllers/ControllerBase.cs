using System;
using System.Threading;
using System.Threading.Tasks;
using Cygni.Snake.Client.Messages;
using Cygni.Snake.Client.Models;
using Microsoft.Extensions.Logging;

namespace Mårten.Snake.Services.Controllers;

public abstract class ControllerBase : IDisposable
{
	protected readonly ClientService client;
	private readonly ILogger<ControllerBase> logger;

	private CancellationTokenSource cancellationSource = new CancellationTokenSource();
	private Task<Direction>? directionCalculation;
	public Direction? ProposedDirection => directionCalculation?.IsCompletedSuccessfully ?? false
		? directionCalculation.Result
		: null;

	public ControllerBase(ClientService client, ILoggerFactory loggerFactory)
	{
		this.client = client;
		this.client.OnMapUpdateEvent += OnMapUpdateEvent;

		logger = loggerFactory.CreateLogger<ControllerBase>();
	}

	private void OnMapUpdateEvent(MapUpdateEvent mapUpdateEvent)
	{
		CancelAndReset();
		logger.LogTrace("Running new direction calculation");
		directionCalculation = Task.Run(
			() => CalculateDirection(mapUpdateEvent, cancellationSource.Token),
			cancellationSource.Token);
	}

	private void CancelAndReset()
	{
		if (directionCalculation?.IsCompleted ?? true)
		{
			if (directionCalculation?.IsFaulted ?? false)
			{
				logger.LogError(directionCalculation.Exception, "Direction calculation faulted");
			}

			logger.LogTrace("Attempting reset controller");
			if (!cancellationSource.TryReset())
			{
				cancellationSource = new CancellationTokenSource();
				logger.LogTrace("Reset failed, recreating controller");
			}
		}
		else
		{
			logger.LogTrace("Canceling controller");
			cancellationSource.Cancel();
			cancellationSource = new CancellationTokenSource();
		}
	}

	protected abstract Direction CalculateDirection(MapUpdateEvent mapUpdateEvent, CancellationToken cancellationToken);

	public void Dispose()
	{
		this.client.OnMapUpdateEvent -= OnMapUpdateEvent;
	}
}
