using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace FarmGame.Core.Save
{
    /// <summary>
    /// 游戏存档数据容器
    /// 包含所有需要持久化的游戏状态
    /// </summary>
    [Serializable]
    public class GameSaveData
    {
        /// <summary>
        /// 存档版本，用于未来兼容性
        /// </summary>
        public int Version { get; set; } = 1;
        
        /// <summary>
        /// 存档创建时间戳
        /// </summary>
        public DateTime SaveTime { get; set; }
        
        /// <summary>
        /// 存档描述（可选的玩家自定义名称）
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// 游戏时间（游戏内天数、时间等）
        /// </summary>
        public GameTimeData GameTime { get; set; } = new GameTimeData();
        
        /// <summary>
        /// 玩家数据
        /// </summary>
        public PlayerSaveData Player { get; set; } = new PlayerSaveData();
        
        /// <summary>
        /// 农场数据
        /// </summary>
        public FarmSaveData Farm { get; set; } = new FarmSaveData();
        
        /// <summary>
        /// NPC 状态数据
        /// </summary>
        public Dictionary<string, NPCSaveData> NPCs { get; set; } = new Dictionary<string, NPCSaveData>();
        
        /// <summary>
        /// 商店状态数据
        /// </summary>
        public ShopSaveData Shop { get; set; } = new ShopSaveData();
    }
    
    /// <summary>
    /// 游戏时间数据
    /// </summary>
    [Serializable]
    public class GameTimeData
    {
        public int Day { get; set; } = 1;
        public float TimeOfDay { get; set; } = 0f; // 0-24小时制
        
        [JsonIgnore]
        public string DisplayTime => $"第{Day}天 {TimeOfDay:00.00}时";
    }
    
    /// <summary>
    /// 玩家存档数据
    /// </summary>
    [Serializable]
    public class PlayerSaveData
    {
        /// <summary>
        /// 玩家位置
        /// </summary>
        public Vector3 Position { get; set; }
        
        /// <summary>
        /// 玩家朝向
        /// </summary>
        public Vector3 FacingDirection { get; set; }
        
        /// <summary>
        /// 移动速度
        /// </summary>
        public float MoveSpeed { get; set; }
        
        /// <summary>
        /// 是否为新玩家（首次进入游戏）
        /// </summary>
        public bool IsNewPlayer { get; set; }
        
        /// <summary>
        /// 金币数量
        /// </summary>
        public int Gold { get; set; }
        
        /// <summary>
        /// 背包物品列表
        /// </summary>
        public List<ItemSaveData> Inventory { get; set; } = new List<ItemSaveData>();
    }
    
    /// <summary>
    /// 物品存档数据
    /// </summary>
    [Serializable]
    public class ItemSaveData
    {
        public int ConfigId { get; set; }
        public int Count { get; set; }
        
        /// <summary>
        /// 实例ID（用于唯一标识，可选）
        /// </summary>
        public string InstanceId { get; set; }
        
        /// <summary>
        /// 额外数据（用于特殊物品）
        /// </summary>
        public Dictionary<string, string> ExtraData { get; set; } = new Dictionary<string, string>();
    }
    
    /// <summary>
    /// 农场存档数据
    /// </summary>
    [Serializable]
    public class FarmSaveData
    {
        /// <summary>
        /// 当前地图ID
        /// </summary>
        public string CurrentMapId { get; set; } = "Main";
        
        /// <summary>
        /// 所有土地数据
        /// </summary>
        public List<SoilSaveData> Soils { get; set; } = new List<SoilSaveData>();
    }
    
    /// <summary>
    /// 土地存档数据
    /// </summary>
    [Serializable]
    public class SoilSaveData
    {
        public int ConfigId { get; set; }
        public Vector2Int GridPos { get; set; }
        public bool IsTilled { get; set; }
        
        /// <summary>
        /// 种植的作物数据（如果存在）
        /// </summary>
        public PlantSaveData Plant { get; set; }
    }
    
    /// <summary>
    /// 作物存档数据
    /// </summary>
    [Serializable]
    public class PlantSaveData
    {
        public int ConfigId { get; set; }
        public float CurrentMaturity { get; set; }
        public int CurrentStageIndex { get; set; }
        public bool IsMature { get; set; }
    }
    
    /// <summary>
    /// NPC存档数据
    /// </summary>
    [Serializable]
    public class NPCSaveData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Vector3 Position { get; set; }
        public float InteractionDistance { get; set; }
        
        /// <summary>
        /// NPC内存/状态数据
        /// </summary>
        public Dictionary<string, string> Memory { get; set; } = new Dictionary<string, string>();
    }
    
    /// <summary>
    /// 商店存档数据
    /// </summary>
    [Serializable]
    public class ShopSaveData
    {
        /// <summary>
        /// 商店已售出物品记录
        /// </summary>
        public Dictionary<int, int> SoldItems { get; set; } = new Dictionary<int, int>();
        
        /// <summary>
        /// 商店库存变化记录
        /// </summary>
        public Dictionary<int, int> StockChanges { get; set; } = new Dictionary<int, int>();
    }
}