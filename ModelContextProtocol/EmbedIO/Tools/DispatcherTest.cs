using ModelContextProtocol.Server;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using UnityEngine;

[McpServerToolType]
public class DispatcherTest : MonoBehaviour
{
    public GameObject spawnedObject;

    // This field simulates the component or property updated from the main thread
    public string mainThreadOnlyText;

    // 1. Spawn a GameObject and set its initial position
    [McpServerTool, Description("Spawn GameObject and set initial transform")]
    public async Task SpawnAndSetupGameObject(string prefabName, Vector3 position)
    {
        await UnityMainThreadDispatcher.RunOnMainThread(() =>
        {
            var prefab = Resources.Load<GameObject>(prefabName);
            if (prefab == null)
                throw new ArgumentException($"Prefab '{prefabName}' not found in Resources.");

            spawnedObject = Instantiate(prefab, position, Quaternion.identity);
        });
    }

    // 2. Set test text synchronously
    [McpServerTool, Description("Test set using sync function")]
    public void SetTestText(string text)
    {
        UnityMainThreadDispatcher.RunOnMainThread(() =>
        {
            mainThreadOnlyText = text;
        });
    }

    // 3. Set test text asynchronously with error handling
    [McpServerTool, Description("Test set using async function")]
    public async Task SetTestTextAsync(string text)
    {
        try
        {
            await UnityMainThreadDispatcher.RunOnMainThread(() =>
            {
                mainThreadOnlyText = text;
            });
        }
        catch (Exception ex)
        {
            MainThreadLogger.LogError($"[SetTestTextAsync] Error: {ex}");
            throw;
        }
    }

    // 4. Get test text asynchronously
    [McpServerTool, Description("Get Test Text Async")]
    public async Task<string> GetTestTextAsync()
    {
        return await UnityMainThreadDispatcher.RunOnMainThread(() => mainThreadOnlyText);
    }

    // 5. Test transformation of spawned object
    [McpServerTool, Description("Test transform of spawned GameObject")]
    public async Task<Vector3> GetSpawnedObjectPositionAsync()
    {
        return await UnityMainThreadDispatcher.RunOnMainThread(() =>
        {
            if (spawnedObject == null)
                throw new InvalidOperationException("Spawned object does not exist.");

            return spawnedObject.transform.position;
        });
    }

    // 6. Test error handling by throwing intentionally from dispatcher
    [McpServerTool, Description("Throw error without internal handling")]
    public async Task ThrowError1()
    {
        MainThreadLogger.LogError($"[SetCommunicationTextAsync] TEST");

        try
        {
            await UnityMainThreadDispatcher.RunOnMainThread(() =>
            {
                throw new InvalidOperationException("Intentionally thrown exception (no internal catch).");
            });
        }
        catch (Exception ex)
        {
            MainThreadLogger.LogError($"[ThrowError1] Exception caught externally: {ex}");
        }
    }

    [McpServerTool, Description("Throw error with internal handling")]
    public async Task ThrowError2()
    {
        Debug.LogError($"[ThrowError2] TEST");

        await UnityMainThreadDispatcher.RunOnMainThread(() =>
        {
            try
            {
                throw new InvalidOperationException("Intentionally thrown exception (caught internally).");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ThrowError2] Exception caught inside dispatcher: {ex}");
                throw; // Optionally propagate
            }
        });
    }
}
