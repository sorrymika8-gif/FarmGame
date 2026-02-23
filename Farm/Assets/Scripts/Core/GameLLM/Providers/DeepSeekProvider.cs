using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace FarmGame.GameLLM
{
    /// <summary>
    /// DeepSeek 专用 Provider
    /// 虽然目前兼容 OpenAI 协议，但独立成类以便未来扩展 DeepSeek 特有功能（如推理参数等）
    /// </summary>
    public class DeepSeekProvider : ILLMProvider
    {
        public string ProviderName => "DeepSeek";

        private readonly string _baseUrl;
        private readonly string _apiKey;
        private readonly string _defaultModel;

        public DeepSeekProvider(string apiKey, string baseUrl = "https://api.deepseek.com", string defaultModel = "deepseek-chat")
        {
            _apiKey = apiKey;
            _baseUrl = baseUrl.TrimEnd('/');
            _defaultModel = defaultModel;
        }

        public async UniTask<LLMResponse> SendAsync(LLMRequest request, CancellationToken ct = default)
        {
            var startTime = Time.realtimeSinceStartup;
            
            try
            {
                var body = BuildRequestBody(request, stream: false);
                var json = JsonConvert.SerializeObject(body);

                // DeepSeek 端点与 OpenAI 保持一致
                using var webRequest = new UnityWebRequest($"{_baseUrl}/chat/completions", "POST");
                webRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Authorization", $"Bearer {_apiKey}");

                await webRequest.SendWebRequest().WithCancellation(ct);

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    return LLMResponse.Fail($"HTTP {webRequest.responseCode}: {webRequest.error}\nContext: {webRequest.downloadHandler.text}");
                }

                var rawResponse = webRequest.downloadHandler.text;
                Debug.Log($"[DeepSeekProvider] Raw response: {rawResponse}");
                
                var responseJson = JObject.Parse(rawResponse);
                
                // DeepSeek-R1 (reasoner) 模型可能返回 reasoning_content 而非 content
                // 优先获取 content，如果为空则尝试 reasoning_content
                var message = responseJson["choices"]?[0]?["message"];
                var content = message?["content"]?.ToString();
                
                // 如果 content 为空，尝试获取 reasoning_content（R1 模型特有）
                if (string.IsNullOrEmpty(content))
                {
                    content = message?["reasoning_content"]?.ToString();
                    Debug.LogWarning($"[DeepSeekProvider] content 为空，使用 reasoning_content");
                }

                // 过滤掉 DeepSeek-R1 的思考过程标签 <think>...</think>
                content = RemoveThinkingTags(content);
                
                if (string.IsNullOrEmpty(content))
                {
                    Debug.LogError($"[DeepSeekProvider] 无法从响应中提取内容");
                    return LLMResponse.Fail("Response content is empty");
                }
                
                var response = LLMResponse.Ok(content);
                response.Latency = Time.realtimeSinceStartup - startTime;
                response.PromptTokens = responseJson["usage"]?["prompt_tokens"]?.Value<int>() ?? 0;
                response.CompletionTokens = responseJson["usage"]?["completion_tokens"]?.Value<int>() ?? 0;
                
                // DeepSeek 特有字段：缓存命中统计 (如有)
                // var cacheHit = responseJson["usage"]?["prompt_cache_hit_tokens"]?.Value<int>() ?? 0;
                
                return response;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return LLMResponse.Fail(ex.Message);
            }
        }

        public async IAsyncEnumerable<string> SendStreamAsync(
            LLMRequest request,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            var body = BuildRequestBody(request, stream: true);
            var json = JsonConvert.SerializeObject(body);

            using var webRequest = new UnityWebRequest($"{_baseUrl}/chat/completions", "POST");
            webRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Authorization", $"Bearer {_apiKey}");

            var operation = webRequest.SendWebRequest();
            int lastPosition = 0;

            while (!operation.isDone)
            {
                ct.ThrowIfCancellationRequested();
                
                var currentText = webRequest.downloadHandler?.text ?? "";
                if (currentText.Length > lastPosition)
                {
                    var newData = currentText.Substring(lastPosition);
                    lastPosition = currentText.Length;

                    foreach (var token in ParseSSE(newData))
                    {
                        yield return token;
                    }
                }
                
                await UniTask.Yield();
            }

            var finalText = webRequest.downloadHandler?.text ?? "";
            if (finalText.Length > lastPosition)
            {
                foreach (var token in ParseSSE(finalText.Substring(lastPosition)))
                {
                    yield return token;
                }
            }
        }

        private object BuildRequestBody(LLMRequest request, bool stream)
        {
            var messages = new List<object>();
            foreach (var msg in request.Messages)
            {
                messages.Add(new
                {
                    role = msg.Role.ToString().ToLower(),
                    content = msg.Content
                });
            }

            return new
            {
                model = request.Model ?? _defaultModel,
                messages,
                // DeepSeek 可能不需要 max_tokens, 或者有特定的默认值
                // 这里保留通用逻辑，未来可在此处添加 deepseek_search 等特有参数
                temperature = request.Temperature,
                max_tokens = request.MaxTokens,
                stream
            };
        }

        private IEnumerable<string> ParseSSE(string data)
        {
            var lines = data.Split('\n');
            foreach (var line in lines)
            {
                if (!line.StartsWith("data:")) continue;
                
                var jsonStr = line.Substring(5).Trim();
                if (jsonStr == "[DONE]") yield break;
                if (string.IsNullOrEmpty(jsonStr)) continue;

                string content = null;
                try
                {
                    var chunk = JObject.Parse(jsonStr);
                    content = chunk["choices"]?[0]?["delta"]?["content"]?.ToString();
                }
                catch
                {
                    // Ignore
                }

                if (!string.IsNullOrEmpty(content))
                {
                    yield return content;
                }
            }
        }

        /// <summary>
        /// 移除 DeepSeek-R1 模型的思考过程标签
        /// 支持 <think>、<thinking>、【思考】等格式
        /// </summary>
        private static string RemoveThinkingTags(string content)
        {
            if (string.IsNullOrEmpty(content)) return content;

            // 移除 <think>...</think> 标签及其内容
            content = Regex.Replace(content, @"<think>[\s\S]*?</think>", "", RegexOptions.IgnoreCase);
            
            // 移除 <thinking>...</thinking> 标签及其内容
            content = Regex.Replace(content, @"<thinking>[\s\S]*?</thinking>", "", RegexOptions.IgnoreCase);
            
            // 移除中文格式的思考标签 【思考】...【/思考】 或 【思考】...【思考结束】
            content = Regex.Replace(content, @"【思考】[\s\S]*?【/?思考(结束)?】", "", RegexOptions.IgnoreCase);
            
            // 移除开头的 "首先" 类解释性文字直到第一个句号（可选，如果仍有问题可启用）
            // content = Regex.Replace(content, @"^首先[^。]*。\s*", "");

            return content.Trim();
        }
    }
}
