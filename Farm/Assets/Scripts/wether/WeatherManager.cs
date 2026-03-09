using UnityEngine;
using QFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FarmGame.Weather
{
    /// <summary>
    /// 天气类型定义
    /// </summary>
    public enum WeatherType
    {
        Sunny,      // 晴天
        Rainy,      // 雨天
        Cloudy,     // 多云
    }

    /// <summary>
    /// 天气系统管理器
    /// 负责管理游戏内的天气状态、切换以及通知
    /// </summary>
    public class WeatherManager : MonoSingleton<WeatherManager>
    {
        /// <summary>
        /// 天气属性配置
        /// </summary>
        private class WeatherConfig
        {
            public int Weight;
            public Vector2 TempRiseRange;      // 升温能力范围 (Min, Max)
            public Vector2 HumRiseRange;       // 升湿度能力范围 (Min, Max)
            public Vector2 RainIntensityRange; // 下雨大小范围 (Min, Max)
            public Vector2 DurationRange;      // 天气时长范围 (Min, Max)
        }

        #region 数据与事件

        // 天气配置表
        private Dictionary<WeatherType, WeatherConfig> mWeatherConfigs;

        // 基础环境属性变化速度
        private const float BASE_TEMP_DROP_SPEED = 2.0f; // 基础降温速度
        private const float BASE_HUM_DROP_SPEED = 2.0f;  // 基础降湿速度

        // 当前环境状态
        public float CurrentTemperature { get; private set; } = 20.0f; // 初始温度
        public float CurrentHumidity { get; private set; } = 30.0f;    // 初始湿度

        // 当前天气生成的随机属性
        public float CurrentTempRiseCapability { get; private set; }
        public float CurrentHumRiseCapability { get; private set; }
        public float CurrentRainIntensity { get; private set; }
        public float CurrentDuration { get; private set; }

        private float mTimer;

        public WeatherType CurrentWeather { get; private set; }

        // 天气变化事件，参数为新天气类型
        // 外部系统（如UI、特效、农作物生长）可监听此事件
        public event Action<WeatherType> OnWeatherChanged;
        
        // 下雨持续事件 (参数：当前帧的降雨量 = 强度 * deltaTime)
        // 外部土壤系统监听此事件来增加湿度
        public event Action<float> OnRainLoop;

        #endregion

        #region 初始化

        private void InitConfigs()
        {
            mWeatherConfigs = new Dictionary<WeatherType, WeatherConfig>
            {
                { 
                    WeatherType.Sunny, new WeatherConfig 
                    { 
                        Weight = 50, 
                        TempRiseRange = new Vector2(5f, 10f), 
                        HumRiseRange = new Vector2(0f, 2f), 
                        RainIntensityRange = Vector2.zero, 
                        DurationRange = new Vector2(10f, 20f) 
                    } 
                },
                { 
                    WeatherType.Cloudy, new WeatherConfig 
                    { 
                        Weight = 30, 
                        TempRiseRange = new Vector2(2f, 5f), 
                        HumRiseRange = new Vector2(1f, 3f), 
                        RainIntensityRange = Vector2.zero, 
                        DurationRange = new Vector2(10f, 15f) 
                    } 
                },
                { 
                    WeatherType.Rainy, new WeatherConfig 
                    { 
                        Weight = 20, 
                        TempRiseRange = new Vector2(0f, 2f), 
                        HumRiseRange = new Vector2(5f, 10f), 
                        RainIntensityRange = new Vector2(5f, 15f), 
                        DurationRange = new Vector2(8f, 12f) 
                    } 
                },

            };
        }

        /// <summary>
        /// 初始化天气系统
        /// </summary>
        public void Initialize()
        {
            InitConfigs();
            // 默认初始天气，后续可接入存档系统读取
            ChangeWeather(WeatherType.Sunny);
            Debug.Log("[WeatherManager] Initialized");
        }

        #endregion

        #region 生命周期

        private void Update()
        {
            // 只有初始化后才运行
            if (mWeatherConfigs == null) return;

            float dt = Time.deltaTime;

            // 1. 更新天气计时
            mTimer += dt;
            if (mTimer >= CurrentDuration)
            {
                RandomizeWeather();
            }

            // 2. 环境温度变化
            // 当前温度变化值 = 天气升温能力 - 基础温度降低速度
            float tempChange = CurrentTempRiseCapability - BASE_TEMP_DROP_SPEED;
            CurrentTemperature += tempChange * dt;

            // 3. 环境湿度变化
            // 当前湿度变化值 = 天气升湿能力 - 基础湿度降低速度
            float humChange = CurrentHumRiseCapability - BASE_HUM_DROP_SPEED;
            CurrentHumidity += humChange * dt;

            // 限制范围 (示例：-10到50度，0到100湿度)
            CurrentTemperature = Mathf.Clamp(CurrentTemperature, -10f, 50f);
            CurrentHumidity = Mathf.Clamp(CurrentHumidity, 0f, 100f);

            // 4. 下雨行为
            if (CurrentWeather == WeatherType.Rainy)
            {
                // 触发下雨事件，传入当前帧的雨量
                // 外部系统（如土地）监听此事件增加湿度
                float rainAmount = CurrentRainIntensity * dt;
                OnRainLoop?.Invoke(rainAmount);
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 切换天气
        /// </summary>
        /// <param name="newWeather">目标天气</param>
        public void ChangeWeather(WeatherType newWeather)
        {
            // 即使天气类型相同，也重新生成属性（因为是新的周期）
            // if (CurrentWeather == newWeather) return; 

            CurrentWeather = newWeather;
            
            // 生成随机属性
            if (mWeatherConfigs.TryGetValue(newWeather, out var config))
            {
                CurrentTempRiseCapability = UnityEngine.Random.Range(config.TempRiseRange.x, config.TempRiseRange.y);
                CurrentHumRiseCapability = UnityEngine.Random.Range(config.HumRiseRange.x, config.HumRiseRange.y);
                CurrentRainIntensity = UnityEngine.Random.Range(config.RainIntensityRange.x, config.RainIntensityRange.y);
                CurrentDuration = UnityEngine.Random.Range(config.DurationRange.x, config.DurationRange.y);
            }
            else
            {
                // 默认值
                CurrentTempRiseCapability = 0;
                CurrentHumRiseCapability = 0;
                CurrentRainIntensity = 0;
                CurrentDuration = 10f;
            }

            // 重置计时器
            mTimer = 0f;
            
            // 触发事件通知其他系统
            OnWeatherChanged?.Invoke(CurrentWeather);
            
            Debug.Log($"[WeatherManager] Weather changed to: {CurrentWeather}, Duration: {CurrentDuration:F1}s, TempRise: {CurrentTempRiseCapability:F1}, Rain: {CurrentRainIntensity:F1}");
        }

        /// <summary>
        /// 随机切换天气（可用于调试或每日随机）
        /// </summary>
        public void RandomizeWeather()
        {
            int totalWeight = mWeatherConfigs.Values.Sum(c => c.Weight);
            int randomValue = UnityEngine.Random.Range(0, totalWeight);
            int currentWeight = 0;

            foreach (var kvp in mWeatherConfigs)
            {
                currentWeight += kvp.Value.Weight;
                if (randomValue < currentWeight)
                {
                    ChangeWeather(kvp.Key);
                    return;
                }
            }
        }

        #endregion
    }
}
