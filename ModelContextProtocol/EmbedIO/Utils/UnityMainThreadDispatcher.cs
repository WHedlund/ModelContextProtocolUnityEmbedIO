using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UnityEngine;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static UnityMainThreadDispatcher _instance;
    private static readonly ConcurrentQueue<Action> _queue = new ConcurrentQueue<Action>();

    public static Task RunOnMainThread(Action action)
    {
        EnsureInstance();
        var tcs = new TaskCompletionSource<object>();
        _queue.Enqueue(() =>
        {
            try
            {
                action();
                tcs.SetResult(null);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });
        return tcs.Task;
    }

    // NEW: For returning a value (async/await)
    public static Task<T> RunOnMainThread<T>(Func<T> func)
    {
        EnsureInstance();
        var tcs = new TaskCompletionSource<T>();

        _queue.Enqueue(() =>
        {
            try
            {
                T result = func();
                tcs.SetResult(result);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });

        return tcs.Task;
    }

    // NEW: For async func (if needed)
    public static Task RunOnMainThread(Func<Task> func)
    {
        EnsureInstance();
        var tcs = new TaskCompletionSource<object>();

        _queue.Enqueue(async () =>
        {
            try
            {
                await func();
                tcs.SetResult(null);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });

        return tcs.Task;
    }

    private static void EnsureInstance()
    {
        if (_instance == null || !_instance) // Handles destroyed singleton
        {
            var obj = new GameObject("UnityMainThreadDispatcher");
            _instance = obj.AddComponent<UnityMainThreadDispatcher>();
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

    void Update()
    {
        while (_queue.TryDequeue(out var action))
        {
            action?.Invoke();
        }
    }
}
