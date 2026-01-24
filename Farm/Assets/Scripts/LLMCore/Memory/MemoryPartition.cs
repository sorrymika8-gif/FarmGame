using System;
using System.Collections.Generic;

namespace FarmGame.LLMCore.Memory
{
    /// <summary>
    /// 记忆分区
    /// 一个存放记忆的区域，记忆按产生顺序排列
    /// </summary>
    [Serializable]
    public class MemoryPartition
    {
        private readonly List<Memory> mMemories = new();

        /// <summary>分区名称</summary>
        public string Name { get; }

        /// <summary>记忆数量</summary>
        public int Count => mMemories.Count;

        /// <summary>分区是否为空</summary>
        public bool IsEmpty => mMemories.Count == 0;

        public MemoryPartition(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name), "分区名称不能为空");
            }
            Name = name;
        }

        /// <summary>追加一条记忆到分区末尾</summary>
        public void Append(Memory memory)
        {
            if (memory == null)
            {
                throw new ArgumentNullException(nameof(memory));
            }
            mMemories.Add(memory);
        }

        /// <summary>追加一条记忆（便捷方法）</summary>
        public Memory Append(string content)
        {
            var memory = new Memory(content);
            mMemories.Add(memory);
            return memory;
        }

        /// <summary>添加一条记忆（Append的别名）</summary>
        public void Add(Memory memory) => Append(memory);

        /// <summary>获取所有记忆（按产生顺序）</summary>
        public IReadOnlyList<Memory> GetAll() => mMemories.AsReadOnly();

        /// <summary>移除指定的记忆</summary>
        public bool Remove(Memory memory) => mMemories.Remove(memory);

        /// <summary>移除指定位置的记忆</summary>
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= mMemories.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            mMemories.RemoveAt(index);
        }

        /// <summary>清空分区内所有记忆</summary>
        public void Clear() => mMemories.Clear();

        /// <summary>获取指定位置的记忆</summary>
        public Memory GetAt(int index)
        {
            if (index < 0 || index >= mMemories.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return mMemories[index];
        }

        /// <summary>更新指定位置的记忆内容</summary>
        public void UpdateAt(int index, string newContent)
        {
            if (index < 0 || index >= mMemories.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            mMemories[index] = new Memory(newContent);
        }

        /// <summary>索引器访问</summary>
        public Memory this[int index] => GetAt(index);
    }
}
