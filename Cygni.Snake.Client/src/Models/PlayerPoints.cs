using System;

namespace Cygni.Snake.Client.Models;

public record PlayerPoints(string Name, Guid PlayerId, int Points);
