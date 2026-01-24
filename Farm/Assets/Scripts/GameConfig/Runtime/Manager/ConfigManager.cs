// ==========================================================
// 自动生成配置系统 - 配置管理器
// ==========================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using QFramework;
using UnityEngine;

namespace FarmGame.GameConfig
{
    /// <summary>
    /// 配置加载进度委托
    /// </summary>
    /// <param name="current">当前进度</param>
    /// <param name="total">总数</param>
    /// <param name="fileName">当前文件名</param>
    public delegate void ConfigLoadProgressHandler(int current, int total, string fileName);

    /// <summary>
    /// 配置管理器
    /// 统一入口，管理所有配置的加载和访问
    /// </summary>
    public class ConfigManager : MonoSingleton<ConfigManager>
    {
        #region 私有字段

        private bool mIsInitialized;
        private bool mIsLoaded;

        /// <summary>
        /// 配置容器字典 - 按配置类型存储
        /// </summary>
        private readonly Dictionary<Type, IConfigContainer> mContainers = new Dictionary<Type, IConfigContainer>();

        /// <summary>
        /// 配置加载器
        /// </summary>
        private ConfigLoader mLoader;

        /// <summary>
        /// Excel 读取器工厂
        /// </summary>
        private IExcelReaderFactory mExcelReaderFactory;

        /// <summary>
        /// 已注册的配置类型映射（类名 -> 类型）
        /// </summary>
        private readonly Dictionary<string, Type> mConfigTypeMap = new Dictionary<string, Type>();

        #endregion

        #region 公共属性

        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized => mIsInitialized;

        /// <summary>
        /// 是否已加载配置
        /// </summary>
        public bool IsLoaded => mIsLoaded;

        /// <summary>
        /// Excel 读取器工厂
        /// </summary>
        public IExcelReaderFactory ExcelReaderFactory
        {
            get => mExcelReaderFactory;
            set => mExcelReaderFactory = value;
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化配置管理器
        /// </summary>
        public void Initialize()
        {
            if (mIsInitialized) return;

            mLoader = new ConfigLoader();
            mContainers.Clear();
            mConfigTypeMap.Clear();

            // 默认使用 CSV 读取器
            if (mExcelReaderFactory == null)
            {
                mExcelReaderFactory = new CsvExcelReaderFactory();
            }

            // 自动注册所有配置类型
            RegisterConfigTypes();

            mIsInitialized = true;
            Debug.Log("[ConfigManager] 配置管理器初始化完成");
        }

        /// <summary>
        /// 初始化并自动加载配置
        /// </summary>
        /// <param name="configFolder">配置文件夹路径</param>
        public async UniTask InitializeAndLoadAsync(string configFolder = "Assets/Configs")
        {
            Initialize();
            await LoadAllCsvAsync(configFolder);
        }

        /// <summary>
        /// 自动注册所有配置类型
        /// </summary>
        private void RegisterConfigTypes()
        {
            // 查找所有 Generated 命名空间下的配置类
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Config"))
                        .Where(t => t.Namespace != null && t.Namespace.Contains("GameConfig"));

                    foreach (var type in types)
                    {
                        mConfigTypeMap[type.Name] = type;
                    }
                }
                catch
                {
                    // 忽略无法加载的程序集
                }
            }

            Debug.Log($"[ConfigManager] 已注册 {mConfigTypeMap.Count} 个配置类型");
        }

        #endregion

        #region 配置加载

        /// <summary>
        /// 异步加载所有配置
        /// </summary>
        /// <param name="folderPath">配置文件夹路径</param>
        /// <param name="searchPattern">文件搜索模式</param>
        /// <param name="onProgress">进度回调</param>
        public async UniTask LoadAllAsync(
            string folderPath,
            string searchPattern = "*.xlsx",
            ConfigLoadProgressHandler onProgress = null)
        {
            if (mExcelReaderFactory == null)
            {
                Debug.LogWarning("[ConfigManager] 未设置 ExcelReaderFactory，无法加载 Excel 配置");
                mIsLoaded = true;
                return;
            }

            if (!Directory.Exists(folderPath))
            {
                Debug.LogWarning($"[ConfigManager] 配置文件夹不存在: {folderPath}");
                mIsLoaded = true;
                return;
            }

            var files = Directory.GetFiles(folderPath, searchPattern, SearchOption.AllDirectories)
                .Where(f => !Path.GetFileName(f).StartsWith("~")) // 排除临时文件
                .ToArray();

            Debug.Log($"[ConfigManager] 找到 {files.Length} 个配置文件");

            for (int i = 0; i < files.Length; i++)
            {
                var filePath = files[i];
                var fileName = Path.GetFileName(filePath);

                try
                {
                    onProgress?.Invoke(i + 1, files.Length, fileName);
                    await LoadFileAsync(filePath);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[ConfigManager] 加载配置失败: {fileName}\n{ex}");
                }

                // 让出主线程
                await UniTask.Yield();
            }

            mIsLoaded = true;
            Debug.Log($"[ConfigManager] 配置加载完成，共 {mContainers.Count} 个配置表");
        }

        /// <summary>
        /// 加载所有 CSV 配置文件
        /// </summary>
        /// <param name="folderPath">配置文件夹路径</param>
        /// <param name="onProgress">进度回调</param>
        public async UniTask LoadAllCsvAsync(
            string folderPath = "Assets/Configs",
            ConfigLoadProgressHandler onProgress = null)
        {
            // 确保使用 CSV 读取器
            mExcelReaderFactory = new CsvExcelReaderFactory();
            await LoadAllAsync(folderPath, "*.csv", onProgress);
        }

        /// <summary>
        /// 加载单个配置文件
        /// </summary>
        private async UniTask LoadFileAsync(string filePath)
        {
            using var reader = mExcelReaderFactory.Create();
            reader.Open(filePath);

            // 解析 Schema 获取类名
            var parser = new ConfigSchemaParser();
            var schema = parser.Parse(reader, filePath);

            // 查找对应的配置类型
            if (!mConfigTypeMap.TryGetValue(schema.ClassName, out var configType))
            {
                Debug.LogWarning($"[ConfigManager] 未找到配置类: {schema.ClassName}，文件: {filePath}");
                return;
            }

            // 加载数据
            var container = mLoader.Load(reader, filePath, configType);
            mContainers[configType] = container;

            Debug.Log($"[ConfigManager] 已加载: {schema.ClassName} ({container.Count} 条记录)");
            await UniTask.CompletedTask;
        }

        #endregion

        #region 配置访问

        /// <summary>
        /// 注册配置容器
        /// </summary>
        /// <typeparam name="T">配置类型</typeparam>
        /// <param name="container">配置容器</param>
        public void Register<T>(IConfigContainer container)
        {
            mContainers[typeof(T)] = container;
        }

        /// <summary>
        /// 获取 List 容器
        /// </summary>
        /// <typeparam name="T">配置类型</typeparam>
        public IListContainer<T> GetList<T>() where T : class
        {
            if (mContainers.TryGetValue(typeof(T), out var container))
            {
                return container as IListContainer<T>;
            }
            throw new KeyNotFoundException($"未找到配置: {typeof(T).Name}");
        }

        /// <summary>
        /// 获取单层 Map 容器
        /// </summary>
        /// <typeparam name="TKey">主键类型</typeparam>
        /// <typeparam name="T">配置类型</typeparam>
        public IMapContainer<TKey, T> GetMap<TKey, T>() where T : class
        {
            if (mContainers.TryGetValue(typeof(T), out var container))
            {
                return container as IMapContainer<TKey, T>;
            }
            throw new KeyNotFoundException($"未找到配置: {typeof(T).Name}");
        }

        /// <summary>
        /// 获取双层 Map 容器
        /// </summary>
        /// <typeparam name="TKey1">第一层主键类型</typeparam>
        /// <typeparam name="TKey2">第二层主键类型</typeparam>
        /// <typeparam name="T">配置类型</typeparam>
        public IMapContainer<TKey1, TKey2, T> GetMap<TKey1, TKey2, T>() where T : class
        {
            if (mContainers.TryGetValue(typeof(T), out var container))
            {
                return container as IMapContainer<TKey1, TKey2, T>;
            }
            throw new KeyNotFoundException($"未找到配置: {typeof(T).Name}");
        }

        /// <summary>
        /// 获取三层 Map 容器
        /// </summary>
        /// <typeparam name="TKey1">第一层主键类型</typeparam>
        /// <typeparam name="TKey2">第二层主键类型</typeparam>
        /// <typeparam name="TKey3">第三层主键类型</typeparam>
        /// <typeparam name="T">配置类型</typeparam>
        public IMapContainer<TKey1, TKey2, TKey3, T> GetMap<TKey1, TKey2, TKey3, T>() where T : class
        {
            if (mContainers.TryGetValue(typeof(T), out var container))
            {
                return container as IMapContainer<TKey1, TKey2, TKey3, T>;
            }
            throw new KeyNotFoundException($"未找到配置: {typeof(T).Name}");
        }

        /// <summary>
        /// 获取单层 GroupMap 容器
        /// </summary>
        /// <typeparam name="TKey">主键类型</typeparam>
        /// <typeparam name="T">配置类型</typeparam>
        public IGroupMapContainer<TKey, T> GetGroupMap<TKey, T>() where T : class
        {
            if (mContainers.TryGetValue(typeof(T), out var container))
            {
                return container as IGroupMapContainer<TKey, T>;
            }
            throw new KeyNotFoundException($"未找到配置: {typeof(T).Name}");
        }

        /// <summary>
        /// 获取双层 GroupMap 容器
        /// </summary>
        /// <typeparam name="TKey1">第一层主键类型</typeparam>
        /// <typeparam name="TKey2">第二层主键类型</typeparam>
        /// <typeparam name="T">配置类型</typeparam>
        public IGroupMapContainer<TKey1, TKey2, T> GetGroupMap<TKey1, TKey2, T>() where T : class
        {
            if (mContainers.TryGetValue(typeof(T), out var container))
            {
                return container as IGroupMapContainer<TKey1, TKey2, T>;
            }
            throw new KeyNotFoundException($"未找到配置: {typeof(T).Name}");
        }

        /// <summary>
        /// 获取三层 GroupMap 容器
        /// </summary>
        /// <typeparam name="TKey1">第一层主键类型</typeparam>
        /// <typeparam name="TKey2">第二层主键类型</typeparam>
        /// <typeparam name="TKey3">第三层主键类型</typeparam>
        /// <typeparam name="T">配置类型</typeparam>
        public IGroupMapContainer<TKey1, TKey2, TKey3, T> GetGroupMap<TKey1, TKey2, TKey3, T>() where T : class
        {
            if (mContainers.TryGetValue(typeof(T), out var container))
            {
                return container as IGroupMapContainer<TKey1, TKey2, TKey3, T>;
            }
            throw new KeyNotFoundException($"未找到配置: {typeof(T).Name}");
        }

        /// <summary>
        /// 尝试获取配置容器
        /// </summary>
        /// <typeparam name="T">配置类型</typeparam>
        /// <param name="container">输出容器</param>
        /// <returns>是否找到</returns>
        public bool TryGetContainer<T>(out IConfigContainer<T> container) where T : class
        {
            if (mContainers.TryGetValue(typeof(T), out var c))
            {
                container = c as IConfigContainer<T>;
                return container != null;
            }
            container = null;
            return false;
        }

        /// <summary>
        /// 检查配置是否存在
        /// </summary>
        /// <typeparam name="T">配置类型</typeparam>
        public bool HasConfig<T>()
        {
            return mContainers.ContainsKey(typeof(T));
        }

        /// <summary>
        /// 获取所有已加载的配置类型
        /// </summary>
        public IEnumerable<Type> GetAllConfigTypes()
        {
            return mContainers.Keys;
        }

        #endregion

        #region 清理

        /// <summary>
        /// 清空所有配置
        /// </summary>
        public void ClearAll()
        {
            foreach (var container in mContainers.Values)
            {
                container.Clear();
            }
            mContainers.Clear();
            mIsLoaded = false;
            Debug.Log("[ConfigManager] 已清空所有配置");
        }

        private void OnDestroy()
        {
            ClearAll();
        }

        #endregion
    }
}
