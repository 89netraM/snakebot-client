using System;
using System.Collections.Generic;

namespace Cygni.Snake.Client.Models;

public record ArenaHistory(Guid GameId, IReadOnlyList<Guid> PlayerPositions);
