using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace FarmGame.GameLLM
{
    /// <summary>
    /// LLM 厂商 Provider 接口
    /// </summary>
    public interface ILLMProvider
    {
        string ProviderName { get; }
        
        /// <summary>
        /// 发送请求
        /// </summary>
        UniTask<LLMResponse> SendAsync(LLMRequest request, CancellationToken ct = default);
        
        /// <summary>
        /// 流式请求
        /// </summary>
        IAsyncEnumerable<string> SendStreamAsync(LLMRequest request, CancellationToken ct = default);
    }
}
