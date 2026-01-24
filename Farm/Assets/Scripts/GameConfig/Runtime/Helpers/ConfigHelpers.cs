// ==========================================================
// 配置系统 - 游戏配置助手
// 提供类型安全的配置访问
// ==========================================================

using System;
using System.Collections.Generic;
using System.Linq;
using FarmGame.GameConfig.Generated;
using UnityEngine;

namespace FarmGame.GameConfig
{
    /// <summary>
    /// 游戏配置助手
    /// 提供对 GameSettingsConfig 的类型安全访问
    /// </summary>
    public static class GameSettingsHelper
    {
        private static Dictionary<string, GameSettingsConfig> sCache;

        /// <summary>
        /// 初始化缓存
        /// </summary>
        private static void EnsureCache()
        {
            if (sCache != null)
            {
                return;
            }

            sCache = new Dictionary<string, GameSettingsConfig>();

            if (ConfigManager.Instance == null || !ConfigManager.Instance.IsInitialized)
            {
                Debug.LogWarning("[GameSettingsHelper] ConfigManager 未初始化");
                return;
            }

            if (!ConfigManager.Instance.HasConfig<GameSettingsConfig>())
            {
                Debug.LogWarning("[GameSettingsHelper] GameSettingsConfig 未加载");
                return;
            }

            var container = ConfigManager.Instance.GetMap<string, GameSettingsConfig>();
            foreach (var key in container.GetKeys())
            {
                sCache[key] = container.Get(key);
            }
        }

        /// <summary>
        /// 刷新缓存
        /// </summary>
        public static void RefreshCache()
        {
            sCache = null;
            EnsureCache();
        }

        /// <summary>
        /// 获取字符串配置
        /// </summary>
        public static string GetString(string key, string defaultValue = "")
        {
            EnsureCache();
            if (sCache != null && sCache.TryGetValue(key, out var config))
            {
                return config.setting_value ?? defaultValue;
            }
            return defaultValue;
        }

        /// <summary>
        /// 获取整数配置
        /// </summary>
        public static int GetInt(string key, int defaultValue = 0)
        {
            EnsureCache();
            if (sCache != null && sCache.TryGetValue(key, out var config))
            {
                if (int.TryParse(config.setting_value, out var result))
                {
                    return result;
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// 获取浮点数配置
        /// </summary>
        public static float GetFloat(string key, float defaultValue = 0f)
        {
            EnsureCache();
            if (sCache != null && sCache.TryGetValue(key, out var config))
            {
                if (float.TryParse(config.setting_value, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var result))
                {
                    return result;
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// 获取布尔配置
        /// </summary>
        public static bool GetBool(string key, bool defaultValue = false)
        {
            EnsureCache();
            if (sCache != null && sCache.TryGetValue(key, out var config))
            {
                var value = config.setting_value?.ToLower();
                return value == "true" || value == "1" || value == "yes";
            }
            return defaultValue;
        }

        /// <summary>
        /// 获取 Vector2 配置
        /// </summary>
        public static Vector2 GetVector2(string keyX, string keyY, Vector2 defaultValue = default)
        {
            return new Vector2(
                GetFloat(keyX, defaultValue.x),
                GetFloat(keyY, defaultValue.y)
            );
        }

        #region 常用配置快捷访问

        /// <summary>初始地图名称</summary>
        public static string InitialMap => GetString("initial_map", "Farm");

        /// <summary>出生点坐标</summary>
        public static Vector2 SpawnPosition => GetVector2("spawn_position_x", "spawn_position_y", new Vector2(5, 5));

        /// <summary>玩家预制体路径</summary>
        public static string PlayerPrefabPath => GetString("player_prefab_path", "prefabs/Player");

        /// <summary>玩家移动速度</summary>
        public static float PlayerMoveSpeed => GetFloat("player_move_speed", 5f);

        /// <summary>相机跟随速度</summary>
        public static float CameraFollowSpeed => GetFloat("camera_follow_speed", 5f);

        #endregion
    }

    /// <summary>
    /// LLM 配置助手
    /// 提供对 LlmSettingsConfig 的类型安全访问
    /// </summary>
    public static class LlmSettingsHelper
    {
        /// <summary>
        /// 获取启用的 LLM 配置
        /// </summary>
        public static LlmSettingsConfig GetEnabledConfig()
        {
            if (ConfigManager.Instance == null || !ConfigManager.Instance.IsInitialized)
            {
                Debug.LogWarning("[LlmSettingsHelper] ConfigManager 未初始化");
                return null;
            }

            if (!ConfigManager.Instance.HasConfig<LlmSettingsConfig>())
            {
                Debug.LogWarning("[LlmSettingsHelper] LlmSettingsConfig 未加载");
                return null;
            }

            var container = ConfigManager.Instance.GetMap<int, LlmSettingsConfig>();
            var allConfigs = container.GetAll();

            // 返回第一个启用的配置
            return allConfigs.FirstOrDefault(c => c.enabled);
        }

        /// <summary>
        /// 获取指定 ID 的配置
        /// </summary>
        public static LlmSettingsConfig GetConfig(int settingId)
        {
            if (ConfigManager.Instance == null || !ConfigManager.Instance.IsInitialized)
            {
                return null;
            }

            if (!ConfigManager.Instance.HasConfig<LlmSettingsConfig>())
            {
                return null;
            }

            var container = ConfigManager.Instance.GetMap<int, LlmSettingsConfig>();
            if (container.TryGet(settingId, out var config))
            {
                return config;
            }
            return null;
        }

        /// <summary>
        /// 获取所有配置
        /// </summary>
        public static IReadOnlyList<LlmSettingsConfig> GetAllConfigs()
        {
            if (ConfigManager.Instance == null || !ConfigManager.Instance.IsInitialized)
            {
                return Array.Empty<LlmSettingsConfig>();
            }

            if (!ConfigManager.Instance.HasConfig<LlmSettingsConfig>())
            {
                return Array.Empty<LlmSettingsConfig>();
            }

            var container = ConfigManager.Instance.GetMap<int, LlmSettingsConfig>();
            return container.GetAll();
        }
    }
}
