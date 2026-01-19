using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace FarmGame.GameLLM
{
    /// <summary>
    /// OpenAI / OpenAI 兼容接口 Provider
    /// 兼容: OpenAI、Azure OpenAI、本地 Ollama、vLLM 等
    /// </summary>
    public class OpenAIProvider : ILLMProvider
    {
        public string ProviderName => "OpenAI";

        private readonly string _baseUrl;
        private readonly string _apiKey;
        private readonly string _defaultModel;

        public OpenAIProvider(string apiKey, string baseUrl = "https://api.openai.com/v1", string defaultModel = "gpt-4o-mini")
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

                using var webRequest = new UnityWebRequest($"{_baseUrl}/chat/completions", "POST");
                webRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Authorization", $"Bearer {_apiKey}");

                await webRequest.SendWebRequest().WithCancellation(ct);

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    return LLMResponse.Fail($"HTTP {webRequest.responseCode}: {webRequest.error}");
                }

                var responseJson = JObject.Parse(webRequest.downloadHandler.text);
                var content = responseJson["choices"]?[0]?["message"]?["content"]?.ToString();
                
                var response = LLMResponse.Ok(content);
                response.Latency = Time.realtimeSinceStartup - startTime;
                response.PromptTokens = responseJson["usage"]?["prompt_tokens"]?.Value<int>() ?? 0;
                response.CompletionTokens = responseJson["usage"]?["completion_tokens"]?.Value<int>() ?? 0;
                
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

            // 处理剩余数据
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
                    // 忽略解析错误
                }

                if (!string.IsNullOrEmpty(content))
                {
                    yield return content;
                }
            }
        }
    }
}
