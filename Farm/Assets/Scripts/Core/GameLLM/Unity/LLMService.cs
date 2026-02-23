using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using FarmGame.GameConfig;
using FarmGame.GameConfig.Generated;

namespace FarmGame.GameLLM
{
    /// <summary>
    /// LLM 服务 (纯 C# 单例，不依赖 GameObject)
    /// </summary>
    public class LLMService
    {
        // 1. 单例实现
        private static LLMService _instance;
        public static LLMService Instance => _instance ??= new LLMService();

        public static LLMClient Client => Instance?._client;

        private LLMClient _client;
        private bool mIsInitialized = false;

        // 私有构造，防止外部 new
        private LLMService() { }

        public void Initialize()
        {
            if (mIsInitialized) return;

            Debug.Log("[LLMService] Starting initialization from config...");
            
            // 从配置表读取启用的 LLM 配置
            var settingsConfig = LlmSettingsHelper.GetEnabledConfig();
            if (settingsConfig == null)
            {
                Debug.LogError("[LLMService] 无法获取 LLM 配置，请检查 llm_settings 配置表是否正确加载且有启用的配置");
                return;
            }
            
            // 解析 provider_type 字符串到枚举
            if (!Enum.TryParse<LLMProviderType>(settingsConfig.provider_type, true, out var providerType))
            {
                Debug.LogError($"[LLMService] 无法识别的 provider_type: {settingsConfig.provider_type}");
                return;
            }
            
            var config = new LLMConfig
            {
                ProviderType = providerType,
                ApiKey = settingsConfig.api_key,
                BaseUrl = settingsConfig.base_url,
                DefaultModel = settingsConfig.default_model
            };

            _client = LLMClientFactory.Create(config);
            mIsInitialized = true;
            Debug.Log($"[LLMService] Initialized with {providerType}, Model: {settingsConfig.default_model}");
        }
    }
}
