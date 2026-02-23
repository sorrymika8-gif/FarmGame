using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace FarmGame.Farm.View
{
    /// <summary>
    /// 农场Tilemap视图组件
    /// 负责管理土地和作物的Tilemap渲染
    /// 挂载在农场地图预制体上
    /// </summary>
    public class FarmTilemapView : MonoBehaviour
    {
        #region 序列化字段

        [Header("Tilemap引用")]
        [Tooltip("土地层Tilemap")]
        [SerializeField] private Tilemap mSoilTilemap;

        [Tooltip("作物层Tilemap")]
        [SerializeField] private Tilemap mPlantTilemap;

        [Tooltip("高亮层Tilemap")]
        [SerializeField] private Tilemap mHighlightTilemap;

        [Header("配置")]
        [Tooltip("Tile资源配置")]
        [SerializeField] private FarmTileSet mTileSet;

        #endregion

        #region 私有字段

        private FarmMapData mMapData;
        private Dictionary<Vector2Int, SoilEntity> mSoilCache = new Dictionary<Vector2Int, SoilEntity>();
        private Dictionary<Vector2Int, CropView> mCropViewCache = new Dictionary<Vector2Int, CropView>();
        private Vector3Int? mCurrentHighlight = null;
        private bool mIsInitialized = false;

        #endregion

        #region 公共属性

        /// <summary>
        /// 土地层Tilemap
        /// </summary>
        public Tilemap SoilTilemap => mSoilTilemap;

        /// <summary>
        /// 作物层Tilemap
        /// </summary>
        public Tilemap PlantTilemap => mPlantTilemap;

        /// <summary>
        /// Grid组件
        /// </summary>
        public Grid Grid => mSoilTilemap?.layoutGrid;

        #endregion

        #region 生命周期

        private void OnDestroy()
        {
            // 取消订阅所有土地事件
            if (mMapData != null)
            {
                foreach (var soil in mMapData.GetAllSoils())
                {
                    soil.OnStateChanged -= OnSoilStateChanged;
                }
            }

            // 清理所有 CropView
            foreach (var cropView in mCropViewCache.Values)
            {
                if (cropView != null)
                {
                    Destroy(cropView.gameObject);
                }
            }
            mCropViewCache.Clear();
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 初始化视图
        /// </summary>
        /// <param name="mapData">农场地图数据</param>
        public void Initialize(FarmMapData mapData)
        {
            if (mIsInitialized)
            {
                Debug.LogWarning("[FarmTilemapView] 已经初始化过了");
                return;
            }

            if (mapData == null)
            {
                Debug.LogError("[FarmTilemapView] mapData 为空!");
                return;
            }

            if (mTileSet == null)
            {
                Debug.LogError("[FarmTilemapView] TileSet 未配置!");
                return;
            }

            mMapData = mapData;
            mSoilCache.Clear();

            // 初始化所有土地Tile
            foreach (var soil in mMapData.GetAllSoils())
            {
                mSoilCache[soil.GridPos] = soil;

                // 订阅状态变化事件
                soil.OnStateChanged += OnSoilStateChanged;

                // 设置初始Tile
                UpdateSoilTile(soil);
                UpdatePlantTile(soil);
            }

            mIsInitialized = true;
            Debug.Log($"[FarmTilemapView] 初始化完成，共 {mSoilCache.Count} 块土地");
        }

        /// <summary>
        /// 根据世界坐标获取土地实体
        /// </summary>
        /// <param name="worldPos">世界坐标</param>
        /// <returns>土地实体，不存在返回null</returns>
        public SoilEntity GetSoilAtWorldPos(Vector3 worldPos)
        {
            if (mSoilTilemap == null) return null;

            Vector3Int cellPos = mSoilTilemap.WorldToCell(worldPos);
            Vector2Int gridPos = new Vector2Int(cellPos.x, cellPos.y);

            if (mSoilCache.TryGetValue(gridPos, out var soil))
            {
                return soil;
            }

            return null;
        }

        /// <summary>
        /// 根据网格坐标获取土地实体
        /// </summary>
        /// <param name="gridPos">网格坐标</param>
        /// <returns>土地实体，不存在返回null</returns>
        public SoilEntity GetSoilAtGridPos(Vector2Int gridPos)
        {
            if (mSoilCache.TryGetValue(gridPos, out var soil))
            {
                return soil;
            }
            return null;
        }

        /// <summary>
        /// 世界坐标转网格坐标
        /// </summary>
        /// <param name="worldPos">世界坐标</param>
        /// <returns>网格坐标</returns>
        public Vector2Int WorldToGrid(Vector3 worldPos)
        {
            if (mSoilTilemap == null) return Vector2Int.zero;

            Vector3Int cellPos = mSoilTilemap.WorldToCell(worldPos);
            return new Vector2Int(cellPos.x, cellPos.y);
        }

        /// <summary>
        /// 网格坐标转世界坐标（格子中心）
        /// </summary>
        /// <param name="gridPos">网格坐标</param>
        /// <returns>世界坐标</returns>
        public Vector3 GridToWorld(Vector2Int gridPos)
        {
            if (mSoilTilemap == null) return Vector3.zero;

            Vector3Int cellPos = new Vector3Int(gridPos.x, gridPos.y, 0);
            return mSoilTilemap.GetCellCenterWorld(cellPos);
        }

        /// <summary>
        /// 设置高亮显示
        /// </summary>
        /// <param name="gridPos">要高亮的网格坐标</param>
        public void SetHighlight(Vector2Int gridPos)
        {
            if (mHighlightTilemap == null || mTileSet == null) return;

            // 清除之前的高亮
            ClearHighlight();

            // 检查该位置是否有土地
            if (!mSoilCache.ContainsKey(gridPos)) return;

            // 设置新的高亮
            Vector3Int cellPos = new Vector3Int(gridPos.x, gridPos.y, 0);
            mHighlightTilemap.SetTile(cellPos, mTileSet.highlightTile);
            mCurrentHighlight = cellPos;
        }

        /// <summary>
        /// 清除高亮显示
        /// </summary>
        public void ClearHighlight()
        {
            if (mHighlightTilemap == null) return;

            if (mCurrentHighlight.HasValue)
            {
                mHighlightTilemap.SetTile(mCurrentHighlight.Value, null);
                mCurrentHighlight = null;
            }
        }

        /// <summary>
        /// 检查指定位置是否是有效的农田
        /// </summary>
        /// <param name="worldPos">世界坐标</param>
        /// <returns>是否是农田</returns>
        public bool IsValidFarmland(Vector3 worldPos)
        {
            return GetSoilAtWorldPos(worldPos) != null;
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 土地状态变化回调
        /// </summary>
        private void OnSoilStateChanged(SoilEntity soil)
        {
            UpdateSoilTile(soil);
            UpdatePlantTile(soil);
        }

        /// <summary>
        /// 更新土地Tile
        /// </summary>
        private void UpdateSoilTile(SoilEntity soil)
        {
            if (mSoilTilemap == null || mTileSet == null) return;

            Vector3Int cellPos = new Vector3Int(soil.GridPos.x, soil.GridPos.y, 0);
            // 土地统一显示为基础土地样式（不区分耕地状态）
            TileBase tile = mTileSet.tilledTile;
            mSoilTilemap.SetTile(cellPos, tile);
        }

        /// <summary>
        /// 更新作物显示（使用 CropView 组件）
        /// </summary>
        private void UpdatePlantTile(SoilEntity soil)
        {
            Vector2Int gridPos = soil.GridPos;
            
            if (soil.HasPlant)
            {
                // 创建或更新 CropView
                if (!mCropViewCache.TryGetValue(gridPos, out var cropView))
                {
                    Vector3 worldPos = GridToWorld(gridPos);
                    cropView = CropView.Create(soil, worldPos, transform);
                    if (cropView != null)
                    {
                        mCropViewCache[gridPos] = cropView;
                    }
                }
                else
                {
                    // 更新现有的 CropView
                    cropView.Bind(soil);
                }
            }
            else
            {
                // 清除 CropView
                if (mCropViewCache.TryGetValue(gridPos, out var cropView))
                {
                    if (cropView != null)
                    {
                        Destroy(cropView.gameObject);
                    }
                    mCropViewCache.Remove(gridPos);
                }
            }
            
            // 同时清除Tilemap上的Tile（如果有）
            if (mPlantTilemap != null)
            {
                Vector3Int cellPos = new Vector3Int(gridPos.x, gridPos.y, 0);
                mPlantTilemap.SetTile(cellPos, null);
            }
        }

        /// <summary>
        /// 获取指定位置的作物视图
        /// </summary>
        /// <param name="gridPos">网格坐标</param>
        /// <returns>CropView 实例，不存在返回 null</returns>
        public CropView GetCropViewAtGridPos(Vector2Int gridPos)
        {
            if (mCropViewCache.TryGetValue(gridPos, out var cropView))
            {
                return cropView;
            }
            return null;
        }

        /// <summary>
        /// 获取指定世界坐标的作物视图
        /// </summary>
        /// <param name="worldPos">世界坐标</param>
        /// <returns>CropView 实例，不存在返回 null</returns>
        public CropView GetCropViewAtWorldPos(Vector3 worldPos)
        {
            Vector2Int gridPos = WorldToGrid(worldPos);
            return GetCropViewAtGridPos(gridPos);
        }

        #endregion
    }
}
