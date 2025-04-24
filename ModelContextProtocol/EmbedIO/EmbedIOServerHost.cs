using EmbedIO;
using EmbedIO.WebApi;
using UnityEngine;
using System.Threading;
using System.Collections.Generic;
using System.Reflection;
using ModelContextProtocol.Server;
using ModelContextProtocol.Protocol.Types;

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

        var serviceDict = FindTaggedInstancesAndMethods(this.gameObject);

        _server = new WebServer(o => o
                .WithUrlPrefix(url)
                .WithMode(HttpListenerMode.EmbedIO))
            .WithLocalSessionManager()
        .WithWebApi("/api", m => m
                .WithController(() => new EmbedIOApiController(serviceDict)));

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



    Dictionary<object, List<MethodInfo>> FindTaggedInstancesAndMethods(GameObject root)
    {
        var result = new Dictionary<object, List<MethodInfo>>();
        var components = root.GetComponentsInChildren<MonoBehaviour>(true);

        foreach (var comp in components)
        {
            if (comp == null) continue; // skip destroyed
            var type = comp.GetType();

            if (type.GetCustomAttribute<McpServerToolTypeAttribute>() != null)
            {
                var methodList = new List<MethodInfo>();
                foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public))
                {
                    if (method.GetCustomAttribute<McpServerToolAttribute>() != null)
                        methodList.Add(method);
                }

                if (methodList.Count > 0)
                    result[comp] = methodList;
            }
        }

        foreach (var kvp in result)
        {
            Debug.Log($"[MCP] Service: {kvp.Key}, Method count: {kvp.Value.Count}");
            foreach (var method in kvp.Value)
                Debug.Log($"[MCP] - Method: {method.Name} ({method.DeclaringType?.FullName})");
        }

        return result;
    }
}
