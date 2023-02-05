using System;
using System.Collections.Generic;

namespace Cygni.Snake.Client.Models;

public record SnakeInfo(
	string Name,
	int Points,
	Guid Id,
	IReadOnlyList<int> Positions,
	long TailProtectedForGameTicks);
