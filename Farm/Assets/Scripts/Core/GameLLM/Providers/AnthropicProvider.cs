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
    /// Anthropic Claude Provider
    /// </summary>
    public class AnthropicProvider : ILLMProvider
    {
        public string ProviderName => "Anthropic";

        private readonly string _baseUrl;
        private readonly string _apiKey;
        private readonly string _defaultModel;
        private const string API_VERSION = "2023-06-01";

        public AnthropicProvider(string apiKey, string baseUrl = "https://api.anthropic.com/v1", string defaultModel = "claude-3-opus-20240229")
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

                using var webRequest = new UnityWebRequest($"{_baseUrl}/messages", "POST");
                webRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("x-api-key", _apiKey);
                webRequest.SetRequestHeader("anthropic-version", API_VERSION);

                await webRequest.SendWebRequest().WithCancellation(ct);

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    return LLMResponse.Fail($"HTTP {webRequest.responseCode}: {webRequest.error}");
                }

                var responseJson = JObject.Parse(webRequest.downloadHandler.text);
                
                // Anthropic 的响应格式：content 是数组
                var contentArray = responseJson["content"] as JArray;
                var textContent = "";
                if (contentArray != null)
                {
                    foreach (var block in contentArray)
                    {
                        if (block["type"]?.ToString() == "text")
                        {
                            textContent += block["text"]?.ToString();
                        }
                    }
                }

                var response = LLMResponse.Ok(textContent);
                response.Latency = Time.realtimeSinceStartup - startTime;
                response.PromptTokens = responseJson["usage"]?["input_tokens"]?.Value<int>() ?? 0;
                response.CompletionTokens = responseJson["usage"]?["output_tokens"]?.Value<int>() ?? 0;
                
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

            using var webRequest = new UnityWebRequest($"{_baseUrl}/messages", "POST");
            webRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("x-api-key", _apiKey);
            webRequest.SetRequestHeader("anthropic-version", API_VERSION);

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

                    foreach (var token in ParseAnthropicSSE(newData))
                    {
                        yield return token;
                    }
                }
                
                await UniTask.Yield();
            }

            var finalText = webRequest.downloadHandler?.text ?? "";
            if (finalText.Length > lastPosition)
            {
                foreach (var token in ParseAnthropicSSE(finalText.Substring(lastPosition)))
                {
                    yield return token;
                }
            }
        }

        private object BuildRequestBody(LLMRequest request, bool stream)
        {
            string systemPrompt = null;
            var messages = new List<object>();
            
            foreach (var msg in request.Messages)
            {
                if (msg.Role == MessageRole.System)
                {
                    systemPrompt = msg.Content;
                }
                else
                {
                    messages.Add(new
                    {
                        role = msg.Role == MessageRole.User ? "user" : "assistant",
                        content = msg.Content
                    });
                }
            }

            var body = new Dictionary<string, object>
            {
                ["model"] = request.Model ?? _defaultModel,
                ["messages"] = messages,
                ["max_tokens"] = request.MaxTokens,
                ["stream"] = stream
            };

            if (!string.IsNullOrEmpty(systemPrompt))
            {
                body["system"] = systemPrompt;
            }

            if (request.Temperature > 0)
            {
                body["temperature"] = request.Temperature;
            }

            return body;
        }

        private IEnumerable<string> ParseAnthropicSSE(string data)
        {
            var lines = data.Split('\n');
            foreach (var line in lines)
            {
                if (!line.StartsWith("data:")) continue;
                
                var jsonStr = line.Substring(5).Trim();
                if (string.IsNullOrEmpty(jsonStr)) continue;

                string content = null;
                try
                {
                    var eventData = JObject.Parse(jsonStr);
                    var eventType = eventData["type"]?.ToString();
                    
                    if (eventType == "content_block_delta")
                    {
                        content = eventData["delta"]?["text"]?.ToString();
                    }
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
