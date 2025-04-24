using UnityEngine;
using System.Threading.Tasks;

public class DispatcherLoggerTest : MonoBehaviour
{
    async void Start()
    {
        Debug.Log("Starting Dispatcher and Logger Test...");

        // Test dispatcher (Action)
        await UnityMainThreadDispatcher.RunOnMainThread(() =>
        {
            Debug.Log("Dispatcher Action executed on main thread.");
        });

        // Test dispatcher with return value
        var dispatcherResult = await UnityMainThreadDispatcher.RunOnMainThread(() =>
        {
            Debug.Log("Dispatcher Func<T> executed on main thread.");
            return "Dispatcher working";
        });

        Debug.Log($"Dispatcher returned: {dispatcherResult}");

        // Test async dispatcher
        await UnityMainThreadDispatcher.RunOnMainThread(async () =>
        {
            Debug.Log("Async Dispatcher Func<Task> starting on main thread...");
            await Task.Delay(500);
            Debug.Log("Async Dispatcher Func<Task> completed after delay.");
        });

        // Test MainThreadLogger explicitly (assuming your logger exists)
        MainThreadLogger.Log("This is a test log from MainThreadLogger.");
        MainThreadLogger.LogWarning("This is a test warning from MainThreadLogger.");
        MainThreadLogger.LogError("This is a test error from MainThreadLogger.");

        Debug.Log("Dispatcher and Logger Test completed.");
    }
}