using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using static UnityEngine.AddressableAssets.Addressables;
//using UnityEngine.ResourceManagement.ResourceLocations;

public class ResourceManager : Manager<ResourceManager>
{
    private Dictionary<string, AsyncOperationHandle> _activeHandles = new();
    private void AddActiveHandle(string address, AsyncOperationHandle handle)
    {
        if (!_activeHandles.ContainsKey(address))
        {
            _activeHandles.Add(address, handle);
        }
    }

    private void RemoveActiveHandle(string address, AsyncOperationHandle handle)
    {
        if (_activeHandles.ContainsKey(address))
        {
            _activeHandles.Remove(address);
        }
        if( handle.IsValid())handle.Release();
    }

    private bool GetActiveHandle(string address, out AsyncOperationHandle handle)
    {
        if (_activeHandles.TryGetValue(address, out var existingHandle)  )
        {
            if( existingHandle.IsValid())
            {
                handle = existingHandle;
                return true;
            }else
            {
                RemoveActiveHandle(address, existingHandle);
            }
        }
        handle = default;
        return false;
    }


    public void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        AsyncInstantiateOperation.SetIntegrationTimeMS(10f);//设置每帧允许的最大异步实例化集成时间，默认10ms
    }

 
    // 异步检查资源是否存在
    public async UniTask<bool> IsAddressableExists(string address)
    {
        // 加载该地址对应的所有资源位置信息
        var locationsHandle = Addressables.LoadResourceLocationsAsync(address);
        await locationsHandle.Task; // 等待异步操作完成

        // 检查是否获取到有效位置信息
        bool exists = locationsHandle.Result != null && locationsHandle.Result.Count > 0;

        // 释放句柄（避免内存泄漏）
        Addressables.Release(locationsHandle);

        return exists;
    }
 
    /// <summary>
    /// 批量加载资源，使用await方式返回结果
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="addresses"></param>
    /// <param name="handleCacheTime"></param>
    /// <returns></returns>
    public async UniTask<List<T?>> LoadAssetsAsync<T>(IEnumerable<string> addresses, float handleCacheTime = 0) where T : UnityEngine.Object
    {
        List<T?> results = new();
        foreach (var addr in addresses)
        {
            results.Add(await LoadAssetAsync<T>(addr, handleCacheTime));
        }
        return results;
    }

    /// <summary>
    /// 批量加载资源，可在回调返回结果
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="keys">addresse/lable/AssetReference/IResourceLocation</param>
    /// <param name="callback"></param>
    /// <param name="mergeMode">根据所有keys条件 Union:合集, Intersection:交集</param>
    /// <param name="handleCacheTime"></param>
    /// <returns></returns>
    public AsyncOperationHandle<IList<T>> LoadAssetsAsync<T>(IEnumerable<object> keys, Action<T> callback, MergeMode mergeMode = MergeMode.Union, float handleCacheTime = 0)
    {
        if (keys == null)
        {
            throw new ArgumentNullException(nameof(keys), "LoadAssetsAsync Fail,Keys cannot be null.");
        }

        AsyncOperationHandle<IList<T>> handle = Addressables.LoadAssetsAsync<T>(keys, callback, mergeMode, releaseDependenciesOnFailure:true);
        handle.Completed += (op) =>
        {
            this.DelayDoSomething(handleCacheTime, () =>
            {
                Addressables.Release(op);
            });
        };

        return handle; 

    }

    /// <summary>
    /// 加载资源,通过task/await方式返回结果
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="address"></param>
    /// <param name="handleCacheTime">加载句柄缓存多少秒</param>
    /// <returns></returns>
    public async UniTask<T?> LoadAssetAsync<T>(string address, float handleCacheTime = 0) where T : UnityEngine.Object
    {
        // bool isExists = await IsAddressableExists(address);
        // if (!isExists)
        // {
        //     Debug.LogError($"Asset not Exist: {address}");
        //     return null;
        // }
        try
        {
            if (GetActiveHandle(address, out var existingHandle))
            {
                await existingHandle.Task;
                if (existingHandle.IsValid() && existingHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    return existingHandle.Result as T;
                }
            }


            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(address);
            if(handle.OperationException != null)
            {
                //Debug.LogError(handle.OperationException.Message);
                return null;
            }
            AddActiveHandle(address, handle);

            await handle.Task;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _ = UniTask.RunOnThreadPool(async () =>
                {
                    // 延迟指定秒数 
                    await UniTask.Delay(TimeSpan.FromSeconds(handleCacheTime));
                    RemoveActiveHandle(address, handle);
                });

                return handle.Result;
            }
            else
            {
                Debug.LogError($"Load Asset Failed: {address}.Exception:{handle.OperationException?.Message}");
                RemoveActiveHandle(address, handle);
                return null;
            }
 
        }
        catch (Exception ex)
        {
            Debug.LogError($"Release Handle Error: {ex.Message}");
            return null;
        }

    }

    /// <summary>
    /// 加载资源,在协程中通过回调方式返回结果
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="address"></param>
    /// <param name="handleCacheTime">加载句柄缓存多少秒</param>
    /// <returns></returns>
    public  IEnumerator LoadAssetAsync_Coro<T>(string address, Action<T?> callback, float handleCacheTime = 0) where T : UnityEngine.Object
    {

        if (GetActiveHandle(address, out var existingHandle))
        {
            if (!existingHandle.IsDone) yield return existingHandle;
            if (existingHandle.IsValid() && existingHandle.Status == AsyncOperationStatus.Succeeded)
            {
                callback?.Invoke(existingHandle.Result as T);
            }
        }


        AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(address);
        if(handle.OperationException != null)
        {
            //Debug.LogError(handle.OperationException.Message);
            yield break;
        }
        AddActiveHandle(address, handle);

        if (!handle.IsDone) yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            callback?.Invoke(handle.Result);
            yield return new WaitForSeconds(handleCacheTime);
            RemoveActiveHandle(address, handle);
        }
        else
        {
            RemoveActiveHandle(address, handle);
            callback?.Invoke(null);
            Debug.LogError($"Load Asset Failed: {address}.Exception:{handle.OperationException?.Message}");
        }

 
    }

    /// <summary>
    /// 加载资源，使用事件回调方式返回结果
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="address"></param>
    /// <param name="callback"></param>
    /// <param name="handleCacheTime">加载句柄缓存多少秒</param>
    public void LoadAssetAsync<T>(string address, Action<T?> callback, float handleCacheTime = 0) where T : UnityEngine.Object
    {

        try
        {
            if (_activeHandles.TryGetValue(address, out var existingHandle))
            {
                existingHandle.Completed += (op) =>
                {
                    if (op.Status == AsyncOperationStatus.Succeeded)
                    {
                        callback?.Invoke(op.Result as T);
                    }
                    else
                    {
                        callback?.Invoke(null);
                    }
                };
                return;
            }

            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(address);
            if(handle.OperationException != null)
            {
                //Debug.LogError(handle.OperationException.Message);
                callback?.Invoke(null);
                return;
            }
            AddActiveHandle(address, handle);
            handle.Completed += (op) =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    this.DelayDoSomething(handleCacheTime, () =>
                    {
                        RemoveActiveHandle(address, handle);
                    });
                    callback?.Invoke(op.Result);
                }
                else
                {
                    Debug.LogError($"Load Asset Failed: {address}.Exception:{handle.OperationException?.Message}");
                    RemoveActiveHandle(address, handle);
                    callback?.Invoke(null);
                }

            };
        }
        catch (Exception ex)
        {
            Debug.LogError($"Load Asset Error: {ex.Message}");
            return;
        }


    }

    /// <summary>
    /// 同步加载资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="address"></param>
    /// <param name="handleCacheTime"></param>
    /// <returns></returns>
    public T? LoadAssetSync<T>(string address, float handleCacheTime = 0) where T : UnityEngine.Object
    {
        try
        {
            if (_activeHandles.TryGetValue(address, out var existingHandle))
            {
                existingHandle.WaitForCompletion();
                return existingHandle.Result as T;
            }

            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(address);
            if(handle.OperationException != null)
            {
                //Debug.LogError(handle.OperationException.Message);
                return null;
            }
            AddActiveHandle(address, handle);
            handle.WaitForCompletion();

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                this.DelayDoSomething(handleCacheTime, () =>
                {
                    RemoveActiveHandle(address, handle);
                });
            }
            else
            {
                Debug.LogError($"Load Asset Failed: {address}.Exception:{handle.OperationException?.Message}");
                RemoveActiveHandle(address, handle);
            }

            return handle.Result;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Load Asset Error: {ex.Message}");
            return null;
        }


    }


    /// <summary>
    /// 不建议直接用这个进行实例化 ,在未load asset时,如果同时执行多次实例化会导致多个load handle残留
    /// 异步实例化资源对象(一般是对prefab的实例化) 
    /// 数量多时建议先LoadAssetAsync加载好资源后再自行Object.Instantiate (2022.3.2之后已经支持异步实例化了)
    /// </summary>
    /// <param name="address"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public async UniTask<GameObject?> InstantiateAsync(string address, Transform parent)
    {
        var handle = Addressables.InstantiateAsync(address, parent, false, trackHandle: true);//.WaitForCompletion();
        if (handle.OperationException != null)
        {
            return null;
        }
        await handle;
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            return handle.Result;
        }
        return null;
    }

    /// <summary>
    /// 异步加载场景，运行在task/await方式下
    /// </summary>
    /// <param name="key">scenes in build中没有自定义group的可以直接使用sceneName</param>
    /// <param name="loadSceneMode"></param>
    /// <param name="activateOnLoad"></param>
    /// <returns></returns>
    public async UniTask<SceneInstance> LoadSceneAsync(object key, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode = UnityEngine.SceneManagement.LoadSceneMode.Single, bool activateOnLoad = true)
    {
        var handle = Addressables.LoadSceneAsync(key, loadSceneMode, activateOnLoad);
        if (handle.OperationException != null)
        {
            return default;
        }
        SceneInstance s = await handle;
        return s;
    }

    public async UniTask<SceneInstance> LoadSceneAsync(object key)
    {
        return await LoadSceneAsync(key, UnityEngine.SceneManagement.LoadSceneMode.Single, activateOnLoad:true);
    }

    /// <summary>
    /// 异步加载场景，使用回调方式返回结果
    /// </summary>
    /// <param name="key">scenes in build中没有自定义group的可以直接使用sceneName</param>
    /// <param name="loadSceneMode"></param>
    /// <param name="activateOnLoad"></param>
    /// <param name="onLoaded"></param>
    /// <returns></returns>
    public AsyncOperationHandle<SceneInstance> LoadSceneAsync(object key, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode = UnityEngine.SceneManagement.LoadSceneMode.Single, bool activateOnLoad = true, Action<SceneInstance>? onLoaded = null)
    {
        var handle = Addressables.LoadSceneAsync(key, loadSceneMode, activateOnLoad);
        if (handle.OperationException != null)
        {
            onLoaded?.Invoke(default);
            return handle;
        }
        handle.Completed += async (op) =>
        {
            onLoaded?.Invoke(op.Result);
        };
        return handle;

    }

    public AsyncOperationHandle<SceneInstance> LoadSceneAsync(object key, Action<SceneInstance>? onLoaded = null)
    {
        return LoadSceneAsync(key, UnityEngine.SceneManagement.LoadSceneMode.Single, activateOnLoad:true, onLoaded);
    }
    
    /// <summary>
    /// 加载场景,在协程中通过回调方式返回结果
    /// </summary>
    /// <param name="key">scenes in build中没有自定义group的可以直接使用sceneName</param>
    /// <param name="loadSceneMode"></param>
    /// <param name="activateOnLoad"></param>
    /// <param name="onLoaded"></param>
    /// <returns></returns>
    public IEnumerator LoadSceneAsync_Coro(object key, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode = UnityEngine.SceneManagement.LoadSceneMode.Single, bool activateOnLoad = true, Action<SceneInstance>? onLoaded = null)
    {
        var handle = Addressables.LoadSceneAsync(key, loadSceneMode, activateOnLoad);
        if (handle.OperationException != null)
        {
            onLoaded?.Invoke(default);
            yield break;
        }
        yield return handle;

        onLoaded?.Invoke(handle.Result);

    }
    
    public  IEnumerator LoadSceneAsync_Coro(object key, Action<SceneInstance>? onLoaded = null)
    {
        yield return LoadSceneAsync_Coro(key, UnityEngine.SceneManagement.LoadSceneMode.Single, activateOnLoad:true, onLoaded);
    }
}
