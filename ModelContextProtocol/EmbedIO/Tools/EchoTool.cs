using ModelContextProtocol.Server;
using System.ComponentModel;
using UnityEngine;


[McpServerToolType]
public static class EchoTool
{
    /// <summary>
    /// A simple MCP tool that echoes back the input message. Used for testing tool invocation.
    /// </summary>

    [McpServerTool, Description("Echoes the message back to the client.")]
    public static string Echo(string message)
    {
        string response = $"hello {message}";
        Debug.Log($"EchoTool: {response}");
        return response;
    }
}
