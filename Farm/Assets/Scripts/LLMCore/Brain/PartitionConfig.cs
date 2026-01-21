using System;

namespace GameLLM.Brain
{
    /// <summary>
    /// 分区配置
    /// 定义分区的名称、含义和容量
    /// </summary>
    [Serializable]
    public class PartitionConfig
    {
        public string Name;
        public string Description;
        public int? Capacity; // null 表示无限容量

        public PartitionConfig(string name, string description, int? capacity = null)
        {
            Name = name;
            Description = description;
            Capacity = capacity;
        }
    }
}
