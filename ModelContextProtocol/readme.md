# Unity EmbedIO Extensions for the MCP C# SDK

🚧 **Preview only** — this is an experimental Unity-based implementation of the [Model Context Protocol (MCP)](https://modelcontextprotocol.io/) using [EmbedIO](https://github.com/unosquare/embedio) as a lightweight HTTP server.

> [!NOTE]
> This project is not officially supported by the MCP maintainers and is community-driven.

## About

This package enables Unity projects to host a fully functional MCP server using EmbedIO and the official [`ModelContextProtocol`](https://www.nuget.org/packages/ModelContextProtocol) C# SDK. It provides:

- 🧩 Tool support using `McpServerTool`
- 🌐 SSE transport via EmbedIO
- ⚙️ Lightweight, self-hosted HTTP server compatible with MCP clients

---

## 🧠 Installation in Unity (Recommended)

The easiest way to install dependencies in Unity is using **[NuGet for Unity](https://github.com/GlitchEnzo/NuGetForUnity)**:

1. Install NuGetForUnity (clone or import as Unity package)
2. Open Unity → **NuGet** → **Manage NuGet Packages**
3. Enable **Show Prerelease Packages** in settings (⚠️ very important)
4. Install:

   - `ModelContextProtocol` (`--prerelease`)
   - `EmbedIO`

✅ You're now ready to build an MCP server in Unity.

---

## 🛠 Getting Started

1. Add the following files to your project under `Assets/ModelContextProtocol/EmbedIO/`:

```
EmbedIOApiController.cs
EmbedIOTransport.cs
EmbedIOServerHost.cs
Tools/EchoTool.cs
```

2. Create a new GameObject in your Unity scene and attach the `EmbedIOServerHost` component.

3. Press Play. The MCP server will start at:

```
http://localhost:8888/api/sse
```

You can now connect to it using any MCP-compliant client (e.g., via Python).

---

## 🧪 Example Tool

```csharp
[McpServerToolType]
public static class EchoTool
{
    [McpServerTool(Description = "Echoes the input message")]
    public static string Echo(string message) => $"Echo: {message}";
}
```

Registered tools are automatically listed via the `list_tools` request and can be invoked via `call_tool`.

---

## 🧰 Adding More Tools

You can register multiple tools cleanly via a helper:

```csharp
ToolRegistry.RegisterToolsFromType(typeof(MyToolClass));
```

Or create a helper that aggregates and injects tools into the server options.

---

## 📂 Folder Structure

```
Assets/
└── ModelContextProtocol/
    └── EmbedIO/
        ├── EmbedIOServerHost.cs          # Unity MonoBehaviour to start the server
        ├── EmbedIOApiController.cs       # Handles /sse and /message endpoints
        ├── EmbedIOTransport.cs           # Custom ITransport for SSE
        └── Tools/
            └── EchoTool.cs               # Sample tool implementation
```

---

## Related Projects

- [ModelContextProtocol.AspNetCore](https://github.com/modelcontextprotocol/csharp-sdk/tree/main/src/ModelContextProtocol.AspNetCore)
- [Official Protocol Spec](https://spec.modelcontextprotocol.io/)

---

## License

x