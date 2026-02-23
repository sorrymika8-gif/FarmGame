using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Cysharp.Threading.Tasks;
using QFramework;
using FarmGame.GameLLM;

namespace FarmGame.Core.LLMDescription
{
    /// <summary>
    /// LLM 描述服务
    /// 提供通用的游戏对象描述生成功能
    /// </summary>
    public class LLMDescriptionService : MonoSingleton<LLMDescriptionService>
    {
        #region 私有字段

        /// <summary>
        /// 模板注册表
        /// Key: 对象类型（如 "Crop", "NPC"）, Value: 模板资源路径
        /// </summary>
        private readonly Dictionary<string, string> mTemplateRegistry = new();

        /// <summary>
        /// 模板内容缓存
        /// Key: 模板路径, Value: 模板内容
        /// </summary>
        private readonly Dictionary<string, string> mTemplateCache = new();

        /// <summary>
        /// 描述结果缓存
        /// Key: 对象缓存键, Value: 生成的描述
        /// </summary>
        private readonly Dictionary<string, string> mDescriptionCache = new();

        /// <summary>
        /// 上下文构建器
        /// </summary>
        private readonly DescriptionContextBuilder mContextBuilder = new();

        /// <summary>
        /// 是否已初始化
        /// </summary>
        private bool mIsInitialized;

        /// <summary>
        /// 默认模板目录
        /// </summary>
        private const string DEFAULT_TEMPLATE_DIR = "Prompts/Descriptions";

        /// <summary>
        /// 缓存最大数量
        /// </summary>
        private const int MAX_CACHE_SIZE = 100;

        #endregion

        #region 公共方法

        /// <summary>
        /// 初始化服务
        /// </summary>
        public void Initialize()
        {
            if (mIsInitialized) return;

            // 注册默认模板
            RegisterDefaultTemplates();

            mIsInitialized = true;
            Debug.Log("[LLMDescriptionService] Initialized");
        }

        /// <summary>
        /// 注册描述模板
        /// </summary>
        /// <param name="type">对象类型标识</param>
        /// <param name="templatePath">模板资源路径（相对于 Resources）</param>
        public void RegisterTemplate(string type, string templatePath)
        {
            if (string.IsNullOrEmpty(type))
            {
                Debug.LogWarning("[LLMDescriptionService] Cannot register template with empty type");
                return;
            }

            mTemplateRegistry[type] = templatePath;
            Debug.Log($"[LLMDescriptionService] Registered template: {type} -> {templatePath}");
        }

        /// <summary>
        /// 生成对象描述
        /// </summary>
        /// <param name="target">可描述对象</param>
        /// <param name="useCache">是否使用缓存</param>
        /// <returns>生成的描述文本，失败返回 null</returns>
        public async UniTask<string> GenerateDescriptionAsync(IDescribable target, bool useCache = true)
        {
            if (target == null)
            {
                Debug.LogWarning("[LLMDescriptionService] Target is null");
                return null;
            }

            var cacheKey = target.GetCacheKey();

            // 检查缓存
            if (useCache && mDescriptionCache.TryGetValue(cacheKey, out var cached))
            {
                Debug.Log($"[LLMDescriptionService] Cache hit: {cacheKey}");
                return cached;
            }

            try
            {
                // 构建上下文
                var context = mContextBuilder.Build(target);
                if (context == null)
                {
                    Debug.LogWarning("[LLMDescriptionService] Failed to build context");
                    return null;
                }

                // 获取模板
                var template = await GetTemplateAsync(target.DescriptionType);
                if (string.IsNullOrEmpty(template))
                {
                    Debug.LogWarning($"[LLMDescriptionService] Template not found for type: {target.DescriptionType}");
                    return null;
                }

                // 替换模板占位符
                var prompt = context.ReplaceTemplate(template);

                // 调用 LLM
                var description = await CallLLMAsync(prompt);

                // 缓存结果
                if (!string.IsNullOrEmpty(description) && useCache)
                {
                    CacheDescription(cacheKey, description);
                }

                return description;
            }
            catch (Exception e)
            {
                Debug.LogError($"[LLMDescriptionService] Error generating description: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 清除描述缓存
        /// </summary>
        /// <param name="cacheKey">指定缓存键，null 则清除所有</param>
        public void ClearCache(string cacheKey = null)
        {
            if (string.IsNullOrEmpty(cacheKey))
            {
                mDescriptionCache.Clear();
                Debug.Log("[LLMDescriptionService] All cache cleared");
            }
            else
            {
                mDescriptionCache.Remove(cacheKey);
                Debug.Log($"[LLMDescriptionService] Cache cleared: {cacheKey}");
            }
        }

        /// <summary>
        /// 预热模板（提前加载到缓存）
        /// </summary>
        /// <param name="type">对象类型</param>
        public async UniTask PreloadTemplateAsync(string type)
        {
            await GetTemplateAsync(type);
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 注册默认模板
        /// </summary>
        private void RegisterDefaultTemplates()
        {
            // 作物描述模板
            RegisterTemplate("Crop", $"{DEFAULT_TEMPLATE_DIR}/CropDescription.md");

            // 可扩展：注册其他默认模板
            // RegisterTemplate("NPC", $"{DEFAULT_TEMPLATE_DIR}/NPCDescription.md");
            // RegisterTemplate("Building", $"{DEFAULT_TEMPLATE_DIR}/BuildingDescription.md");
        }

        /// <summary>
        /// 获取模板内容
        /// </summary>
        /// <param name="type">对象类型</param>
        /// <returns>模板内容</returns>
        private async UniTask<string> GetTemplateAsync(string type)
        {
            // 查找注册的模板路径
            if (!mTemplateRegistry.TryGetValue(type, out var templatePath))
            {
                Debug.LogWarning($"[LLMDescriptionService] No template registered for type: {type}");
                return null;
            }

            // 检查模板缓存
            if (mTemplateCache.TryGetValue(templatePath, out var cached))
            {
                return cached;
            }

            // 构建完整文件路径 (Assets/Prompts/Descriptions/xxx.md)
            string fullPath = Path.Combine(Application.dataPath, templatePath);

            if (!File.Exists(fullPath))
            {
                Debug.LogWarning($"[LLMDescriptionService] Template file not found: {fullPath}");
                return null;
            }

            try
            {
                var content = File.ReadAllText(fullPath);
                mTemplateCache[templatePath] = content;

                // 异步让出一帧，避免阻塞
                await UniTask.Yield();

                return content;
            }
            catch (Exception e)
            {
                Debug.LogError($"[LLMDescriptionService] Failed to load template: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 调用 LLM 生成描述
        /// </summary>
        /// <param name="prompt">完整提示词</param>
        /// <returns>LLM 生成的描述</returns>
        private async UniTask<string> CallLLMAsync(string prompt)
        {
            var client = LLMService.Client;
            if (client == null)
            {
                Debug.LogError("[LLMDescriptionService] LLMService.Client is null");
                return null;
            }

            var request = new LLMRequest()
                .AddSystem("你是游戏中的诗意叙述者。用户会给你物品信息，你只需输出一句简短的描述文字（30-50字），不要解释、不要列表、不要重复用户的话。")
                .AddUser(prompt);

            request.Temperature = 0.7f;
            request.MaxTokens = 100;

            var response = await client.SendAsync(request);

            if (response == null || !response.Success)
            {
                Debug.LogWarning($"[LLMDescriptionService] LLM request failed: {response?.ErrorMessage}");
                return null;
            }

            return response.Content?.Trim();
        }

        /// <summary>
        /// 缓存描述结果
        /// </summary>
        private void CacheDescription(string key, string description)
        {
            // 简单的 LRU 策略：超过最大数量时清除一半
            if (mDescriptionCache.Count >= MAX_CACHE_SIZE)
            {
                var keysToRemove = new List<string>();
                int count = 0;
                foreach (var k in mDescriptionCache.Keys)
                {
                    if (count++ < MAX_CACHE_SIZE / 2)
                    {
                        keysToRemove.Add(k);
                    }
                    else
                    {
                        break;
                    }
                }

                foreach (var k in keysToRemove)
                {
                    mDescriptionCache.Remove(k);
                }
            }

            mDescriptionCache[key] = description;
        }

        #endregion
    }
}
