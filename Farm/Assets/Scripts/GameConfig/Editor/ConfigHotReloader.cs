// ==========================================================
// 自动生成配置系统 - 编辑器配置热重载
// ==========================================================

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FarmGame.GameConfig.Editor
{
    /// <summary>
    /// 配置文件变更监听器
    /// 在编辑器中监听配置文件变化，自动触发重载
    /// </summary>
    [InitializeOnLoad]
    public static class ConfigHotReloader
    {
        /// <summary>
        /// 配置文件夹路径
        /// </summary>
        private static string ConfigFolder = "Assets/Configs";

        /// <summary>
        /// 是否启用热重载
        /// </summary>
        private static bool sEnabled = true;

        /// <summary>
        /// 文件最后修改时间缓存
        /// </summary>
        private static readonly Dictionary<string, DateTime> sFileTimestamps = new Dictionary<string, DateTime>();

        /// <summary>
        /// 上次检查时间
        /// </summary>
        private static double sLastCheckTime;

        /// <summary>
        /// 检查间隔（秒）
        /// </summary>
        private const float CHECK_INTERVAL = 1.0f;

        /// <summary>
        /// 配置变更事件
        /// </summary>
        public static event Action<string> OnConfigFileChanged;

        static ConfigHotReloader()
        {
            EditorApplication.update += OnEditorUpdate;
            RefreshTimestamps();
            Debug.Log("[ConfigHotReloader] 配置热重载已启用");
        }

        /// <summary>
        /// 启用/禁用热重载
        /// </summary>
        public static bool Enabled
        {
            get => sEnabled;
            set
            {
                sEnabled = value;
                if (value)
                {
                    RefreshTimestamps();
                }
            }
        }

        /// <summary>
        /// 编辑器更新回调
        /// </summary>
        private static void OnEditorUpdate()
        {
            if (!sEnabled)
            {
                return;
            }

            // 检查间隔
            var currentTime = EditorApplication.timeSinceStartup;
            if (currentTime - sLastCheckTime < CHECK_INTERVAL)
            {
                return;
            }
            sLastCheckTime = currentTime;

            // 检查文件变更
            CheckFileChanges();
        }

        /// <summary>
        /// 刷新文件时间戳缓存
        /// </summary>
        private static void RefreshTimestamps()
        {
            sFileTimestamps.Clear();

            var fullPath = Path.GetFullPath(ConfigFolder);
            if (!Directory.Exists(fullPath))
            {
                return;
            }

            var files = Directory.GetFiles(fullPath, "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var ext = Path.GetExtension(file).ToLower();
                if (ext == ".csv" || ext == ".xlsx")
                {
                    sFileTimestamps[file] = File.GetLastWriteTime(file);
                }
            }
        }

        /// <summary>
        /// 检查文件变更
        /// </summary>
        private static void CheckFileChanges()
        {
            var fullPath = Path.GetFullPath(ConfigFolder);
            if (!Directory.Exists(fullPath))
            {
                return;
            }

            var files = Directory.GetFiles(fullPath, "*.*", SearchOption.AllDirectories);
            var changedFiles = new List<string>();

            foreach (var file in files)
            {
                var ext = Path.GetExtension(file).ToLower();
                if (ext != ".csv" && ext != ".xlsx")
                {
                    continue;
                }

                // 跳过临时文件
                if (Path.GetFileName(file).StartsWith("~"))
                {
                    continue;
                }

                var lastWriteTime = File.GetLastWriteTime(file);

                if (sFileTimestamps.TryGetValue(file, out var cachedTime))
                {
                    if (lastWriteTime > cachedTime)
                    {
                        changedFiles.Add(file);
                        sFileTimestamps[file] = lastWriteTime;
                    }
                }
                else
                {
                    // 新文件
                    sFileTimestamps[file] = lastWriteTime;
                    changedFiles.Add(file);
                }
            }

            // 处理变更的文件
            foreach (var file in changedFiles)
            {
                HandleFileChanged(file);
            }
        }

        /// <summary>
        /// 处理文件变更
        /// </summary>
        private static void HandleFileChanged(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            Debug.Log($"[ConfigHotReloader] 检测到配置文件变更: {fileName}");

            // 触发事件
            OnConfigFileChanged?.Invoke(filePath);

            // 如果游戏正在运行，自动重载配置
            if (Application.isPlaying && ConfigManager.Instance != null && ConfigManager.Instance.IsInitialized)
            {
                ReloadConfigFile(filePath);
            }
        }

        /// <summary>
        /// 重载指定的配置文件
        /// </summary>
        private static void ReloadConfigFile(string filePath)
        {
            try
            {
                var ext = Path.GetExtension(filePath).ToLower();
                if (ext != ".csv")
                {
                    Debug.LogWarning($"[ConfigHotReloader] 热重载暂只支持 CSV 文件: {filePath}");
                    return;
                }

                // 解析 Schema
                var parser = new ConfigSchemaParser();
                using var reader = new CsvExcelReader();
                reader.Open(filePath);

                var schema = parser.Parse(reader, filePath);

                // 查找对应的配置类型
                var configType = FindConfigType(schema.ClassName);
                if (configType == null)
                {
                    Debug.LogWarning($"[ConfigHotReloader] 未找到配置类: {schema.ClassName}");
                    return;
                }

                // 重新加载数据
                var loader = new ConfigLoader();
                using var newReader = new CsvExcelReader();
                newReader.Open(filePath);

                var container = loader.Load(newReader, filePath, configType);

                // 注册到 ConfigManager
                var registerMethod = typeof(ConfigManager).GetMethod("Register");
                var genericMethod = registerMethod.MakeGenericMethod(configType);
                genericMethod.Invoke(ConfigManager.Instance, new object[] { container });

                Debug.Log($"[ConfigHotReloader] 已热重载配置: {schema.ClassName} ({container.Count} 条记录)");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ConfigHotReloader] 热重载失败: {filePath}\n{ex}");
            }
        }

        /// <summary>
        /// 查找配置类型
        /// </summary>
        private static Type FindConfigType(string className)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes();
                    foreach (var type in types)
                    {
                        if (type.Name == className && type.Namespace != null && type.Namespace.Contains("GameConfig"))
                        {
                            return type;
                        }
                    }
                }
                catch
                {
                    // 忽略
                }
            }
            return null;
        }

        /// <summary>
        /// 手动触发重载所有配置
        /// </summary>
        [MenuItem("Tools/FarmGame/Reload All Configs", false, 101)]
        public static void ReloadAllConfigs()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[ConfigHotReloader] 请在游戏运行时执行此操作");
                return;
            }

            var fullPath = Path.GetFullPath(ConfigFolder);
            if (!Directory.Exists(fullPath))
            {
                Debug.LogWarning($"[ConfigHotReloader] 配置文件夹不存在: {ConfigFolder}");
                return;
            }

            var files = Directory.GetFiles(fullPath, "*.csv", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                if (!Path.GetFileName(file).StartsWith("~"))
                {
                    ReloadConfigFile(file);
                }
            }

            Debug.Log($"[ConfigHotReloader] 已重载 {files.Length} 个配置文件");
        }

        /// <summary>
        /// 切换热重载状态
        /// </summary>
        [MenuItem("Tools/FarmGame/Toggle Config Hot Reload", false, 102)]
        public static void ToggleHotReload()
        {
            Enabled = !Enabled;
            Debug.Log($"[ConfigHotReloader] 热重载已{(Enabled ? "启用" : "禁用")}");
        }
    }
}
#endif
