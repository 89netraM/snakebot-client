using System.Collections.Generic;

namespace Mårten.Snake.Utils;

public static class DictionaryExtensions
{
	public static TValue? GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) =>
		dictionary.TryGetValue(key, out TValue? value) ? value : default;
}
