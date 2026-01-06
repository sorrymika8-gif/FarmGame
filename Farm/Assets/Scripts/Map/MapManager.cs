using UnityEngine;
using QFramework;

namespace FarmGame.Map
{
    /// <summary>
    /// 地图管理器
    /// 负责地图资源的加载、卸载和坐标转换
    /// </summary>
    public class MapManager : MonoSingleton<MapManager>
    {
        #region 私有字段

        private bool mIsInitialized;
        private GameObject mCurrentMap;         // 当前加载的地图实例
        private Transform mMapRoot;             // 地图根节点
        private float mTileSize = 1f;           // 单个地块的世界尺寸

        private const string MAP_DIR = "maps/";

        #endregion

        #region 公共属性

        /// <summary>
        /// 当前地图实例
        /// </summary>
        public GameObject CurrentMap => mCurrentMap;

        /// <summary>
        /// 地块尺寸
        /// </summary>
        public float TileSize
        {
            get => mTileSize;
            set => mTileSize = value;
        }

        #endregion

        #region 生命周期

        /// <summary>
        /// 初始化地图管理器
        /// </summary>
        public void Initialize()
        {
            if (mIsInitialized) return;

            // 创建地图根节点
            mMapRoot = new GameObject("MapRoot").transform;
            mMapRoot.SetParent(transform);

            mIsInitialized = true;
            Debug.Log("[MapManager] Initialized");
        }

        private void OnDestroy()
        {
            Dispose();
        }

        private void Dispose()
        {
            if (!mIsInitialized) return;

            UnloadMap();
            mIsInitialized = false;
        }

        #endregion

        #region 公共接口 - 地图加载

        /// <summary>
        /// 加载地图
        /// </summary>
        /// <param name="mapPath">地图资源路径（相对于Resources文件夹，如 "Maps/FarmMap"）</param>
        /// <returns>是否加载成功</returns>
        public bool LoadMap(string mapPath)
        {
            if (!ValidateInitialized()) return false;

            // 先卸载当前地图
            UnloadMap();

            // 通过ResourceManager加载地图预制体
            GameObject mapPrefab = Core.ResourceManager.Instance.Load<GameObject>(MAP_DIR + mapPath);
            if (mapPrefab == null)
            {
                Debug.LogError($"[MapManager] Failed to load map: {mapPath}");
                return false;
            }

            // 实例化地图
            mCurrentMap = Instantiate(mapPrefab, mMapRoot);
            mCurrentMap.name = mapPrefab.name;

            Debug.Log($"[MapManager] Map loaded: {mapPath}");
            return true;
        }

        /// <summary>
        /// 异步加载地图
        /// </summary>
        /// <param name="mapPath">地图资源路径</param>
        /// <param name="onComplete">加载完成回调</param>
        public void LoadMapAsync(string mapPath, System.Action<bool> onComplete = null)
        {
            if (!ValidateInitialized())
            {
                onComplete?.Invoke(false);
                return;
            }

            // 先卸载当前地图
            UnloadMap();

            // 通过ResourceManager异步加载地图预制体
            Core.ResourceManager.Instance.LoadAsync<GameObject>(mapPath, (mapPrefab) =>
            {
                if (mapPrefab == null)
                {
                    Debug.LogError($"[MapManager] Failed to load map async: {mapPath}");
                    onComplete?.Invoke(false);
                    return;
                }

                // 实例化地图
                mCurrentMap = Instantiate(mapPrefab, mMapRoot);
                mCurrentMap.name = mapPrefab.name;

                Debug.Log($"[MapManager] Map loaded async: {mapPath}");
                onComplete?.Invoke(true);
            });
        }

        /// <summary>
        /// 卸载当前地图
        /// </summary>
        public void UnloadMap()
        {
            if (mCurrentMap != null)
            {
                Destroy(mCurrentMap);
                mCurrentMap = null;
                Debug.Log("[MapManager] Map unloaded");
            }
        }

        #endregion

        #region 公共接口 - 坐标转换

        /// <summary>
        /// 世界坐标转网格坐标
        /// </summary>
        /// <param name="worldPos">世界坐标</param>
        /// <returns>网格坐标</returns>
        public Vector2Int WorldToGrid(Vector3 worldPos)
        {
            int x = Mathf.FloorToInt(worldPos.x / mTileSize);
            int y = Mathf.FloorToInt(worldPos.z / mTileSize);
            return new Vector2Int(x, y);
        }

        /// <summary>
        /// 网格坐标转世界坐标（返回地块中心点）
        /// </summary>
        /// <param name="x">网格X坐标</param>
        /// <param name="y">网格Y坐标</param>
        /// <returns>世界坐标（地块中心）</returns>
        public Vector3 GridToWorld(int x, int y)
        {
            return new Vector3(
                x * mTileSize + mTileSize * 0.5f,
                0,
                y * mTileSize + mTileSize * 0.5f
            );
        }

        /// <summary>
        /// 网格坐标转世界坐标（返回地块中心点）
        /// </summary>
        /// <param name="gridPos">网格坐标</param>
        /// <returns>世界坐标（地块中心）</returns>
        public Vector3 GridToWorld(Vector2Int gridPos)
        {
            return GridToWorld(gridPos.x, gridPos.y);
        }

        #endregion

        #region 私有方法

        private bool ValidateInitialized()
        {
            if (!mIsInitialized)
            {
                Debug.LogError("[MapManager] Operation failed: MapManager is not initialized");
                return false;
            }
            return true;
        }

        #endregion
    }
}
