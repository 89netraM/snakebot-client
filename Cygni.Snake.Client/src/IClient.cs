using System;
using System.Threading;
using System.Threading.Tasks;
using Cygni.Snake.Client.Messages;

namespace Cygni.Snake.Client;

public interface IClient : IDisposable
{
	ClientInfo ClientInfo { get; }

	bool IsConnected { get; }

	ValueTask SendMessage(Message message, CancellationToken? cancellationToken = null);

	ValueTask<Message?> ReceiveMessage(CancellationToken? cancellationToken = null);
}
