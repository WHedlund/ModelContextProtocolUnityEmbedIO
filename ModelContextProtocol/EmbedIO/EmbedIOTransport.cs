using ModelContextProtocol.Protocol.Messages;
using ModelContextProtocol.Protocol.Transport;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using UnityEngine;

public class EmbedIOTransport : ITransport
{
    /// <summary>
    /// A custom ITransport implementation using SSE over EmbedIO's response stream.
    /// Allows the MCP server to send messages to connected clients.
    /// </summary>

    private readonly Stream _outputStream;
    private readonly Channel<JsonRpcMessage> _channel = Channel.CreateUnbounded<JsonRpcMessage>();

    public EmbedIOTransport(Stream outputStream)
    {
        _outputStream = outputStream;
        Debug.Log("[MCP] EmbedIoTransport initialized.");

    }

    public ChannelReader<JsonRpcMessage> MessageReader => _channel.Reader;

    public bool IsConnected => true;

    public async Task SendMessageAsync(JsonRpcMessage message, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(message, message.GetType());
        var sse = $"event: message\ndata: {json}\n\n";
        var bytes = Encoding.UTF8.GetBytes(sse);
        await _outputStream.WriteAsync(bytes, 0, bytes.Length, cancellationToken);
        await _outputStream.FlushAsync(cancellationToken);
    }

    public Task OnMessageReceivedAsync(object message, CancellationToken cancellationToken)
    {
        if (message is JsonRpcMessage m)
            return _channel.Writer.WriteAsync(m, cancellationToken).AsTask();

        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        _outputStream.Dispose();
        return new ValueTask(Task.CompletedTask);
    }
}
