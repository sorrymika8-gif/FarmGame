// ============================================================
// 文件: LLMCore/Memory/MemoryStore.cs
// 描述: 记忆存储，一个个体的完整记忆
// ============================================================

using System;
using System.Collections.Generic;

namespace GameLLM.Memory
{
    /// <summary>
    /// 记忆存储
    /// 一个个体的完整记忆，包含若干分区
    /// 记忆天然属于持有它的个体，不需要标记归属者
    /// </summary>
    [Serializable]
    public class MemoryStore
    {
        private readonly Dictionary<string, MemoryPartition> _partitions = new();

        /// <summary>
        /// 分区数量
        /// </summary>
        public int PartitionCount => _partitions.Count;

        /// <summary>
        /// 创建一个分区
        /// 如果同名分区已存在，返回已有的分区
        /// </summary>
        /// <param name="name">分区名称</param>
        /// <returns>记忆分区</returns>
        public MemoryPartition CreatePartition(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name), "分区名称不能为空");
            }
            
            if (_partitions.TryGetValue(name, out var existingPartition))
            {
                return existingPartition;
            }

            var partition = new MemoryPartition(name);
            _partitions[name] = partition;
            return partition;
        }

        /// <summary>
        /// 获取一个分区
        /// </summary>
        /// <param name="name">分区名称</param>
        /// <returns>记忆分区，如果不存在返回null</returns>
        public MemoryPartition GetPartition(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }
            
            return _partitions.TryGetValue(name, out var partition) ? partition : null;
        }

        /// <summary>
        /// 获取或创建一个分区
        /// </summary>
        /// <param name="name">分区名称</param>
        /// <returns>记忆分区</returns>
        public MemoryPartition GetOrCreatePartition(string name)
        {
            return GetPartition(name) ?? CreatePartition(name);
        }

        /// <summary>
        /// 移除一个分区
        /// </summary>
        /// <param name="name">分区名称</param>
        /// <returns>是否移除成功</returns>
        public bool RemovePartition(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }
            
            return _partitions.Remove(name);
        }

        /// <summary>
        /// 获取所有分区名称
        /// </summary>
        /// <returns>分区名称集合</returns>
        public IEnumerable<string> GetPartitionNames()
        {
            return _partitions.Keys;
        }

        /// <summary>
        /// 获取所有分区
        /// </summary>
        /// <returns>分区集合</returns>
        public IEnumerable<MemoryPartition> GetAllPartitions()
        {
            return _partitions.Values;
        }

        /// <summary>
        /// 分区是否存在
        /// </summary>
        /// <param name="name">分区名称</param>
        /// <returns>是否存在</returns>
        public bool HasPartition(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }
            
            return _partitions.ContainsKey(name);
        }

        /// <summary>
        /// 清空所有分区（移除所有分区）
        /// </summary>
        public void ClearAllPartitions()
        {
            _partitions.Clear();
        }

        /// <summary>
        /// 清空所有分区内的记忆（保留分区结构）
        /// </summary>
        public void ClearAllMemories()
        {
            foreach (var partition in _partitions.Values)
            {
                partition.Clear();
            }
        }

        /// <summary>
        /// 获取所有分区的记忆总数
        /// </summary>
        public int TotalMemoryCount
        {
            get
            {
                int count = 0;
                foreach (var partition in _partitions.Values)
                {
                    count += partition.Count;
                }
                return count;
            }
        }
    }
}
