using System;
using System.Collections.Generic;
using System.Reflection;
using SkiaSharp;

namespace Mårten.Snake.Services;

public class SpritePool : IDisposable
{
	private readonly Assembly manifestAssembly = Assembly.GetEntryAssembly()!;
	private readonly IDictionary<string, SKBitmap> bitmaps = new Dictionary<string, SKBitmap>();

	public SKBitmap this[string path]
	{
		get
		{
			if (!bitmaps.TryGetValue(path, out SKBitmap? bitmap))
			{
				lock (bitmaps)
				{
					if (!bitmaps.TryGetValue(path, out bitmap))
					{
						using var stream = manifestAssembly.GetManifestResourceStream(path);
						bitmaps[path] = bitmap = SKBitmap.Decode(stream);
					}
				}
			}
			return bitmap;
		}
	}

	public void Dispose()
	{
		foreach (var bitmap in bitmaps.Values)
		{
			bitmap.Dispose();
		}
	}
}
