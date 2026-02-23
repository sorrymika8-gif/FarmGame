using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace FarmGame.Farm
{
    /// <summary>
    /// 作物Tile阶段配置
    /// </summary>
    [Serializable]
    public class PlantTileStages
    {
        /// <summary>
        /// 作物配置ID（对应 SeedConfig.class_id）
        /// </summary>
        public int plantConfigId;

        /// <summary>
        /// 作物名称（仅用于编辑器显示）
        /// </summary>
        public string plantName;

        /// <summary>
        /// 各生长阶段的Tile
        /// 索引对应 PlantEntity.CurrentStageIndex
        /// </summary>
        public TileBase[] stageTiles;
    }

    /// <summary>
    /// 农场Tile资源配置
    /// ScriptableObject，用于在Unity编辑器中配置各种Tile资源
    /// </summary>
    [CreateAssetMenu(fileName = "FarmTileSet", menuName = "FarmGame/Farm Tile Set")]
    public class FarmTileSet : ScriptableObject
    {
        #region 土地 Tiles

        [Header("土地 Tiles")]
        [Tooltip("未耕地Tile")]
        public TileBase untilledTile;

        [Tooltip("已耕地Tile")]
        public TileBase tilledTile;

        [Tooltip("选中高亮Tile")]
        public TileBase highlightTile;

        #endregion

        #region 作物 Tiles

        [Header("作物 Tiles")]
        [Tooltip("各作物的生长阶段Tile配置")]
        public List<PlantTileStages> plantTileConfigs = new List<PlantTileStages>();

        // 运行时缓存：plantConfigId -> PlantTileStages
        private Dictionary<int, PlantTileStages> mPlantTileCache;

        #endregion

        #region 公共方法

        /// <summary>
        /// 获取土地Tile
        /// </summary>
        /// <param name="isTilled">是否已耕地</param>
        /// <returns>对应的Tile</returns>
        public TileBase GetSoilTile(bool isTilled)
        {
            return isTilled ? tilledTile : untilledTile;
        }

        /// <summary>
        /// 获取作物指定阶段的Tile
        /// </summary>
        /// <param name="plantConfigId">作物配置ID</param>
        /// <param name="stageIndex">生长阶段索引</param>
        /// <returns>对应的Tile，未找到返回null</returns>
        public TileBase GetPlantTile(int plantConfigId, int stageIndex)
        {
            EnsureCache();

            if (mPlantTileCache.TryGetValue(plantConfigId, out var stages))
            {
                if (stages.stageTiles != null && stageIndex < stages.stageTiles.Length)
                {
                    return stages.stageTiles[stageIndex];
                }
            }

            Debug.LogWarning($"[FarmTileSet] 未找到作物Tile: plantId={plantConfigId}, stage={stageIndex}");
            return null;
        }

        /// <summary>
        /// 检查是否有指定作物的Tile配置
        /// </summary>
        /// <param name="plantConfigId">作物配置ID</param>
        /// <returns>是否存在配置</returns>
        public bool HasPlantTileConfig(int plantConfigId)
        {
            EnsureCache();
            return mPlantTileCache.ContainsKey(plantConfigId);
        }

        #endregion

        #region 私有方法

        private void EnsureCache()
        {
            if (mPlantTileCache != null) return;

            mPlantTileCache = new Dictionary<int, PlantTileStages>();
            foreach (var config in plantTileConfigs)
            {
                if (!mPlantTileCache.ContainsKey(config.plantConfigId))
                {
                    mPlantTileCache[config.plantConfigId] = config;
                }
                else
                {
                    Debug.LogWarning($"[FarmTileSet] 重复的作物Tile配置: {config.plantConfigId}");
                }
            }
        }

        private void OnValidate()
        {
            // 编辑器中修改时清除缓存
            mPlantTileCache = null;
        }

        #endregion
    }
}
