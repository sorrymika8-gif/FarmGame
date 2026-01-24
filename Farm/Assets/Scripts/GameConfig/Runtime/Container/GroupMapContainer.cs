// ==========================================================
// 自动生成配置系统 - GroupMap 容器实现
// ==========================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace FarmGame.GameConfig
{
    /// <summary>
    /// 单层 GroupMap 容器实现
    /// </summary>
    /// <typeparam name="TKey">主键类型</typeparam>
    /// <typeparam name="T">配置类型</typeparam>
    public class GroupMapContainer<TKey, T> : IGroupMapContainer<TKey, T>
    {
        private readonly Dictionary<TKey, List<T>> mData;
        private readonly List<T> mAllItems;

        public GroupMapContainer()
        {
            mData = new Dictionary<TKey, List<T>>();
            mAllItems = new List<T>();
        }

        public int Count => mAllItems.Count;
        public Type ConfigType => typeof(T);

        public IReadOnlyList<T> GetGroup(TKey key)
        {
            if (mData.TryGetValue(key, out var list))
            {
                return list.AsReadOnly();
            }
            return Array.Empty<T>();
        }

        public bool ContainsKey(TKey key)
        {
            return mData.ContainsKey(key);
        }

        public void Add(TKey key, T value)
        {
            if (!mData.TryGetValue(key, out var list))
            {
                list = new List<T>();
                mData[key] = list;
            }
            list.Add(value);
            mAllItems.Add(value);
        }

        public IEnumerable<TKey> GetKeys()
        {
            return mData.Keys;
        }

        public IReadOnlyList<T> GetAll()
        {
            return mAllItems.AsReadOnly();
        }

        public void Clear()
        {
            mData.Clear();
            mAllItems.Clear();
        }
    }

    /// <summary>
    /// 双层 GroupMap 容器实现
    /// </summary>
    /// <typeparam name="TKey1">第一层主键类型</typeparam>
    /// <typeparam name="TKey2">第二层主键类型</typeparam>
    /// <typeparam name="T">配置类型</typeparam>
    public class GroupMapContainer<TKey1, TKey2, T> : IGroupMapContainer<TKey1, TKey2, T>
    {
        private readonly Dictionary<TKey1, Dictionary<TKey2, List<T>>> mData;
        private readonly List<T> mAllItems;

        public GroupMapContainer()
        {
            mData = new Dictionary<TKey1, Dictionary<TKey2, List<T>>>();
            mAllItems = new List<T>();
        }

        public int Count => mAllItems.Count;
        public Type ConfigType => typeof(T);

        public IReadOnlyList<T> GetGroup(TKey1 key1, TKey2 key2)
        {
            if (mData.TryGetValue(key1, out var dict) && dict.TryGetValue(key2, out var list))
            {
                return list.AsReadOnly();
            }
            return Array.Empty<T>();
        }

        public IReadOnlyDictionary<TKey2, IReadOnlyList<T>> GetGroup(TKey1 key1)
        {
            if (mData.TryGetValue(key1, out var dict))
            {
                return dict.ToDictionary(
                    kvp => kvp.Key,
                    kvp => (IReadOnlyList<T>)kvp.Value.AsReadOnly()
                );
            }
            return new Dictionary<TKey2, IReadOnlyList<T>>();
        }

        public void Add(TKey1 key1, TKey2 key2, T value)
        {
            if (!mData.TryGetValue(key1, out var dict))
            {
                dict = new Dictionary<TKey2, List<T>>();
                mData[key1] = dict;
            }

            if (!dict.TryGetValue(key2, out var list))
            {
                list = new List<T>();
                dict[key2] = list;
            }

            list.Add(value);
            mAllItems.Add(value);
        }

        public IEnumerable<TKey1> GetKeys()
        {
            return mData.Keys;
        }

        public IReadOnlyList<T> GetAll()
        {
            return mAllItems.AsReadOnly();
        }

        public void Clear()
        {
            mData.Clear();
            mAllItems.Clear();
        }
    }

    /// <summary>
    /// 三层 GroupMap 容器实现
    /// </summary>
    /// <typeparam name="TKey1">第一层主键类型</typeparam>
    /// <typeparam name="TKey2">第二层主键类型</typeparam>
    /// <typeparam name="TKey3">第三层主键类型</typeparam>
    /// <typeparam name="T">配置类型</typeparam>
    public class GroupMapContainer<TKey1, TKey2, TKey3, T> : IGroupMapContainer<TKey1, TKey2, TKey3, T>
    {
        private readonly Dictionary<TKey1, Dictionary<TKey2, Dictionary<TKey3, List<T>>>> mData;
        private readonly List<T> mAllItems;

        public GroupMapContainer()
        {
            mData = new Dictionary<TKey1, Dictionary<TKey2, Dictionary<TKey3, List<T>>>>();
            mAllItems = new List<T>();
        }

        public int Count => mAllItems.Count;
        public Type ConfigType => typeof(T);

        public IReadOnlyList<T> GetGroup(TKey1 key1, TKey2 key2, TKey3 key3)
        {
            if (mData.TryGetValue(key1, out var dict1) &&
                dict1.TryGetValue(key2, out var dict2) &&
                dict2.TryGetValue(key3, out var list))
            {
                return list.AsReadOnly();
            }
            return Array.Empty<T>();
        }

        public IReadOnlyDictionary<TKey3, IReadOnlyList<T>> GetGroup(TKey1 key1, TKey2 key2)
        {
            if (mData.TryGetValue(key1, out var dict1) && dict1.TryGetValue(key2, out var dict2))
            {
                return dict2.ToDictionary(
                    kvp => kvp.Key,
                    kvp => (IReadOnlyList<T>)kvp.Value.AsReadOnly()
                );
            }
            return new Dictionary<TKey3, IReadOnlyList<T>>();
        }

        public void Add(TKey1 key1, TKey2 key2, TKey3 key3, T value)
        {
            if (!mData.TryGetValue(key1, out var dict1))
            {
                dict1 = new Dictionary<TKey2, Dictionary<TKey3, List<T>>>();
                mData[key1] = dict1;
            }

            if (!dict1.TryGetValue(key2, out var dict2))
            {
                dict2 = new Dictionary<TKey3, List<T>>();
                dict1[key2] = dict2;
            }

            if (!dict2.TryGetValue(key3, out var list))
            {
                list = new List<T>();
                dict2[key3] = list;
            }

            list.Add(value);
            mAllItems.Add(value);
        }

        public IReadOnlyList<T> GetAll()
        {
            return mAllItems.AsReadOnly();
        }

        public void Clear()
        {
            mData.Clear();
            mAllItems.Clear();
        }
    }
}
