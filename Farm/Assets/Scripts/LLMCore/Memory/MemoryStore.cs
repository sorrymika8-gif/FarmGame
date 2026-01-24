using System;
using System.Collections.Generic;

namespace FarmGame.LLMCore.Memory
{
    /// <summary>
    /// 记忆存储
    /// 一个个体的完整记忆，包含若干分区
    /// </summary>
    [Serializable]
    public class MemoryStore
    {
        private readonly Dictionary<string, MemoryPartition> mPartitions = new();

        /// <summary>分区数量</summary>
        public int PartitionCount => mPartitions.Count;

        /// <summary>所有分区的记忆总数</summary>
        public int TotalMemoryCount
        {
            get
            {
                int count = 0;
                foreach (var partition in mPartitions.Values)
                {
                    count += partition.Count;
                }
                return count;
            }
        }

        /// <summary>创建一个分区（如已存在则返回已有的）</summary>
        public MemoryPartition CreatePartition(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (mPartitions.TryGetValue(name, out var existing))
            {
                return existing;
            }

            var partition = new MemoryPartition(name);
            mPartitions[name] = partition;
            return partition;
        }

        /// <summary>获取一个分区</summary>
        public MemoryPartition GetPartition(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            return mPartitions.TryGetValue(name, out var partition) ? partition : null;
        }

        /// <summary>获取或创建一个分区</summary>
        public MemoryPartition GetOrCreatePartition(string name)
        {
            return GetPartition(name) ?? CreatePartition(name);
        }

        /// <summary>移除一个分区</summary>
        public bool RemovePartition(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            return mPartitions.Remove(name);
        }

        /// <summary>获取所有分区名称</summary>
        public IEnumerable<string> GetPartitionNames() => mPartitions.Keys;

        /// <summary>获取所有分区</summary>
        public IEnumerable<MemoryPartition> GetAllPartitions() => mPartitions.Values;

        /// <summary>分区是否存在</summary>
        public bool HasPartition(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            return mPartitions.ContainsKey(name);
        }

        /// <summary>清空所有分区（移除所有分区）</summary>
        public void ClearAllPartitions() => mPartitions.Clear();

        /// <summary>清空所有分区内的记忆（保留分区结构）</summary>
        public void ClearAllMemories()
        {
            foreach (var partition in mPartitions.Values)
            {
                partition.Clear();
            }
        }

        /// <summary>获取所有分区的所有记忆</summary>
        public List<Memory> GetAllMemories()
        {
            var allMemories = new List<Memory>();
            foreach (var partition in mPartitions.Values)
            {
                allMemories.AddRange(partition.GetAll());
            }
            return allMemories;
        }
    }
}
