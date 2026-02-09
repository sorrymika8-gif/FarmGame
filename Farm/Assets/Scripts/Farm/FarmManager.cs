using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FarmGame.GameConfig;
using FarmGame.GameConfig.Generated;
using FarmGame.Item;
using QFramework;
using UnityEngine;

namespace FarmGame.Farm
{
    public class FarmManager : MonoSingleton<FarmManager>
    {
        // Managing multiple farm maps (e.g., "Main", "NPC_Home")
        private Dictionary<string, FarmMapData> mMaps = new Dictionary<string, FarmMapData>();
        
        public FarmMapData CurrentMap { get; private set; }

        public bool IsInitialized { get; private set; }

        public void Initialize()
        {
            // Default Map initialization
            var defaultMap = new FarmMapData("Main");
            
            try 
            {
                var soilMap = ConfigManager.Instance.GetMap<int, SoilConfig>();
                if (soilMap != null)
                {
                    foreach (var soil in soilMap.GetAll())
                    {
                        var pos = new Vector2Int(Mathf.RoundToInt(soil.pos_x), Mathf.RoundToInt(soil.pos_y));
                        var soilEntity = new SoilEntity(soil.id, pos.x, pos.y);
                        defaultMap.AddSoil(soilEntity);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[FarmManager] Failed to load soil config: {e.Message}");
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
            if (!soil.IsTilled) 
            {
                Debug.LogWarning($"[FarmLogic] Cannot plant at {soil.GridPos}: Land is not tilled.");
                return false;
            }
            if (soil.HasPlant) 
            {
                Debug.LogWarning($"[FarmLogic] Cannot plant at {soil.GridPos}: Already has plant.");
                return false;
            }
            
            // Check Item Config (Data Lookup)
            var itemConfig = ConfigManager.Instance.GetConfig<ItemConfig>(itemId);
            if (itemConfig == null || itemConfig.type != 1) // 1 = Seed
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
            Debug.Log($"[FarmLogic] Planted {itemConfig.name} at {soil.GridPos}.");
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

        // --- Logic Loop ---

        private async UniTaskVoid StartGrowthLoop()
        {
            while (this != null) 
            {
                await UniTask.Delay(1000); 
                
                // Update all maps
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
    }
}
