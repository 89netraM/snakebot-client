using System;

namespace Cygni.Snake.Client.Models;

public record PlayerRank(
	string PlayerName,
	Guid PlayerId,
	int Rank,
	int Points,
	bool Alive);
