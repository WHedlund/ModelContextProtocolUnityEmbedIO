using ModelContextProtocol.Protocol.Types;
using ModelContextProtocol.Server;
using System.Collections.Generic;
using System.Reflection;
using System;

//namespace ModelContextProtocol.EmbedIO.Tools;

public static class ToolRegistry
{
    /// <summary>
    /// Provides utilities for discovering and registering MCP tools from static classes.
    /// Simplifies tool registration and tool invocation handling.
    /// </summary>

    public static List<McpServerTool> DiscoverToolsFromTypes(params Type[] toolTypes)
    {
        var tools = new List<McpServerTool>();

        foreach (var type in toolTypes)
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                if (method != null)
                {
                    var tool = McpServerTool.Create(method);
                    tools.Add(tool);
                }
            }
        }

        return tools;
    }

    public static ToolsCapability CreateToolsCapability(List<McpServerTool> tools)
    {
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
    }
}