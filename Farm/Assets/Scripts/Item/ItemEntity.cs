using FarmGame.GameConfig;
using FarmGame.GameConfig.Generated;

namespace FarmGame.Item
{
    /// <summary>
    /// Runtime data for an item.
    /// Can be stored in inventory or dropped in the world.
    /// </summary>
    public class ItemEntity
    {
        public int ConfigId { get; protected set; }
        public int Count { get; set; }
        public string InstanceId { get; private set; }

        /// <summary>
        /// 获取 ItemConfig 配置（仅当物品在 item.xlsx 中有配置时有效）
        /// </summary>
        public ItemConfig Config => ConfigManager.Instance.GetConfig<ItemConfig>(ConfigId);

        /// <summary>
        /// 获取统一的物品配置信息（支持从多个配置表中查询：item.xlsx、seed.xlsx 等）
        /// </summary>
        public ItemConfigInfo ConfigInfo => ItemConfigHelper.GetConfigInfo(ConfigId);

        public ItemEntity(int configId, int count)
        {
            ConfigId = configId;
            Count = count;
            InstanceId = System.Guid.NewGuid().ToString();
        }
        
        // For serialization or empty creation
        public ItemEntity() { }
    }
}
