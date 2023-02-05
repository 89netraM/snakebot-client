using System;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cygni.Snake.Client.Messages;

namespace Cygni.Snake.Client;

public class Client : IClient
{
	public static async Task<Client> Initialize(Uri uri, CancellationToken? cancellationToken = null)
	{
		ClientWebSocket socket = new ClientWebSocket();
		await socket.ConnectAsync(uri, cancellationToken ?? CancellationToken.None);
		return new Client(socket);
	}

	public ClientInfo ClientInfo { get; } = new ClientInfo(
		"C#",
		"11",
		Environment.OSVersion.Platform.ToString(),
		Environment.OSVersion.VersionString,
		Assembly.GetExecutingAssembly().GetName().Version!.ToString());

	private WebSocket socket;

	public bool IsConnected => socket.State == WebSocketState.Open;

	public Client(WebSocket socket) =>
		this.socket = socket;

	public ValueTask SendMessage(Message message, CancellationToken? cancellationToken = null)
	{
		string json = MessageJsonSerializer.Serialize(message);
		return socket.SendAsync(
			Encoding.UTF8.GetBytes(json),
			WebSocketMessageType.Text,
			WebSocketMessageFlags.EndOfMessage,
			cancellationToken ?? CancellationToken.None);
	}

	public async ValueTask<Message?> ReceiveMessage(CancellationToken? cancellationToken = null)
	{
		StringBuilder sb = new StringBuilder();

		byte[] buffer = new byte[1024];
		ArraySegment<byte> bufferSegment = new ArraySegment<byte>(buffer);
		while (cancellationToken?.IsCancellationRequested != true)
		{
			WebSocketReceiveResult result = await socket.ReceiveAsync(bufferSegment, cancellationToken ?? CancellationToken.None);

			if (result.MessageType is WebSocketMessageType.Binary or WebSocketMessageType.Close)
			{
				return null;
			}

			sb.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));

			if (result.EndOfMessage)
			{
				break;
			}
		}

		if (cancellationToken?.IsCancellationRequested == true)
		{
			return null;
		}

		string json = sb.ToString();
		return MessageJsonSerializer.Deserialize(json);
	}

	public void Dispose()
	{
		socket.Dispose();
	}
}
