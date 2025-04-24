# Unity EmbedIO Extensions for the MCP C# SDK

> **Unstable:** This package tracks the evolving [Model Context Protocol (MCP)](https://modelcontextprotocol.io/) and is subject to breaking changes as the MCP package updates.

## About

This package enables Unity projects to run a fully functional [Model Context Protocol (MCP)](https://modelcontextprotocol.io/) server using [EmbedIO](https://github.com/unosquare/embedio) as a lightweight HTTP server. It is designed specifically for Unity, since **ASP.NET Core is not fully compatible with the Unity runtime (Mono)**.

**Key features:**
- Native Unity integration for MCP servers.
- Uses EmbedIO for lightweight HTTP/SSE transport.
- Runs the HTTP server in a **separate thread** to avoid blocking the Unity main thread.
- (Planned) A MainThreadDispatcher may be added for safe Unity object interaction.
- Automatic tool discovery: Finds tools on the same GameObject (see `EchoToolWithInstance`).
- Updated to match latest MCP type/name changes.

> This is a community-driven project and not officially maintained by the MCP core team.

---

## 🚀 Installation & Getting Started

### 1. Add This Package to Your Unity Project

Simply **clone or copy this repository into your project’s `Assets` folder**.

```
Assets/
└── ModelContextProtocol/
    └── EmbedIO/
        ... (all scripts here)
```

---

### 2. Install Dependencies

Use **[NuGet for Unity](https://github.com/GlitchEnzo/NuGetForUnity?tab=readme-ov-file#how-do-i-install-nugetforunity)** to install the required packages:

- `EmbedIO` **3.5.2**
- `ModelContextProtocol` **0.1.0-preview.10**

> Enable "Show Prerelease Packages" in NuGet for Unity’s settings to find the correct MCP version.

---

### 3. Add Components to Your Scene

1. **Create a new GameObject** in your Unity scene.
2. **Attach both `EmbedIOServerHost` and `EchoToolWithInstance`** scripts to this GameObject.

When you press Play, the MCP server starts automatically (in a background thread) and is available at:

```
http://localhost:8888/api/sse
```

You can test your server using the [MCP Inspector tool from MCP[cli]](https://github.com/modelcontextprotocol/python-sdk/tree/main).

> To add more tools, just add additional tool scripts as components on the same GameObject for automatic discovery!

---

## 🧩 Usage & Examples

Tools are automatically discovered when added as components to the same GameObject as the server host.  
Here’s an example of an instance tool using MonoBehaviour:

```csharp
using UnityEngine;
using ModelContextProtocol.Tools;

public class EchoToolWithInstance : MonoBehaviour
{
    [McpServerTool(Description = "Echoes the input message (instance)")]
    public string Echo(string message)
    {
        return $"Echo: {message}";
    }
}
```

- Attach your tool script (like `EchoToolWithInstance`) to the same GameObject as `EmbedIOServerHost`.
- All `[McpServerTool]` methods will be registered and available to MCP clients.
- You can test tool calls using [MCP Inspector](https://github.com/modelcontextprotocol/python-sdk/tree/main) or any MCP-compatible client.

---

## 🔗 Related Projects & Links

- [ModelContextProtocol C# SDK (NuGet)](https://www.nuget.org/packages/ModelContextProtocol)
- [ModelContextProtocol.AspNetCore (reference)](https://github.com/modelcontextprotocol/csharp-sdk/tree/main/src/ModelContextProtocol.AspNetCore)
- [Official Model Context Protocol Specification](https://spec.modelcontextprotocol.io/)
- [MCP Inspector (Python SDK/CLI tool)](https://github.com/modelcontextprotocol/python-sdk/tree/main)
