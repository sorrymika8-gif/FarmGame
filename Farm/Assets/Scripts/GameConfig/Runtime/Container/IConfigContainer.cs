// ==========================================================
// 自动生成配置系统 - 容器接口定义
// ==========================================================

using System.Collections.Generic;

namespace FarmGame.GameConfig
{
    /// <summary>
    /// 配置容器基础接口
    /// </summary>
    public interface IConfigContainer
    {
        /// <summary>
        /// 配置数量
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 配置类型
        /// </summary>
        System.Type ConfigType { get; }

        /// <summary>
        /// 清空容器
        /// </summary>
        void Clear();
    }

    /// <summary>
    /// 泛型配置容器接口
    /// </summary>
    /// <typeparam name="T">配置类型</typeparam>
    public interface IConfigContainer<T> : IConfigContainer
    {
        /// <summary>
        /// 获取所有配置项
        /// </summary>
        IReadOnlyList<T> GetAll();
    }

    /// <summary>
    /// List 容器接口
    /// </summary>
    /// <typeparam name="T">配置类型</typeparam>
    public interface IListContainer<T> : IConfigContainer<T>
    {
        /// <summary>
        /// 添加配置项
        /// </summary>
        void Add(T item);

        /// <summary>
        /// 通过索引获取配置项
        /// </summary>
        T GetByIndex(int index);
    }

    /// <summary>
    /// 单层 Map 容器接口
    /// </summary>
    /// <typeparam name="TKey">主键类型</typeparam>
    /// <typeparam name="T">配置类型</typeparam>
    public interface IMapContainer<TKey, T> : IConfigContainer<T>
    {
        /// <summary>
        /// 通过主键获取配置项
        /// </summary>
        T Get(TKey key);

        /// <summary>
        /// 尝试获取配置项
        /// </summary>
        bool TryGet(TKey key, out T value);

        /// <summary>
        /// 检查主键是否存在
        /// </summary>
        bool ContainsKey(TKey key);

        /// <summary>
        /// 添加配置项
        /// </summary>
        void Add(TKey key, T value);

        /// <summary>
        /// 获取所有主键
        /// </summary>
        IEnumerable<TKey> GetKeys();
    }

    /// <summary>
    /// 双层 Map 容器接口
    /// </summary>
    /// <typeparam name="TKey1">第一层主键类型</typeparam>
    /// <typeparam name="TKey2">第二层主键类型</typeparam>
    /// <typeparam name="T">配置类型</typeparam>
    public interface IMapContainer<TKey1, TKey2, T> : IConfigContainer<T>
    {
        /// <summary>
        /// 通过双层主键获取配置项
        /// </summary>
        T Get(TKey1 key1, TKey2 key2);

        /// <summary>
        /// 获取第一层下的所有配置
        /// </summary>
        IReadOnlyDictionary<TKey2, T> Get(TKey1 key1);

        /// <summary>
        /// 尝试获取配置项
        /// </summary>
        bool TryGet(TKey1 key1, TKey2 key2, out T value);

        /// <summary>
        /// 检查主键是否存在
        /// </summary>
        bool ContainsKey(TKey1 key1, TKey2 key2);

        /// <summary>
        /// 添加配置项
        /// </summary>
        void Add(TKey1 key1, TKey2 key2, T value);

        /// <summary>
        /// 获取所有第一层主键
        /// </summary>
        IEnumerable<TKey1> GetKeys();
    }

    /// <summary>
    /// 三层 Map 容器接口
    /// </summary>
    /// <typeparam name="TKey1">第一层主键类型</typeparam>
    /// <typeparam name="TKey2">第二层主键类型</typeparam>
    /// <typeparam name="TKey3">第三层主键类型</typeparam>
    /// <typeparam name="T">配置类型</typeparam>
    public interface IMapContainer<TKey1, TKey2, TKey3, T> : IConfigContainer<T>
    {
        /// <summary>
        /// 通过三层主键获取配置项
        /// </summary>
        T Get(TKey1 key1, TKey2 key2, TKey3 key3);

        /// <summary>
        /// 获取第一、二层下的所有配置
        /// </summary>
        IReadOnlyDictionary<TKey3, T> Get(TKey1 key1, TKey2 key2);

        /// <summary>
        /// 获取第一层下的所有配置
        /// </summary>
        IReadOnlyDictionary<TKey2, IReadOnlyDictionary<TKey3, T>> Get(TKey1 key1);

        /// <summary>
        /// 添加配置项
        /// </summary>
        void Add(TKey1 key1, TKey2 key2, TKey3 key3, T value);
    }

    /// <summary>
    /// 单层 GroupMap 容器接口
    /// </summary>
    /// <typeparam name="TKey">主键类型</typeparam>
    /// <typeparam name="T">配置类型</typeparam>
    public interface IGroupMapContainer<TKey, T> : IConfigContainer<T>
    {
        /// <summary>
        /// 通过主键获取配置组
        /// </summary>
        IReadOnlyList<T> GetGroup(TKey key);

        /// <summary>
        /// 检查主键是否存在
        /// </summary>
        bool ContainsKey(TKey key);

        /// <summary>
        /// 添加配置项到组
        /// </summary>
        void Add(TKey key, T value);

        /// <summary>
        /// 获取所有主键
        /// </summary>
        IEnumerable<TKey> GetKeys();
    }

    /// <summary>
    /// 双层 GroupMap 容器接口
    /// </summary>
    /// <typeparam name="TKey1">第一层主键类型</typeparam>
    /// <typeparam name="TKey2">第二层主键类型</typeparam>
    /// <typeparam name="T">配置类型</typeparam>
    public interface IGroupMapContainer<TKey1, TKey2, T> : IConfigContainer<T>
    {
        /// <summary>
        /// 通过双层主键获取配置组
        /// </summary>
        IReadOnlyList<T> GetGroup(TKey1 key1, TKey2 key2);

        /// <summary>
        /// 获取第一层下的所有配置组
        /// </summary>
        IReadOnlyDictionary<TKey2, IReadOnlyList<T>> GetGroup(TKey1 key1);

        /// <summary>
        /// 添加配置项到组
        /// </summary>
        void Add(TKey1 key1, TKey2 key2, T value);

        /// <summary>
        /// 获取所有第一层主键
        /// </summary>
        IEnumerable<TKey1> GetKeys();
    }

    /// <summary>
    /// 三层 GroupMap 容器接口
    /// </summary>
    /// <typeparam name="TKey1">第一层主键类型</typeparam>
    /// <typeparam name="TKey2">第二层主键类型</typeparam>
    /// <typeparam name="TKey3">第三层主键类型</typeparam>
    /// <typeparam name="T">配置类型</typeparam>
    public interface IGroupMapContainer<TKey1, TKey2, TKey3, T> : IConfigContainer<T>
    {
        /// <summary>
        /// 通过三层主键获取配置组
        /// </summary>
        IReadOnlyList<T> GetGroup(TKey1 key1, TKey2 key2, TKey3 key3);

        /// <summary>
        /// 获取第一、二层下的所有配置组
        /// </summary>
        IReadOnlyDictionary<TKey3, IReadOnlyList<T>> GetGroup(TKey1 key1, TKey2 key2);

        /// <summary>
        /// 添加配置项到组
        /// </summary>
        void Add(TKey1 key1, TKey2 key2, TKey3 key3, T value);
    }
}
