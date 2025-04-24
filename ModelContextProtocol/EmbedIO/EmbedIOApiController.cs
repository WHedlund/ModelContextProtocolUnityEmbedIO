using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using ModelContextProtocol.Protocol.Messages;
using ModelContextProtocol.Protocol.Types;
using ModelContextProtocol.Server;
using ModelContextProtocol.Utils.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

//namespace ModelContextProtocol.EmbedIO;

public sealed class EmbedIOApiController : WebApiController
{
    /// <summary>
    /// Defines API routes for SSE session establishment and JSON-RPC message handling.
    /// Integrates EmbedIOTransport with the MCP server.
    /// </summary>

    private static readonly ConcurrentDictionary<string, EmbedIOTransport> Sessions = new();

    private readonly Func<Dictionary<object, List<MethodInfo>>> _getServices;
    private readonly List<McpServerTool> tools;

    [Route(HttpVerbs.Get, "/sse")]
    public async Task Sse()
    {
        var sessionId = Guid.NewGuid().ToString("N");
        var response = HttpContext.Response;

        response.ContentType = "text/event-stream";
        response.ContentEncoding = Encoding.UTF8;
        response.SendChunked = true;
        response.KeepAlive = true;
        response.Headers["Cache-Control"] = "no-cache";

        Debug.Log($"[MCP] New SSE connection established. Session ID: {sessionId}");

        var transport = new EmbedIOTransport(response.OutputStream);
        Sessions[sessionId] = transport;

        var initEvent = $"event: endpoint\ndata: /api/message?sessionId={sessionId}\n\n";
        await response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(initEvent));
        await response.OutputStream.FlushAsync();

        var serverOptions = new McpServerOptions
        {
            ServerInfo = new() { Name = "UnityEmbedIOServer", Version = "0.1" },
            Capabilities = new ServerCapabilities { Tools = CreateToolsCapability(tools) }
        };

        await using var mcpServer = McpServerFactory.Create(transport, serverOptions);
        Debug.Log("[MCP] MCP server running...");
        await mcpServer.RunAsync(CancellationToken.None);

        Sessions.TryRemove(sessionId, out _);
        Debug.Log($"[MCP] Session {sessionId} ended.");
    }


    [Route(HttpVerbs.Post, "/message")]
    public async Task ReceiveMessage()
    {
        var sessionId = HttpContext.Request.QueryString["sessionId"];
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.SendStringAsync("Missing sessionId", "text/plain", Encoding.UTF8);
            Debug.LogWarning("[MCP] Received message without sessionId.");
            return;
        }

        if (!Sessions.TryGetValue(sessionId, out var transport))
        {
            HttpContext.Response.StatusCode = 404;
            await HttpContext.SendStringAsync("Invalid sessionId", "text/plain", Encoding.UTF8);
            Debug.LogWarning($"[MCP] Unknown sessionId: {sessionId}");
            return;
        }

        var json = await HttpContext.GetRequestBodyAsStringAsync();
        Debug.Log($"[MCP] Received message for session {sessionId}: {json}");

        try
        {
            var message = JsonSerializer.Deserialize<JsonRpcMessage>(json, McpJsonUtilities.DefaultOptions);

            await transport.OnMessageReceivedAsync(message, CancellationToken.None);
            HttpContext.Response.StatusCode = 202;
            await HttpContext.SendStringAsync("Accepted", "text/plain", Encoding.UTF8);

            Debug.Log($"[MCP] Message for session {sessionId} queued for processing.");
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            await HttpContext.SendStringAsync("Server error", "text/plain", Encoding.UTF8);
            Debug.LogWarning($"[MCP] Server error: {ex.ToString()}");
        }
    }


    public static ToolsCapability CreateToolsCapability(List<McpServerTool> tools)
    {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        return new ToolsCapability
        {
            ListToolsHandler = async (ctx, ct) =>
            {
                return new ListToolsResult
                {
                    Tools = tools.ConvertAll(t => t.ProtocolTool)
                };
            },
            CallToolHandler = async (ctx, ct) =>
            {
                var targetTool = tools.Find(t => t.ProtocolTool.Name == ctx.Params?.Name);
                return targetTool is not null
                    ? await targetTool.InvokeAsync(ctx, ct)
                    : throw new InvalidOperationException($"Tool '{ctx.Params?.Name}' not found.");
            }
        };
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }

    public EmbedIOApiController(Dictionary<object, List<MethodInfo>> services = null)
    {
        if (services == null || services.Count == 0)
            return;

        this.tools = new List<McpServerTool>();
        foreach (var service in services)
        {
            foreach (var method in service.Value)
            {
                tools.Add(McpServerTool.Create(method, target: service.Key));
            }
        }
    }
}