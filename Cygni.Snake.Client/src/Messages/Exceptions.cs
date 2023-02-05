using Cygni.Snake.Client.Models;

namespace Cygni.Snake.Client.Messages;

public record ArenaIsFull(int PlayersConnected) : Message;

public record InvalidArenaName(ArenaNameInvalidReason ArenaNameInvalidReason) : Message;

public record InvalidMessage(string ErrorMessage, string ReceivedMessage) : Message;

public record InvalidPlayerName(PlayerNameInvalidReason PlayerNameInvalidReason) : Message;

public record NoActiveTournament() : Message;
