using System;

namespace FarmGame.GameLLM
{
    /// <summary>
    /// Provider 类型枚举
    /// </summary>
    public enum LLMProviderType
    {
        OpenAI,
        Anthropic,
        AzureOpenAI,
        DeepSeek,
        Custom
    }

    /// <summary>
    /// 配置数据
    /// </summary>
    [Serializable]
    public class LLMConfig
    {
        public LLMProviderType ProviderType;
        public string ApiKey;
        public string BaseUrl;      // 可选，留空用默认值
        public string DefaultModel; // 可选，留空用默认值
    }

    /// <summary>
    /// 客户端工厂
    /// </summary>
    public static class LLMClientFactory
    {
        public static LLMClient Create(LLMConfig config)
        {
            ILLMProvider provider = config.ProviderType switch
            {
                LLMProviderType.OpenAI => CreateOpenAI(config),
                LLMProviderType.Anthropic => CreateAnthropic(config),
                LLMProviderType.AzureOpenAI => CreateAzureOpenAI(config),
                LLMProviderType.DeepSeek => CreateDeepSeek(config),
                LLMProviderType.Custom => CreateCustom(config),
                _ => throw new ArgumentException($"Unknown provider type: {config.ProviderType}")
            };

            return new LLMClient(provider);
        }

        private static ILLMProvider CreateOpenAI(LLMConfig config)
        {
            var baseUrl = string.IsNullOrEmpty(config.BaseUrl) 
                ? "https://api.openai.com/v1" 
                : config.BaseUrl;
            var model = string.IsNullOrEmpty(config.DefaultModel) 
                ? "gpt-4o-mini" 
                : config.DefaultModel;
                
            return new OpenAIProvider(config.ApiKey, baseUrl, model);
        }

        private static ILLMProvider CreateAnthropic(LLMConfig config)
        {
            var baseUrl = string.IsNullOrEmpty(config.BaseUrl) 
                ? "https://api.anthropic.com/v1" 
                : config.BaseUrl;
            var model = string.IsNullOrEmpty(config.DefaultModel) 
                ? "claude-3-5-sonnet-20240620" 
                : config.DefaultModel;
                
            return new AnthropicProvider(config.ApiKey, baseUrl, model);
        }

        private static ILLMProvider CreateAzureOpenAI(LLMConfig config)
        {
            // Azure OpenAI 使用 OpenAI Provider，但 URL 格式不同
            // URL 格式: https://{resource}.openai.azure.com/openai/deployments/{deployment}
            return new OpenAIProvider(config.ApiKey, config.BaseUrl, config.DefaultModel);
        }
        private static ILLMProvider CreateDeepSeek(LLMConfig config)
        {
            var baseUrl = string.IsNullOrEmpty(config.BaseUrl) 
                ? "https://api.deepseek.com" 
                : config.BaseUrl;
            var model = string.IsNullOrEmpty(config.DefaultModel) 
                ? "deepseek-chat" 
                : config.DefaultModel;
                
            return new DeepSeekProvider(config.ApiKey, baseUrl, model);
        }

        private static ILLMProvider CreateCustom(LLMConfig config)
        {
            // 自定义端点，假设兼容 OpenAI 接口（如 Ollama、vLLM）
            return new OpenAIProvider(config.ApiKey ?? "", config.BaseUrl, config.DefaultModel);
        }
    }
}
