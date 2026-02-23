using FarmGame.GameConfig;
using FarmGame.GameConfig.Generated;

namespace FarmGame.Item
{
    /// <summary>
    /// 物品配置信息的统一封装
    /// 由于道具类型多样（种子、产物等），配置分布在不同的表中，
    /// 此结构体统一提供基础物品信息的访问
    /// </summary>
    public struct ItemConfigInfo
    {
        /// <summary>物品ID</summary>
        public int ClassId { get; set; }

        /// <summary>物品名称</summary>
        public string Name { get; set; }

        /// <summary>物品类型</summary>
        public int ItemType { get; set; }

        /// <summary>图标路径</summary>
        public string Icon { get; set; }

        /// <summary>描述</summary>
        public string Description { get; set; }

        /// <summary>是否为有效配置</summary>
        public bool IsValid => ClassId > 0;

        /// <summary>
        /// 从 ItemConfig 创建
        /// </summary>
        public static ItemConfigInfo FromItemConfig(ItemConfig config)
        {
            if (config == null) return default;
            return new ItemConfigInfo
            {
                ClassId = config.class_id,
                Name = config.name,
                ItemType = config.item_type,
                Icon = config.icon,
                Description = config.description
            };
        }

        /// <summary>
        /// 从 SeedConfig 创建
        /// </summary>
        public static ItemConfigInfo FromSeedConfig(SeedConfig config)
        {
            if (config == null) return default;
            return new ItemConfigInfo
            {
                ClassId = config.class_id,
                Name = config.name,
                ItemType = config.item_type,
                Icon = config.icon,
                Description = $"{config.name}种子" // 种子表没有 description 字段
            };
        }
    }

    /// <summary>
    /// 物品配置查询帮助类
    /// 从多个配置表中查找物品配置
    /// </summary>
    public static class ItemConfigHelper
    {
        /// <summary>
        /// 根据物品ID获取统一的配置信息
        /// 优先从 ItemConfig 查询，找不到则从 SeedConfig 查询
        /// </summary>
        /// <param name="configId">物品配置ID</param>
        /// <returns>统一的配置信息</returns>
        public static ItemConfigInfo GetConfigInfo(int configId)
        {
            var configManager = ConfigManager.Instance;

            // 1. 先尝试从 ItemConfig 获取
            var itemMap = configManager.GetMap<int, ItemConfig>();
            if (itemMap != null && itemMap.TryGet(configId, out var itemConfig))
            {
                return ItemConfigInfo.FromItemConfig(itemConfig);
            }

            // 2. 再尝试从 SeedConfig 获取
            var seedMap = configManager.GetMap<int, SeedConfig>();
            if (seedMap != null && seedMap.TryGet(configId, out var seedConfig))
            {
                return ItemConfigInfo.FromSeedConfig(seedConfig);
            }

            // 3. 未找到配置
            return default;
        }

        /// <summary>
        /// 尝试获取配置信息
        /// </summary>
        /// <param name="configId">物品配置ID</param>
        /// <param name="info">输出的配置信息</param>
        /// <returns>是否找到配置</returns>
        public static bool TryGetConfigInfo(int configId, out ItemConfigInfo info)
        {
            info = GetConfigInfo(configId);
            return info.IsValid;
        }
    }
}
