// ==========================================================
// 自动生成配置系统 - Map 容器实现
// ==========================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace FarmGame.GameConfig
{
    /// <summary>
    /// 单层 Map 容器实现
    /// </summary>
    /// <typeparam name="TKey">主键类型</typeparam>
    /// <typeparam name="T">配置类型</typeparam>
    public class MapContainer<TKey, T> : IMapContainer<TKey, T>
    {
        private readonly Dictionary<TKey, T> mData;
        private readonly List<T> mAllItems;

        public MapContainer()
        {
            mData = new Dictionary<TKey, T>();
            mAllItems = new List<T>();
        }

        public int Count => mAllItems.Count;
        public Type ConfigType => typeof(T);

        public T Get(TKey key)
        {
            if (mData.TryGetValue(key, out var value))
            {
                return value;
            }
            throw new KeyNotFoundException($"配置不存在: key={key}");
        }

        public bool TryGet(TKey key, out T value)
        {
            return mData.TryGetValue(key, out value);
        }

        public bool ContainsKey(TKey key)
        {
            return mData.ContainsKey(key);
        }

        public void Add(TKey key, T value)
        {
            if (mData.ContainsKey(key))
            {
                throw new ArgumentException($"重复的主键: {key}");
            }
            mData[key] = value;
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
    /// 双层 Map 容器实现
    /// </summary>
    /// <typeparam name="TKey1">第一层主键类型</typeparam>
    /// <typeparam name="TKey2">第二层主键类型</typeparam>
    /// <typeparam name="T">配置类型</typeparam>
    public class MapContainer<TKey1, TKey2, T> : IMapContainer<TKey1, TKey2, T>
    {
        private readonly Dictionary<TKey1, Dictionary<TKey2, T>> mData;
        private readonly List<T> mAllItems;

        public MapContainer()
        {
            mData = new Dictionary<TKey1, Dictionary<TKey2, T>>();
            mAllItems = new List<T>();
        }

        public int Count => mAllItems.Count;
        public Type ConfigType => typeof(T);

        public T Get(TKey1 key1, TKey2 key2)
        {
            if (mData.TryGetValue(key1, out var dict) && dict.TryGetValue(key2, out var value))
            {
                return value;
            }
            throw new KeyNotFoundException($"配置不存在: key1={key1}, key2={key2}");
        }

        public IReadOnlyDictionary<TKey2, T> Get(TKey1 key1)
        {
            if (mData.TryGetValue(key1, out var dict))
            {
                return dict;
            }
            return new Dictionary<TKey2, T>();
        }

        public bool TryGet(TKey1 key1, TKey2 key2, out T value)
        {
            value = default;
            if (mData.TryGetValue(key1, out var dict))
            {
                return dict.TryGetValue(key2, out value);
            }
            return false;
        }

        public bool ContainsKey(TKey1 key1, TKey2 key2)
        {
            return mData.TryGetValue(key1, out var dict) && dict.ContainsKey(key2);
        }

        public void Add(TKey1 key1, TKey2 key2, T value)
        {
            if (!mData.TryGetValue(key1, out var dict))
            {
                dict = new Dictionary<TKey2, T>();
                mData[key1] = dict;
            }

            if (dict.ContainsKey(key2))
            {
                throw new ArgumentException($"重复的主键: key1={key1}, key2={key2}");
            }

            dict[key2] = value;
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
    /// 三层 Map 容器实现
    /// </summary>
    /// <typeparam name="TKey1">第一层主键类型</typeparam>
    /// <typeparam name="TKey2">第二层主键类型</typeparam>
    /// <typeparam name="TKey3">第三层主键类型</typeparam>
    /// <typeparam name="T">配置类型</typeparam>
    public class MapContainer<TKey1, TKey2, TKey3, T> : IMapContainer<TKey1, TKey2, TKey3, T>
    {
        private readonly Dictionary<TKey1, Dictionary<TKey2, Dictionary<TKey3, T>>> mData;
        private readonly List<T> mAllItems;

        public MapContainer()
        {
            mData = new Dictionary<TKey1, Dictionary<TKey2, Dictionary<TKey3, T>>>();
            mAllItems = new List<T>();
        }

        public int Count => mAllItems.Count;
        public Type ConfigType => typeof(T);

        public T Get(TKey1 key1, TKey2 key2, TKey3 key3)
        {
            if (mData.TryGetValue(key1, out var dict1) &&
                dict1.TryGetValue(key2, out var dict2) &&
                dict2.TryGetValue(key3, out var value))
            {
                return value;
            }
            throw new KeyNotFoundException($"配置不存在: key1={key1}, key2={key2}, key3={key3}");
        }

        public IReadOnlyDictionary<TKey3, T> Get(TKey1 key1, TKey2 key2)
        {
            if (mData.TryGetValue(key1, out var dict1) && dict1.TryGetValue(key2, out var dict2))
            {
                return dict2;
            }
            return new Dictionary<TKey3, T>();
        }

        public IReadOnlyDictionary<TKey2, IReadOnlyDictionary<TKey3, T>> Get(TKey1 key1)
        {
            if (mData.TryGetValue(key1, out var dict1))
            {
                return dict1.ToDictionary(
                    kvp => kvp.Key,
                    kvp => (IReadOnlyDictionary<TKey3, T>)kvp.Value
                );
            }
            return new Dictionary<TKey2, IReadOnlyDictionary<TKey3, T>>();
        }

        public void Add(TKey1 key1, TKey2 key2, TKey3 key3, T value)
        {
            if (!mData.TryGetValue(key1, out var dict1))
            {
                dict1 = new Dictionary<TKey2, Dictionary<TKey3, T>>();
                mData[key1] = dict1;
            }

            if (!dict1.TryGetValue(key2, out var dict2))
            {
                dict2 = new Dictionary<TKey3, T>();
                dict1[key2] = dict2;
            }

            if (dict2.ContainsKey(key3))
            {
                throw new ArgumentException($"重复的主键: key1={key1}, key2={key2}, key3={key3}");
            }

            dict2[key3] = value;
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
