using System;
using System.Runtime.CompilerServices;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

// 扩展 AsyncOperationHandle<T>，使其支持 await
// 其实也可以直接 await handle.Task，但这样更简洁,  且WebGL由于不支持多task从而不支持await handle.Task
public static class AsyncOperationHandleAwaiterExtensions
{
    // 为 AsyncOperationHandle<T> 添加 GetAwaiter 方法（C# 可等待对象的核心）
    public static AsyncOperationHandleAwaiter<T> GetAwaiter<T>(this AsyncOperationHandle<T> handle)
    {
        return new AsyncOperationHandleAwaiter<T>(handle);
    }
}

// 实现 awaiter 接口（必须实现 INotifyCompletion 和 GetResult）
public class AsyncOperationHandleAwaiter<T> : INotifyCompletion
{
    private readonly AsyncOperationHandle<T> _handle;
    private Action? _continuation; // 等待完成后的回调

    public AsyncOperationHandleAwaiter(AsyncOperationHandle<T> handle)
    {
        _handle = handle;
        // 监听句柄的完成事件
        _handle.Completed += OnHandleCompleted;
    }

    // 标记是否已完成（C# 会自动检查）
    public bool IsCompleted => _handle.Status != AsyncOperationStatus.None;

    // 等待完成后获取结果（await 表达式的返回值）
    public T GetResult()
    {
        // 若失败，抛出异常（可在调用处用 try-catch 捕获）
        if (_handle.Status == AsyncOperationStatus.Failed)
        {
            throw new Exception($"资源加载失败：{_handle.OperationException.Message}");
        }
        return _handle.Result;
    }

    // 完成后执行回调（C# 会自动调用）
    public void OnCompleted(Action continuation)
    {
        _continuation = continuation;
    }

    // 句柄完成时触发的回调
    private void OnHandleCompleted(AsyncOperationHandle<T> handle)
    {
        _continuation?.Invoke();
    }
}