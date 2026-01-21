// ============================================================
// 文件: LLMCore/Memory/MemoryPartition.cs
// 描述: 记忆分区，存放一组按顺序排列的记忆
// ============================================================

using System;
using System.Collections.Generic;

namespace GameLLM.Memory
{
    /// <summary>
    /// 记忆分区
    /// 一个存放记忆的区域，记忆按产生顺序排列
    /// 分区的名称和用途由使用者（大脑/演化系统）自行定义
    /// </summary>
    [Serializable]
    public class MemoryPartition
    {
        private readonly List<Memory> _memories = new();

        /// <summary>
        /// 分区名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 记忆数量
        /// </summary>
        public int Count => _memories.Count;

        /// <summary>
        /// 创建一个记忆分区
        /// </summary>
        /// <param name="name">分区名称</param>
        /// <exception cref="ArgumentNullException">名称不能为空</exception>
        public MemoryPartition(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name), "分区名称不能为空");
            }
            
            Name = name;
        }

        /// <summary>
        /// 追加一条记忆到分区末尾
        /// </summary>
        /// <param name="memory">要追加的记忆</param>
        /// <exception cref="ArgumentNullException">记忆不能为空</exception>
        public void Append(Memory memory)
        {
            if (memory == null)
            {
                throw new ArgumentNullException(nameof(memory), "记忆不能为空");
            }
            
            _memories.Add(memory);
        }

        /// <summary>
        /// 追加一条记忆（便捷方法，直接传入文本内容）
        /// </summary>
        /// <param name="content">记忆内容</param>
        /// <returns>创建的记忆对象</returns>
        public Memory Append(string content)
        {
            var memory = new Memory(content);
            _memories.Add(memory);
            return memory;
        }

        /// <summary>
        /// 获取所有记忆（按产生顺序）
        /// </summary>
        /// <returns>只读的记忆列表</returns>
        public IReadOnlyList<Memory> GetAll()
        {
            return _memories.AsReadOnly();
        }

        /// <summary>
        /// 移除指定的记忆
        /// </summary>
        /// <param name="memory">要移除的记忆</param>
        /// <returns>是否移除成功</returns>
        public bool Remove(Memory memory)
        {
            return _memories.Remove(memory);
        }

        /// <summary>
        /// 移除指定位置的记忆
        /// </summary>
        /// <param name="index">位置索引</param>
        /// <exception cref="ArgumentOutOfRangeException">索引超出范围</exception>
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _memories.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "索引超出范围");
            }
            
            _memories.RemoveAt(index);
        }

        /// <summary>
        /// 清空分区内所有记忆
        /// </summary>
        public void Clear()
        {
            _memories.Clear();
        }

        /// <summary>
        /// 更新指定位置的记忆
        /// </summary>
        /// <param name="index">位置索引</param>
        /// <param name="newContent">新内容</param>
        public void UpdateAt(int index, string newContent)
        {
            if (index < 0 || index >= _memories.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "索引超出范围");
            }
            if (string.IsNullOrEmpty(newContent))
            {
                throw new ArgumentNullException(nameof(newContent), "新内容不能为空");
            }
            _memories[index] = new Memory(newContent);
        }

        /// <summary>
        /// 获取指定位置的记忆
        /// </summary>
        /// <param name="index">位置索引</param>
        /// <returns>记忆对象</returns>
        /// <exception cref="ArgumentOutOfRangeException">索引超出范围</exception>
        public Memory GetAt(int index)
        {
            if (index < 0 || index >= _memories.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "索引超出范围");
            }
            
            return _memories[index];
        }

        /// <summary>
        /// 索引器访问
        /// </summary>
        public Memory this[int index] => GetAt(index);

        /// <summary>
        /// 分区是否为空
        /// </summary>
        public bool IsEmpty => _memories.Count == 0;
    }
}
