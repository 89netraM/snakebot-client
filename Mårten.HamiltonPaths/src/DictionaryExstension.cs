using System.Collections.Generic;

namespace Mårten.HamiltonPaths;

public static class DictionaryExtension
{
	public static void Union<TKey, TValue>(this IDictionary<TKey, TValue> dict, IEnumerable<KeyValuePair<TKey, TValue>> other)
	{
		foreach (var kvp in other)
		{
			dict.Add(kvp);
		}
	}
}
