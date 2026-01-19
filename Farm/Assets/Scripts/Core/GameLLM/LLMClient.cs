using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace FarmGame.GameLLM
{
    /// <summary>
    /// LLM 客户端 - 统一调用入口
    /// </summary>
    public class LLMClient
    {
        private readonly ILLMProvider _provider;

        public LLMClient(ILLMProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        /// <summary>
        /// 发送请求，获取文本响应
        /// </summary>
        public UniTask<LLMResponse> SendAsync(LLMRequest request, CancellationToken ct = default)
        {
            return _provider.SendAsync(request, ct);
        }

        /// <summary>
        /// 发送请求，获取流式响应
        /// </summary>
        public IAsyncEnumerable<string> SendStreamAsync(LLMRequest request, CancellationToken ct = default)
        {
            return _provider.SendStreamAsync(request, ct);
        }

        /// <summary>
        /// 发送请求，解析为指定类型（自动提取 JSON）
        /// </summary>
        public async UniTask<(bool success, T result, string error)> SendAsync<T>(
            LLMRequest request, 
            CancellationToken ct = default)
        {
            var response = await SendAsync(request, ct);
            
            if (!response.Success)
            {
                return (false, default, response.ErrorMessage);
            }

            try
            {
                var json = ExtractJson(response.Content);
                var result = JsonConvert.DeserializeObject<T>(json);
                return (true, result, null);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[LLMClient] JSON parse failed: {ex.Message}\nRaw: {response.Content}");
                return (false, default, $"JSON parse error: {ex.Message}");
            }
        }

        /// <summary>
        /// 从响应文本中提取 JSON
        /// </summary>
        private string ExtractJson(string text)
        {
            if (string.IsNullOrEmpty(text)) return "{}";
            
            // 尝试提取 ```json ... ``` 代码块
            int start = text.IndexOf("```json", StringComparison.OrdinalIgnoreCase);
            if (start >= 0)
            {
                start = text.IndexOf('\n', start) + 1;
                int end = text.IndexOf("```", start);
                if (end > start)
                {
                    return text.Substring(start, end - start).Trim();
                }
            }

            // 尝试提取 { } 或 [ ]
            int braceStart = text.IndexOf('{');
            int bracketStart = text.IndexOf('[');
            
            start = (braceStart >= 0 && bracketStart >= 0) 
                ? Math.Min(braceStart, bracketStart) 
                : Math.Max(braceStart, bracketStart);

            if (start >= 0)
            {
                char open = text[start];
                char close = open == '{' ? '}' : ']';
                int depth = 0;
                
                for (int i = start; i < text.Length; i++)
                {
                    if (text[i] == open) depth++;
                    if (text[i] == close) depth--;
                    if (depth == 0) return text.Substring(start, i - start + 1);
                }
            }

            return text;
        }
    }
}
