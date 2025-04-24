using UnityEngine;
using System.Collections.Concurrent;

/// <summary>
/// Thread-safe logger for Unity that ensures logs from any thread
/// (background or main) are flushed on the Unity main thread,
/// so they always appear in the Console.
/// </summary>
public class MainThreadLogger : MonoBehaviour
{
    private static MainThreadLogger _instance;
    private static readonly ConcurrentQueue<LogItem> logQueue = new();

    public enum LogType
    {
        Info,
        Warning,
        Error
    }

    private struct LogItem
    {
        public string Message;
        public LogType Type;
    }

    /// <summary>
    /// Log a message from any thread.
    /// </summary>
    public static void LogFromAnyThread(string message, LogType type = LogType.Info)
    {
        EnsureInstance();
        logQueue.Enqueue(new LogItem { Message = message, Type = type });
    }

    // Shortcuts
    public static void Log(string message) => LogFromAnyThread(message, LogType.Info);
    public static void LogWarning(string message) => LogFromAnyThread(message, LogType.Warning);
    public static void LogError(string message) => LogFromAnyThread(message, LogType.Error);

    void Update()
    {
        while (logQueue.TryDequeue(out var logItem))
        {
            switch (logItem.Type)
            {
                case LogType.Info:
                    Debug.Log(logItem.Message);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(logItem.Message);
                    break;
                case LogType.Error:
                    Debug.LogError(logItem.Message);
                    break;
            }
        }
    }

    private static void EnsureInstance()
    {
        if (_instance == null || !_instance)
        {
            var obj = new GameObject("MainThreadLogger");
            _instance = obj.AddComponent<MainThreadLogger>();
            DontDestroyOnLoad(obj);
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
}
