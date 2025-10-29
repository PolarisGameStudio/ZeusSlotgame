using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UniRx.Async;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;

namespace Libs
{
    /// <summary>
    /// 高级 Addressable 资源管理器
    /// </summary>
    public class AddressableManager : MonoSingleton<AddressableManager>
    {
        private bool _isInitialized = false;
        private AsyncOperationHandle _initHandle;
        public bool IsInitialized => _isInitialized;

        #region 字段

        // 存储所有加载的句柄
        private readonly Dictionary<string, AsyncOperationHandle> _handleDict =
            new Dictionary<string, AsyncOperationHandle>();

        // 存储实例化的对象
        private readonly Dictionary<string, GameObject> _instantiatedObjects = new Dictionary<string, GameObject>();

        // 在类字段区域添加
        private readonly Dictionary<string, object> _typedCache = new Dictionary<string, object>();
        private readonly Dictionary<string, List<string>> _labelToAddressMap = new Dictionary<string, List<string>>();

        #endregion

        /// <summary>
        /// 初始化Addressable系统
        /// </summary>
        /// <param name="progressCallback">进度回调(0-1)</param>
        /// <returns>协程迭代器</returns>
        public async Task InitializeAsync(Action<float> progressCallback = null)
        {
            if (_isInitialized)
            {
                progressCallback?.Invoke(1f);
                return;
            }

            try
            {
                // 1. 初始化Addressables系统
                _initHandle = Addressables.InitializeAsync(false);

                // 2. 等待初始化完成并更新进度
                while (!_initHandle.IsDone)
                {
                    progressCallback?.Invoke(_initHandle.PercentComplete * 0.5f);
                    await Task.Yield();
                }

                // 3. 检查初始化结果
                if (_initHandle.Status == AsyncOperationStatus.Failed)
                {
                    throw new Exception("Addressables初始化失败: " + _initHandle.OperationException);
                }

                // 4. 加载目录(可选)
                var catalogsHandle = Addressables.CheckForCatalogUpdates(false);
                while (!catalogsHandle.IsDone)
                {
                    progressCallback?.Invoke(0.5f + catalogsHandle.PercentComplete * 0.5f);
                    await Task.Yield();
                }

                if (catalogsHandle.Status == AsyncOperationStatus.Succeeded &&
                    catalogsHandle.Result != null &&
                    catalogsHandle.Result.Count > 0)
                {
                    // 有更新目录需要下载
                    var updateHandle = Addressables.UpdateCatalogs(catalogsHandle.Result, false);
                    while (!updateHandle.IsDone)
                    {
                        progressCallback?.Invoke(0.5f + updateHandle.PercentComplete * 0.5f);
                        await Task.Yield();
                    }
                }

                Addressables.Release(catalogsHandle);

                _isInitialized = true;
                progressCallback?.Invoke(1f);
            }
            catch (Exception e)
            {
                Debug.LogError($"AddressableManager初始化异常: {e}");
                throw;
            }
        }

        /// <summary>
        /// 检查是否已初始化
        /// </summary>


        #region 公共方法
        public async UniTask<T> LoadAssetAsync<T>(string address) where T : class
        {
            if (string.IsNullOrEmpty(address))
            {
                return null;
            }
    
            // 智能处理地址（根据类型决定是否添加后缀）
            address = ProcessAddressWithExtension<T>(address);

            // 检查缓存（类型安全）
            if (TryGetFromCache<T>(address, out var cachedAsset))
            {
                return cachedAsset;
            }
            try
            {
                var handle = Addressables.LoadAssetAsync<T>(address);
                T asset = await handle.Task;
                return asset;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load asset at address: {address}. Error: {ex.Message}");
                return null;
            }
        }
        
        
        /// <summary>
        /// 加载资源（泛型），支持进度回调
        /// </summary>
        public void LoadAsset<T>(string address, Action<T> onLoaded, Action<string> onFailed = null,
            Action<float> onProgress = null)where T : class
        {
            if (string.IsNullOrEmpty(address))
            {
                onFailed?.Invoke("地址不能为空");
                return;
            }

            // 智能处理地址（根据类型决定是否添加后缀）
            address = ProcessAddressWithExtension<T>(address);

            // 检查缓存（类型安全）
            if (TryGetFromCache<T>(address, out var cachedAsset))
            {
                onLoaded?.Invoke(cachedAsset);
                return;
            }

            // 检查是否已加载
            if (_handleDict.TryGetValue(address, out var existingHandle))
            {
                if (existingHandle.IsDone)
                {
                    if (existingHandle.Status == AsyncOperationStatus.Succeeded)
                    {
                        AddToCache(address,(T)existingHandle.Result);
                        onLoaded?.Invoke((T)existingHandle.Result);
                    }
                    else
                    {
                        onFailed?.Invoke($"资源加载失败: {address}");
                    }
                }
                else
                {
                    // 如果正在加载，添加进度回调
                    existingHandle.Completed += handle =>
                    {
                        if (handle.Status == AsyncOperationStatus.Succeeded)
                        {
                            AddToCache(address,(T)handle.Result);
                            onLoaded?.Invoke((T)handle.Result);
                        }
                        else
                        {
                            onFailed?.Invoke($"资源加载失败: {address}");
                        }
                    };
                }

                return;
            }

            // 开始加载
            var handle = Addressables.LoadAssetAsync<T>(address);
            _handleDict[address] = handle;

            // 添加进度回调
            if (onProgress != null)
            {
                StartCoroutine(ReportProgress(handle, onProgress));
            }

            handle.Completed += op =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    // 添加到缓存
                    AddToCache(address,op.Result);
                    onLoaded?.Invoke(op.Result);
                }
                else
                {
                    var errorMsg = $"[Addressable] 加载失败: {address}";
                    Debug.LogError(errorMsg);
                    onFailed?.Invoke(errorMsg);
                    _handleDict.Remove(address);
                }
            };
        }

        /// <summary>
        /// 报告加载进度
        /// </summary>
        private IEnumerator ReportProgress(AsyncOperationHandle handle, Action<float> onProgress)
        {
            while (!handle.IsDone)
            {
                onProgress?.Invoke(handle.PercentComplete);
                yield return null;
            }
        }

        /// <summary>
        /// 根据资源类型处理地址后缀
        /// </summary>
        public string ProcessAddressWithExtension<T>(string address)
        {
            // 如果地址已经包含扩展名，直接返回
            if (Path.HasExtension(address))
            {
                return address;
            }

            // 根据类型添加推荐扩展名
            return typeof(T) switch
            {
                // 预制件
                _ when typeof(T) == typeof(GameObject) => $"{address}.prefab",

                // 图片
                _ when typeof(T) == typeof(Texture2D) => $"{address}.png",
                _ when typeof(T) == typeof(Sprite) => $"{address}.png",

                // 音频
                _ when typeof(T) == typeof(AudioClip) => $"{address}.mp3",

                // 文本
                _ when typeof(T) == typeof(TextAsset) => $"{address}.txt",

                // 其他类型不添加后缀
                _ => address
            };
        }

        /// <summary>
        /// 加载场景
        /// </summary>
        public void LoadScene(string address, LoadSceneMode mode = LoadSceneMode.Single,
            Action onLoaded = null, Action<string> onFailed = null, Action<float> onProgress = null)
        {
            if (string.IsNullOrEmpty(address))
            {
                onFailed?.Invoke("场景地址不能为空");
                return;
            }

            var handle = Addressables.LoadSceneAsync(address, mode);
            _handleDict[address] = handle;

            if (onProgress != null)
            {
                StartCoroutine(ReportProgress(handle, onProgress));
            }

            handle.Completed += op =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    onLoaded?.Invoke();
                }
                else
                {
                    var errorMsg = $"[Addressable] 场景加载失败: {address}";
                    Debug.LogError(errorMsg);
                    onFailed?.Invoke(errorMsg);
                    _handleDict.Remove(address);
                }
            };
        }

        /// <summary>
        /// 协程方式加载场景（增强版）
        /// </summary>
        /// <param name="sceneKey">场景地址</param>
        /// <param name="loadMode">加载模式</param>
        /// <param name="activateOnLoad">是否立即激活</param>
        /// <param name="priority">加载优先级</param>
        /// <param name="onLoaded">加载完成回调</param>
        /// <param name="onProgress">进度回调</param>
        /// <param name="onFailed">失败回调</param>
        /// <param name="progressWeight">进度权重(0-1)</param>
        public IEnumerator LoadSceneCor(
            string sceneKey,
            LoadSceneMode loadMode = LoadSceneMode.Single,
            bool activateOnLoad = true,
            int priority = 100,
            Action<SceneInstance> onLoaded = null,
            Action<float> onProgress = null,
            Action<string> onFailed = null,
            float progressWeight = 1.0f)
        {
            if (string.IsNullOrEmpty(sceneKey))
            {
                onFailed?.Invoke("场景key不能为空");
                yield break;
            }

            // 生成唯一标识key
            string handleKey = $"Scene_{sceneKey}";

            // 检查是否已加载
            if (_handleDict.TryGetValue(handleKey, out var existingHandle))
            {
                // 等待已有操作完成
                while (!existingHandle.IsDone)
                {
                    onProgress?.Invoke(existingHandle.PercentComplete * progressWeight);
                    yield return null;
                }

                // 处理结果
                if (existingHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    var sceneInstance = (SceneInstance)existingHandle.Result;
                    onLoaded?.Invoke(sceneInstance);
                }
                else
                {
                    onFailed?.Invoke($"场景加载失败: {sceneKey}");
                }

                yield break;
            }

            // 开始新场景加载
            var loadHandle = Addressables.LoadSceneAsync(
                sceneKey,
                loadMode,
                activateOnLoad,
                priority);

            _handleDict[handleKey] = loadHandle;

            // 更新加载进度
            while (!loadHandle.IsDone)
            {
                try
                {
                    onProgress?.Invoke(loadHandle.PercentComplete * progressWeight);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"进度回调异常: {ex.Message}");
                }

                yield return null;
            }

            // 处理加载结果
            if (loadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                var sceneInstance = loadHandle.Result;
                try
                {
                    onLoaded?.Invoke(sceneInstance);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"加载回调异常: {ex.Message}");
                }
            }
            else
            {
                string errorMsg = $"场景加载失败: {sceneKey} - {loadHandle.OperationException?.Message}";
                Debug.LogError(errorMsg);
                _handleDict.Remove(handleKey);
                onFailed?.Invoke(errorMsg);
            }
        }


        /// <summary>
        /// 实例化对象
        /// </summary>
        public void Instantiate(string address, Transform parent = null, bool worldPositionStays = false,
            Action<GameObject> onLoaded = null, Action<float> onProgress = null, Action<string> onFailed = null)
        {
            if (string.IsNullOrEmpty(address))
            {
                onFailed?.Invoke("实例化地址不能为空");
                return;
            }

            var handle = Addressables.InstantiateAsync(address, parent, worldPositionStays);
            _handleDict[address] = handle;

            if (onProgress != null)
            {
                StartCoroutine(ReportProgress(handle, onProgress));
            }

            handle.Completed += op =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    _instantiatedObjects[address] = op.Result;
                    onLoaded?.Invoke(op.Result);
                }
                else
                {
                    var errorMsg = $"[Addressable] 实例化失败: {address}";
                    Debug.LogError(errorMsg);
                    onFailed?.Invoke(errorMsg);
                    _handleDict.Remove(address);
                }
            };
        }


        #region 标签加载功能

        /// <summary>
        /// 协程方式按标签加载资源（带缓存检查）
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="label">资源标签</param>
        /// <param name="onLoaded">加载完成回调</param>
        /// <param name="onFailed">失败回调</param>
        /// <param name="onProgress">进度回调</param>
        public IEnumerator LoadAssetsByLabelCoroutine<T>(string label,
            Action<IList<T>> onLoaded = null,
            Action<string> onFailed = null,
            Action<float> onProgress = null) where T : class
        {
            if (string.IsNullOrEmpty(label))
            {
                onFailed?.Invoke("标签不能为空");
                yield break;
            }

            string handleKey = $"Label_{label}_{typeof(T).Name}";

            // 检查是否已缓存
            if (_labelToAddressMap.TryGetValue(label, out var cachedAddresses))
            {
                var results = new List<T>();
                foreach (var addr in cachedAddresses)
                {
                    if (TryGetFromCache<T>(addr, out var cachedObj))
                    {
                        results.Add(cachedObj);
                    }
                }
        
                if (results.Count > 0)
                {
                    onLoaded?.Invoke(results);
                    yield break;
                }
            }
            
            // 检查缓存中是否已有加载操作
            if (_handleDict.TryGetValue(handleKey, out var existingHandle))
            {
                // 等待已有操作完成
                while (!existingHandle.IsDone)
                {
                    onProgress?.Invoke(existingHandle.PercentComplete);
                    yield return null;
                }

                // 处理完成结果
                if (existingHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    // 缓存标签资源
                    var addresses = new List<string>();
                    foreach (var item in existingHandle.Result as IList<T>)
                    {
                        var address = GetAddressFromAsset(item);
                        AddToCache(address,item);
                        addresses.Add(address);
                    }
                    _labelToAddressMap[label] = addresses;
                    onLoaded?.Invoke((IList<T>)existingHandle.Result);
                }
                else
                {
                    string errorMsg = $"标签资源加载失败: {label}";
                    Debug.LogError(errorMsg);
                    onFailed?.Invoke(errorMsg);
                    _handleDict.Remove(handleKey); // 移除失败的加载记录
                }

                yield break;
            }

            // 创建新的加载操作
            var loadHandle = Addressables.LoadAssetsAsync<T>(label, null);
            _handleDict[handleKey] = loadHandle;

            // 更新进度
            while (!loadHandle.IsDone)
            {
                try
                {
                    onProgress?.Invoke(loadHandle.PercentComplete);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"进度回调异常: {ex.Message}");
                }

                yield return null;
            }

            // 处理加载结果
            if (loadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                try
                {
                    // 将资源添加到缓存
                    var result = loadHandle.Result;
                    // 缓存标签资源
                    var addresses = new List<string>();
                    foreach (var item in result)
                    {
                        var address = GetAddressFromAsset(item);
                        AddToCache(address,item);
                        addresses.Add(address);
                    }
                    _labelToAddressMap[label] = addresses;
                    onLoaded?.Invoke(result);
                }
                catch (Exception ex)
                {
                    string errorMsg = $"资源回调处理异常: {ex.Message}";
                    Debug.LogError(errorMsg);
                    onFailed?.Invoke(errorMsg);
                    _handleDict.Remove(handleKey);
                }
            }
            else
            {
                string errorMsg = $"标签资源加载失败: {label} - {loadHandle.OperationException}";
                Debug.LogError(errorMsg);
                onFailed?.Invoke(errorMsg);
                _handleDict.Remove(handleKey);
            }

            // 注意：这里不释放handle，因为我们需要保持资源引用
        }


        /// <summary>
        /// 通过标签加载资源列表（泛型）
        /// </summary>
        public void LoadAssetsByLabel<T>(string label, Action<IList<T>> onLoaded,
            Action<string> onFailed = null, Action<float> onProgress = null)where T:class
        {
            if (string.IsNullOrEmpty(label))
            {
                onFailed?.Invoke("标签不能为空");
                return;
            }

            // 生成唯一标识key
            string handleKey = $"Label_{label}_{typeof(T).Name}";

            // 检查是否已加载
            if (_handleDict.TryGetValue(handleKey, out var existingHandle))
            {
                if (existingHandle.IsDone)
                {
                    if (existingHandle.Status == AsyncOperationStatus.Succeeded)
                    {
                        // 将资源添加到缓存
                        var result = (IList<T>)existingHandle.Result;
                        // 缓存标签资源
                        var addresses = new List<string>();
                        foreach (var item in result)
                        {
                            var address = GetAddressFromAsset(item);
                            AddToCache(address,item);
                            addresses.Add(address);
                        }
                        _labelToAddressMap[label] = addresses;
                        onLoaded?.Invoke(result);
                    }
                    else
                    {
                        onFailed?.Invoke($"标签资源加载失败: {label}");
                    }
                }
                else
                {
                    existingHandle.Completed += handle =>
                    {
                        if (handle.Status == AsyncOperationStatus.Succeeded)
                        {
                            // 将资源添加到缓存
                            var result = (IList<T>)handle.Result;
                            // 缓存标签资源
                            var addresses = new List<string>();
                            foreach (var item in result)
                            {
                                var address = GetAddressFromAsset(item);
                                AddToCache(address,item);
                                addresses.Add(address);
                            }
                            _labelToAddressMap[label] = addresses;
                            onLoaded?.Invoke(result);
                        }
                        else
                        {
                            onFailed?.Invoke($"标签资源加载失败: {label}");
                        }
                    };
                }

                return;
            }

            // 开始加载
            var handle = Addressables.LoadAssetsAsync<T>(label, null);
            _handleDict[handleKey] = handle;

            // 进度报告
            if (onProgress != null)
            {
                StartCoroutine(ReportProgress(handle, onProgress));
            }

            handle.Completed += op =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    // 将资源添加到缓存
                    var result = (IList<T>)op.Result;
                    // 缓存标签资源
                    var addresses = new List<string>();
                    foreach (var item in result)
                    {
                        var address = GetAddressFromAsset(item);
                        AddToCache(address,item);
                        addresses.Add(address);
                    }
                    _labelToAddressMap[label] = addresses;
                    onLoaded?.Invoke(result);
                }
                else
                {
                    var errorMsg = $"[Addressable] 标签加载失败: {label}";
                    Debug.LogError(errorMsg);
                    onFailed?.Invoke(errorMsg);
                    _handleDict.Remove(handleKey);
                }
            };
        }

        /// <summary>
        /// 通过标签实例化所有对象
        /// </summary>
        public void InstantiateByLabel(string label, Transform parent = null,
            bool worldPositionStays = false, Action<List<GameObject>> onLoaded = null,
            Action<float> onProgress = null, Action<string> onFailed = null)
        {
            LoadAssetsByLabel<GameObject>(label, (prefabs) =>
            {
                var instances = new List<GameObject>();
                foreach (var prefab in prefabs)
                {
                    var instance = Instantiate(prefab, parent, worldPositionStays);
                    _instantiatedObjects.Add($"{label}_{instance.GetInstanceID()}", instance);
                    instances.Add(instance);
                }

                onLoaded?.Invoke(instances);
            }, onFailed, onProgress);
        }

        /// <summary>
        /// 释放标签加载的所有资源
        /// </summary>
        public void ReleaseLabelAssets(string label, Type assetType = null)
        {
            string prefix = $"Label_{label}_";
            if (assetType != null)
            {
                prefix += assetType.Name;
            }

            List<string> keysToRemove = new List<string>();
            foreach (var kvp in _handleDict)
            {
                if (kvp.Key.StartsWith(prefix))
                {
                    Addressables.Release(kvp.Value);
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                _handleDict.Remove(key);
            }
        }

        #endregion
        private string GetAddressFromAsset<T>(T asset)
        {
            // 这里需要根据实际情况实现，可能需要使用Addressables.GetDownloadSizeAsync等
            // 简化版实现：
            return asset switch
            {
                UnityEngine.Object obj => obj.name,
                _ => Guid.NewGuid().ToString() // 非Unity对象的临时方案
            };
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Release(string address)
        {
            if (_handleDict.TryGetValue(address, out var handle))
            {
                Addressables.Release(handle);
                _handleDict.Remove(address);
            }
        }

        /// <summary>
        /// 释放实例化对象
        /// </summary>
        public void ReleaseInstance(string address)
        {
            if (_instantiatedObjects.TryGetValue(address, out var obj))
            {
                Addressables.ReleaseInstance(obj);
                _instantiatedObjects.Remove(address);
            }
        }

        /// <summary>
        /// 获取已实例化的对象数量
        /// </summary>
        public int GetInstantiatedObjectCount()
        {
            return _instantiatedObjects.Count;
        }

        #endregion

        #region 析构

        public override void Dispose()
        {
            // 释放所有资源
            foreach (var handle in _handleDict.Values)
            {
                Addressables.Release(handle);
            }

            _handleDict.Clear();

            // 释放所有实例化对象
            foreach (var obj in _instantiatedObjects.Values)
            {
                Addressables.ReleaseInstance(obj);
            }

            _instantiatedObjects.Clear();

            base.Dispose();
        }

        #endregion
        
        // 添加类型安全的缓存方法
        private bool TryGetFromCache<T>(string address, out T asset) where T:class
        {
            if (_typedCache.TryGetValue(address, out var cacheEntry))
            {
                if (cacheEntry is ResourceCacheEntry<T> typedEntry)
                {
                    typedEntry.LastAccessTime = DateTime.Now;
                    asset = typedEntry.Asset;
                    Debug.Log($"TryGetFromCache address:{address}");
                    return true;
                }
            }
            asset = null;
            return false;
        }

        private void AddToCache<T>(string address, T asset) where T : class
        {
            Debug.Log($"AddToCache address:{address}");
            _typedCache[address] = new ResourceCacheEntry<T>(asset);
        }
    }
}