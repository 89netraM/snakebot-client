using System;
using System.Threading;
using System.Threading.Tasks;
using Cygni.Snake.Client.Models;
using Cygni.Snake.Utils;
using Microsoft.Extensions.Logging;

namespace Mårten.Snake.Services.Controllers;

public abstract class ControllerBase : IDisposable
{
	protected readonly MapInfoService mapInfoService;
	private readonly ILogger<ControllerBase> logger;

	private CancellationTokenSource cancellationSource = new CancellationTokenSource();
	private Task<Direction>? directionCalculation;
	public Direction? ProposedDirection => directionCalculation?.IsCompletedSuccessfully ?? false
		? directionCalculation.Result
		: null;

	public ControllerBase(MapInfoService mapInfo, ILoggerFactory loggerFactory)
	{
		this.mapInfoService = mapInfo;
		this.mapInfoService.OnMapInfoUpdateEvent += OnMapInfoUpdateEvent;

		logger = loggerFactory.CreateLogger<ControllerBase>();
	}

	private void OnMapInfoUpdateEvent(MapInfo mapInfo)
	{
		CancelAndReset();
		logger.LogTrace("Running new direction calculation");
		directionCalculation = Task.Run(
			() => CalculateDirectionInternal(mapInfo, cancellationSource.Token),
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

	private Direction CalculateDirectionInternal(MapInfo mapInfo, CancellationToken cancellationToken)
	{
		try
		{
			return CalculateDirection(mapInfo, cancellationToken);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, $"Could not find a possible direction, continuuing current direction ({mapInfo.PlayerSnake.HeadDirection})");
			return mapInfo.PlayerSnake.HeadDirection;
		}
	}

	protected abstract Direction CalculateDirection(MapInfo mapInfo, CancellationToken cancellationToken);

	public void Dispose()
	{
		mapInfoService.OnMapInfoUpdateEvent -= OnMapInfoUpdateEvent;
	}
}
