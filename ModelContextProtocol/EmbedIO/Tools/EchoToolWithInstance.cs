using ModelContextProtocol.Server;
using System.ComponentModel;
using UnityEngine;

//namespace ModelContextProtocol.EmbedIO.Tools;

[McpServerToolType]
public class EchoToolWithInstance: MonoBehaviour
{
    public string prefix;
    /// <summary>
    /// A simple MCP tool that echoes back the input message. Used for testing tool invocation.
    /// </summary>
    [McpServerTool, Description("Echoes the message back to the client.")]
    public string Echo(string message)
    {
        string response = $"{prefix} {message}";
        Debug.Log($"EchoTool: {response}");
        return response;
    }

    public EchoToolWithInstance(string prefix) {
        this.prefix = prefix;
    }
}
