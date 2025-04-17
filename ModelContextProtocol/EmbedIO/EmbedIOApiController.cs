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
using UnityEngine;

//namespace ModelContextProtocol.EmbedIO;

public sealed class EmbedIOApiController : WebApiController
{
    /// <summary>
    /// Defines API routes for SSE session establishment and JSON-RPC message handling.
    /// Integrates EmbedIOTransport with the MCP server.
    /// </summary>

    private static readonly ConcurrentDictionary<string, EmbedIOTransport> Sessions = new();

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

        // Prepare EchoTool method
        MethodInfo echoMethod = typeof(EchoTool).GetMethod("Echo", BindingFlags.Public | BindingFlags.Static);
        if (echoMethod == null)
        {
            Debug.LogError("[MCP] Failed to retrieve Echo method.");
            return;
        }

        var echoTool = McpServerTool.Create(echoMethod);
        var serverOptions = new McpServerOptions
        {
            ServerInfo = new() { Name = "UnityEmbedIOServer", Version = "0.1" },
            Capabilities = new ServerCapabilities
            {
                Tools = new ToolsCapability
                {
                    ListToolsHandler = async (ctx, ct) =>
                    {
                        Debug.Log("[MCP] Listing available tools.");
                        return new ListToolsResult
                        {
                            Tools = new List<Tool> { echoTool.ProtocolTool }
                        };
                    },
                    CallToolHandler = async (ctx, ct) =>
                    {
                        Debug.Log($"[MCP] Invoking tool: {ctx.Params?.Name}");
                        return await echoTool.InvokeAsync(ctx, ct);
                    }
                }
            }
        };


        Debug.Log($"[MCP] Tool Registered: {echoTool.ProtocolTool.Name} — {echoTool.ProtocolTool.Description}");


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
            var message = JsonSerializer.Deserialize<IJsonRpcMessage>(json, McpJsonUtilities.DefaultOptions);
            if (message == null)
                throw new JsonException("Parsed message is null.");

            await transport.OnMessageReceivedAsync(message, CancellationToken.None);
            HttpContext.Response.StatusCode = 202;
            await HttpContext.SendStringAsync("Accepted", "text/plain", Encoding.UTF8);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[MCP] Error handling message: {ex.Message}");
            HttpContext.Response.StatusCode = 500;
            await HttpContext.SendStringAsync("Server error", "text/plain", Encoding.UTF8);
        }
    }
}
