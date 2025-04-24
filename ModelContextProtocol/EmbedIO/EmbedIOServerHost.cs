using EmbedIO;
using EmbedIO.WebApi;
using UnityEngine;
using System.Threading;

//namespace ModelContextProtocol.EmbedIO;

public class EmbedIOServerHost : MonoBehaviour
{
    /// <summary>
    /// Initializes and hosts the EmbedIO HTTP server for handling MCP sessions in Unity.
    /// Registers API endpoints and manages server lifecycle.
    /// </summary>

    private WebServer _server;
    private CancellationTokenSource _cts;

    void Start()
    {
        const string url = "http://localhost:8888/";
        _cts = new CancellationTokenSource();

        Debug.Log("[MCP] Initializing EmbedIO server...");

        _server = new WebServer(o => o
                .WithUrlPrefix(url)
                .WithMode(HttpListenerMode.EmbedIO))
            .WithLocalSessionManager()
            .WithWebApi("/api", m => m
                .WithController(() => new EmbedIOApiController()));

        _server.RunAsync(_cts.Token);
        Debug.Log("[MCP] EmbedIO Server running at " + url);
    }

    void OnApplicationQuit()
    {
        Debug.Log("[MCP] Application quitting. Stopping server...");
        _cts.Cancel();
        _server?.Dispose();
        Debug.Log("[MCP] Server stopped.");
    }
}
