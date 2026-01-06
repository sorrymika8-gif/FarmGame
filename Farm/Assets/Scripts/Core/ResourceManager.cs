using System;
using System.Collections.Generic;
using UnityEngine;
using QFramework;

namespace FarmGame.Core
{
    /// <summary>
    /// 通用资源管理器
    /// 封装 QFramework ResKit，提供资源加载、卸载、预制体对象池管理
    /// </summary>
    public class ResourceManager : MonoSingleton<ResourceManager>
    {
        #region 私有字段

        private const string RESOURCES_PREFIX = "resources://";
        private ResLoader mResLoader;
        private Dictionary<string, SimpleObjectPool<GameObject>> mPrefabPools;
        private bool mIsInitialized;

        #endregion

        #region 生命周期

        /// <summary>
        /// 初始化资源管理器（需要在游戏启动时显式调用）
        /// </summary>
        public void Initialize()
        {
            if (mIsInitialized) return;

            // 初始化 ResKit
            ResKit.Init();

            // 分配全局资源加载器
            mResLoader = ResLoader.Allocate();

            // 验证 ResLoader 是否分配成功
            if (mResLoader == null)
            {
                Debug.LogError("[ResourceManager] Failed to allocate ResLoader! ResKit may not be properly initialized.");
                return;
            }

            // 初始化对象池字典
            mPrefabPools = new Dictionary<string, SimpleObjectPool<GameObject>>();

            mIsInitialized = true;
            Debug.Log("[ResourceManager] Initialized successfully");
        }

        private void OnDestroy()
        {
            Dispose();
        }

        private void Dispose()
        {
            if (!mIsInitialized) return;

            // 清理所有对象池
            ClearAllPools();

            // 回收资源加载器（会自动释放所有资源）
            mResLoader?.Recycle2Cache();
            mResLoader = null;

            mIsInitialized = false;
        }

        #endregion

        #region 公共接口 - 资源加载

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <typeparam name="T">资源类型（Texture2D, AudioClip, Material, TextAsset, GameObject 等）</typeparam>
        /// <param name="assetPath">资源路径（Resources 下的相对路径，不含扩展名）</param>
        /// <returns>加载的资源，失败返回 null</returns>
        public T Load<T>(string assetPath) where T : UnityEngine.Object
        {
            if (!ValidateLoad(assetPath)) return null;
            
            var formattedPath = FormatResourcePath(assetPath);
            try
            {
                var asset = mResLoader.LoadSync<T>(formattedPath);
                if (asset == null)
                {
                    Debug.LogError($"[ResourceManager] Failed to load resource: {assetPath}. Make sure the asset exists in Resources folder.");
                }
                return asset;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ResourceManager] Exception while loading resource '{assetPath}': {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="assetPath">资源路径</param>
        /// <param name="onComplete">加载完成回调</param>
        public void LoadAsync<T>(string assetPath, Action<T> onComplete) where T : UnityEngine.Object
        {
            if (!ValidateLoad(assetPath))
            {
                onComplete?.Invoke(null);
                return;
            }

            var formattedPath = FormatResourcePath(assetPath);
            mResLoader.Add2Load<T>(formattedPath);
            mResLoader.LoadAsync(() =>
            {
                var asset = mResLoader.LoadSync<T>(formattedPath);
                onComplete?.Invoke(asset);
            });
        }

        /// <summary>
        /// 批量异步加载资源
        /// </summary>
        /// <param name="assetPaths">资源路径数组</param>
        /// <param name="onComplete">全部加载完成回调</param>
        public void LoadMultipleAsync(string[] assetPaths, Action onComplete)
        {
            if (assetPaths == null || assetPaths.Length == 0)
            {
                onComplete?.Invoke();
                return;
            }

            foreach (var path in assetPaths)
            {
                if (!string.IsNullOrEmpty(path))
                {
                    mResLoader.Add2Load(FormatResourcePath(path));
                }
            }

            mResLoader.LoadAsync(onComplete);
        }

        /// <summary>
        /// 预加载资源（后台加载，不返回结果）
        /// </summary>
        /// <param name="assetPaths">资源路径</param>
        public void Preload(params string[] assetPaths)
        {
            LoadMultipleAsync(assetPaths, null);
        }

        #endregion

        #region 公共接口 - 资源释放

        /// <summary>
        /// 释放指定资源
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        public void Release(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath)) return;
            mResLoader.ReleaseRes(assetPath);
        }

        /// <summary>
        /// 释放所有资源
        /// </summary>
        public void ReleaseAll()
        {
            mResLoader.ReleaseAllRes();
        }

        #endregion

        #region 公共接口 - 预制体对象池

        /// <summary>
        /// 从对象池生成 GameObject
        /// </summary>
        /// <param name="prefabPath">预制体路径</param>
        /// <param name="parent">父物体</param>
        /// <returns>生成的 GameObject</returns>
        public GameObject Spawn(string prefabPath, Transform parent = null)
        {
            if (!ValidateLoad(prefabPath)) return null;

            var pool = GetOrCreatePool(prefabPath);
            if (pool == null) return null;

            var go = pool.Allocate();
            if (go != null)
            {
                go.transform.SetParent(parent);
                go.SetActive(true);
            }

            return go;
        }

        /// <summary>
        /// 从对象池生成 GameObject 并设置位置和旋转
        /// </summary>
        /// <param name="prefabPath">预制体路径</param>
        /// <param name="position">位置</param>
        /// <param name="rotation">旋转</param>
        /// <param name="parent">父物体</param>
        /// <returns>生成的 GameObject</returns>
        public GameObject Spawn(string prefabPath, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            var go = Spawn(prefabPath, parent);
            if (go != null)
            {
                go.transform.position = position;
                go.transform.rotation = rotation;
            }

            return go;
        }

        /// <summary>
        /// 回收 GameObject 到对象池
        /// </summary>
        /// <param name="prefabPath">预制体路径（必须与 Spawn 时使用的路径一致）</param>
        /// <param name="obj">要回收的对象</param>
        public void Despawn(string prefabPath, GameObject obj)
        {
            if (obj == null) return;

            var formattedPath = FormatResourcePath(prefabPath);
            if (string.IsNullOrEmpty(formattedPath) || !mPrefabPools.TryGetValue(formattedPath, out var pool))
            {
                Debug.LogWarning($"[ResourceManager] Despawn: pool not found for '{prefabPath}', destroying object");
                Destroy(obj);
                return;
            }

            obj.SetActive(false);
            obj.transform.SetParent(transform);
            pool.Recycle(obj);
        }

        /// <summary>
        /// 预热对象池
        /// </summary>
        /// <param name="prefabPath">预制体路径</param>
        /// <param name="count">预创建数量</param>
        public void PrewarmPool(string prefabPath, int count)
        {
            if (string.IsNullOrEmpty(prefabPath) || count <= 0) return;

            var pool = GetOrCreatePool(prefabPath);
            if (pool == null) return;

            for (int i = 0; i < count; i++)
            {
                var go = pool.Allocate();
                if (go != null)
                {
                    go.SetActive(false);
                    go.transform.SetParent(transform);
                    pool.Recycle(go);
                }
            }
        }

        /// <summary>
        /// 清理指定对象池
        /// </summary>
        /// <param name="prefabPath">预制体路径</param>
        public void ClearPool(string prefabPath)
        {
            if (string.IsNullOrEmpty(prefabPath)) return;

            if (mPrefabPools.TryGetValue(prefabPath, out var pool))
            {
                pool.Clear(obj =>
                {
                    if (obj != null) Destroy(obj);
                });
                mPrefabPools.Remove(prefabPath);
            }
        }

        /// <summary>
        /// 清理所有对象池
        /// </summary>
        public void ClearAllPools()
        {
            if (mPrefabPools == null) return;

            foreach (var kvp in mPrefabPools)
            {
                kvp.Value.Clear(obj =>
                {
                    if (obj != null) Destroy(obj);
                });
            }

            mPrefabPools.Clear();
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 格式化资源路径，自动添加 resources:// 前缀
        /// </summary>
        private string FormatResourcePath(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath)) return assetPath;
            
            // 如果已经有前缀则不重复添加
            if (assetPath.StartsWith(RESOURCES_PREFIX))
            {
                return assetPath;
            }
            
            return RESOURCES_PREFIX + assetPath;
        }

        private bool ValidateLoad(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogError("[ResourceManager] Load failed: assetPath is null or empty");
                return false;
            }

            if (!mIsInitialized)
            {
                Debug.LogError("[ResourceManager] Load failed: ResourceManager is not initialized");
                return false;
            }

            if (mResLoader == null)
            {
                Debug.LogError("[ResourceManager] Load failed: mResLoader is null. ResLoader.Allocate() may have failed.");
                return false;
            }

            return true;
        }

        private SimpleObjectPool<GameObject> GetOrCreatePool(string prefabPath)
        {
            var formattedPath = FormatResourcePath(prefabPath);
            
            if (mPrefabPools.TryGetValue(formattedPath, out var pool))
            {
                return pool;
            }

            // 加载预制体
            var prefab = mResLoader.LoadSync<GameObject>(formattedPath);
            if (prefab == null)
            {
                Debug.LogError($"[ResourceManager] GetOrCreatePool failed: prefab not found at '{prefabPath}'");
                return null;
            }

            // 创建对象池
            pool = new SimpleObjectPool<GameObject>(
                () => Instantiate(prefab),
                OnRecyclePoolObject,
                initCount: 0
            );

            mPrefabPools[formattedPath] = pool;
            return pool;
        }

        private void OnRecyclePoolObject(GameObject obj)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }

        #endregion
    }
}
