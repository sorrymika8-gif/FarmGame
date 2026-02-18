using System;

namespace FarmGame.GameConfig
{
    /// <summary>
    /// 物品类型枚举
    /// </summary>
    public enum ItemType
    {
        None = 0,
        Seed = 1,       // 种子
        Product = 2,    // 农产品
        Tool = 3        // 工具
    }

    [Serializable]
    public class ItemConfig
    {
        public int id;
        public string name;
        public string description;
        public int max_stack;
        /// <summary>
        /// 物品类型: 1=Seed, 2=Product, 3=Tool
        /// </summary>
        public int type; 
        /// <summary>
        /// 功能参数: 如果type==Seed, 关联 PlantConfig.class_id
        /// </summary>
        public int function_args;
        /// <summary>
        /// 物品图标路径 (Resources目录下的相对路径)
        /// </summary>
        public string iconPath;

        /// <summary>
        /// 获取物品类型枚举
        /// </summary>
        public ItemType ItemType => (ItemType)type;
    }
}
