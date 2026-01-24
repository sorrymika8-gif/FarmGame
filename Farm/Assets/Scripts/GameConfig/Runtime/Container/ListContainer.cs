// ==========================================================
// 自动生成配置系统 - List 容器实现
// ==========================================================

using System;
using System.Collections.Generic;

namespace FarmGame.GameConfig
{
    /// <summary>
    /// List 容器实现
    /// </summary>
    /// <typeparam name="T">配置类型</typeparam>
    public class ListContainer<T> : IListContainer<T>
    {
        private readonly List<T> mItems;

        public ListContainer()
        {
            mItems = new List<T>();
        }

        public ListContainer(int capacity)
        {
            mItems = new List<T>(capacity);
        }

        /// <summary>
        /// 配置数量
        /// </summary>
        public int Count => mItems.Count;

        /// <summary>
        /// 配置类型
        /// </summary>
        public Type ConfigType => typeof(T);

        /// <summary>
        /// 添加配置项
        /// </summary>
        public void Add(T item)
        {
            mItems.Add(item);
        }

        /// <summary>
        /// 通过索引获取配置项
        /// </summary>
        public T GetByIndex(int index)
        {
            if (index < 0 || index >= mItems.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index),
                    $"索引超出范围: {index}，有效范围: [0, {mItems.Count - 1}]");
            }
            return mItems[index];
        }

        /// <summary>
        /// 获取所有配置项
        /// </summary>
        public IReadOnlyList<T> GetAll()
        {
            return mItems.AsReadOnly();
        }

        /// <summary>
        /// 清空容器
        /// </summary>
        public void Clear()
        {
            mItems.Clear();
        }
    }
}
