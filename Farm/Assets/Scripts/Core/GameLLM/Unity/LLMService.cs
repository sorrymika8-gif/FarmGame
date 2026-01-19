using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

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

        // 2. 硬编码配置区域 (请修改此处)
        // ==========================================
        private const LLMProviderType CONF_PROVIDER = LLMProviderType.DeepSeek;
        
        // TODO: 请将 "sk-replace-with-your-key" 替换为您真实的 API Key
        private const string CONF_API_KEY = "sk-88310d74635747c38833c53f24ef02e7"; 
        
        // DeepSeek 默认 BaseURL
        private const string CONF_BASE_URL = "https://api.deepseek.com";
        // DeepSeek V3/R1 模型: deepseek-chat (或 deepseek-reasoner)
        private const string CONF_MODEL = "deepseek-reasoner";
        // ==========================================

        private LLMClient _client;
        private bool mIsInitialized = false;

        // 私有构造，防止外部 new
        private LLMService() { }

        public void Initialize()
        {
            if (mIsInitialized) return;

            Debug.Log("[LLMService] Starting initialization (Code Config)...");
            
            var config = new LLMConfig
            {
                ProviderType = CONF_PROVIDER,
                ApiKey = CONF_API_KEY,
                BaseUrl = CONF_BASE_URL,
                DefaultModel = CONF_MODEL
            };

            _client = LLMClientFactory.Create(config);
            mIsInitialized = true;
            Debug.Log($"[LLMService] Initialized with {CONF_PROVIDER}");
        }
    }
}
