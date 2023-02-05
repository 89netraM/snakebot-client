using System;
using System.Collections.Generic;
using Mårten.Snake.Utils;
using SkiaSharp;

namespace Mårten.Snake.Services;

public class SnakeColor
{
	private static readonly ColorScheme playerColor = new(new(0xFF0EBDE7), new(0xFF000000));
	private static readonly IReadOnlyList<ColorScheme> colors = new ColorScheme[]
	{
		new (new(0xFF3CC321), new(0xFF000000)),
		new (new(0xFFFF8F35), new(0xFF000000)),
		new (new(0xFFF978AD), new(0xFF000000)),
		new (new(0xFFBA43FF), new(0xFF000000)),
		new (new(0xFFF8F8F8), new(0xFF000000)),
		new (new(0xFFFFDF4A), new(0xFF000000)),
		new (new(0xFF000000), new(0xFFFFDF4A)),
		new (new(0xFFFF4848), new(0xFF000000)),
		new (new(0xFF9AF48E), new(0xFF000000)),
		new (new(0xFF9BF3F0), new(0xFF000000)),
	};

	public ColorScheme this[Guid id] => GetColor(id);

	private readonly Random random = new Random();
	private readonly IDictionary<Guid, int> cache = new Dictionary<Guid, int>();
	private readonly IDictionary<int, int> indexCount = new Dictionary<int, int>();

	private readonly PlayerInfoService playerInfoService;

	public SnakeColor(PlayerInfoService playerInfoService) =>
		this.playerInfoService = playerInfoService;

	private ColorScheme GetColor(Guid id)
	{
		if (id == playerInfoService.PlayerId)
		{
			return playerColor;
		}
		if (!cache.TryGetValue(id, out int index))
		{
			cache[id] = index = SampleColorIndex();
			indexCount[index] = indexCount.GetValueOrDefault(index) + 1;
		}
		return colors[index];
	}

	private int SampleColorIndex()
	{
		int colorsLeft = PositiveMod(colors.Count - 1 - cache.Count, colors.Count) + 1;
		int index = random.Next(colorsLeft);

		int timesFilled = cache.Count / colors.Count;
		for (int i = 0; i <= index; i++)
		{
			if (indexCount.GetValueOrDefault(i) > timesFilled)
			{
				index++;
			}
		}

		return index % colors.Count;

		static int PositiveMod(int t, int n) =>
			(t % n + n) % n;
	}
}

public record ColorScheme(SKColor Body, SKColor Eyes);
