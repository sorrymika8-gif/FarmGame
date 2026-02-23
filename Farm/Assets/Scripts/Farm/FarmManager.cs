using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FarmGame.GameConfig;
using FarmGame.GameConfig.Generated;
using FarmGame.Item;
using QFramework;
using UnityEngine;

namespace FarmGame.Farm
{
    /// <summary>
    /// 农场管理器
    /// 负责农场业务逻辑：耕地、种植、收获、生长循环
    /// </summary>
    public class FarmManager : MonoSingleton<FarmManager>
    {
        #region 常量

        /// <summary>
        /// 生长周期配置键名
        /// </summary>
        private const string GROWTH_TICK_INTERVAL_KEY = "growth_tick_interval";

        /// <summary>
        /// 默认生长周期间隔（毫秒）
        /// </summary>
        private const int DEFAULT_GROWTH_TICK_INTERVAL = 1000;

        /// <summary>
        /// 默认农田宽度（格）
        /// </summary>
        private const int DEFAULT_FARM_WIDTH = 5;

        /// <summary>
        /// 默认农田高度（格）
        /// </summary>
        private const int DEFAULT_FARM_HEIGHT = 4;

        /// <summary>
        /// 默认农田左下角原点（网格）
        /// </summary>
        private static readonly Vector2Int DEFAULT_FARM_ORIGIN = Vector2Int.zero;

        #endregion

        #region 私有字段

        /// <summary>
        /// 管理多个农场地图
        /// </summary>
        private Dictionary<string, FarmMapData> mMaps = new Dictionary<string, FarmMapData>();

        /// <summary>
        /// 生长周期间隔（毫秒）
        /// </summary>
        private int mGrowthTickInterval = DEFAULT_GROWTH_TICK_INTERVAL;

        #endregion

        #region 公共属性

        /// <summary>
        /// 当前地图
        /// </summary>
        public FarmMapData CurrentMap { get; private set; }

        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized { get; private set; }

        #endregion

        #region 公共方法

        /// <summary>
        /// 初始化农场管理器
        /// </summary>
        public void Initialize()
        {
            // 读取生长周期配置
            mGrowthTickInterval = GameSettingsHelper.GetInt(GROWTH_TICK_INTERVAL_KEY, DEFAULT_GROWTH_TICK_INTERVAL);
            Debug.Log($"[FarmManager] 生长周期间隔: {mGrowthTickInterval}ms");

            // 默认地图初始化
            var defaultMap = new FarmMapData("Main");

            int soilId = 1;
            for (int y = 0; y < DEFAULT_FARM_HEIGHT; y++)
            {
                for (int x = 0; x < DEFAULT_FARM_WIDTH; x++)
                {
                    int gridX = DEFAULT_FARM_ORIGIN.x + x;
                    int gridY = DEFAULT_FARM_ORIGIN.y + y;
                    var soilEntity = new SoilEntity(soilId, gridX, gridY);
                    // 默认已耕地，可直接种植
                    soilEntity.IsTilled = true;
                    defaultMap.AddSoil(soilEntity);
                    soilId++;
                }
            }

            RegisterMap(defaultMap);
            CurrentMap = defaultMap;

            IsInitialized = true;
            StartGrowthLoop().Forget();
            Debug.Log($"[FarmManager] Initialized with default map ({defaultMap.Count} soils).");
        }

        public void RegisterMap(FarmMapData map)
        {
            if (mMaps.ContainsKey(map.MapId)) return;
            mMaps[map.MapId] = map;
        }

        public SoilEntity GetSoil(Vector2Int pos)
        {
            // Simplified global access to current map
            return CurrentMap?.GetSoil(pos);
        }

        // --- Core PURE Business Logic APIs (Operating on Entities) ---

        public bool Till(SoilEntity soil)
        {
            if (soil == null) return false;
            if (soil.IsTilled) return false;

            soil.IsTilled = true;
            Debug.Log($"[FarmLogic] Soil at {soil.GridPos} tilled.");
            return true;
        }

        public bool Plant(SoilEntity soil, int itemId, InventoryComponent inventory)
        {
            if (soil == null) return false;
            
            // Logic Checks on Entity State
            if (soil.HasPlant) 
            {
                Debug.LogWarning($"[FarmLogic] Cannot plant at {soil.GridPos}: Already has plant.");
                return false;
            }
            
            // Check Item Config (Data Lookup)
            var configInfo = ItemConfigHelper.GetConfigInfo(itemId);
            if (!configInfo.IsValid || configInfo.ItemType != (int)ItemType.Seed)
            {
                Debug.LogWarning($"[FarmLogic] Item {itemId} is not a seed.");
                return false;
            }

            // Inventory Operation
            if (!inventory.RemoveItem(itemId, 1))
            {
                return false;
            }

            // State Mutation
            soil.Plant = new PlantEntity(itemId);
            Debug.Log($"[FarmLogic] Planted {configInfo.Name} at {soil.GridPos}.");
            return true;
        }

        public bool Harvest(SoilEntity soil, InventoryComponent inventory)
        {
            if (soil == null || !soil.HasPlant) return false;

            var plant = soil.Plant;
            if (!plant.IsMature)
            {
                Debug.LogWarning($"[FarmLogic] Plant at {soil.GridPos} is not mature yet.");
                return false;
            }

            // Logic: Calculate Yield
            var plantConfig = plant.PlantData;
            if (plantConfig != null && plantConfig.bonus_item != null)
            {
                for (int i = 0; i < plantConfig.bonus_item.Length; i++)
                {
                    int itemId = plantConfig.bonus_item[i];
                    int count = 1; 
                    if (plantConfig.bonus_amount != null && i < plantConfig.bonus_amount.Length)
                        count = plantConfig.bonus_amount[i];
                    
                    inventory.AddItem(itemId, count);
                }
            }

            // Logic: State Mutation
            soil.Plant = null;
            Debug.Log($"[FarmLogic] Harvested at {soil.GridPos}.");
            return true;
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 生长循环
        /// </summary>
        private async UniTaskVoid StartGrowthLoop()
        {
            while (this != null)
            {
                await UniTask.Delay(mGrowthTickInterval);

                // 更新所有地图
                foreach (var map in mMaps.Values)
                {
                    foreach (var soil in map.GetAllSoils())
                    {
                        if (soil.HasPlant)
                        {
                            var plant = soil.Plant;
                            if (!plant.IsMature)
                            {
                                var config = plant.PlantData;
                                if (config != null)
                                {
                                    plant.Grow(config.maturity_speed);
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}
