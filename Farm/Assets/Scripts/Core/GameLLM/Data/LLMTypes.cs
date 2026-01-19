using System;
using System.Collections.Generic;

namespace FarmGame.GameLLM
{
    /// <summary>
    /// 消息角色
    /// </summary>
    public enum MessageRole
    {
        System,
        User,
        Assistant
    }

    /// <summary>
    /// 单条消息
    /// </summary>
    [Serializable]
    public class LLMMessage
    {
        public MessageRole Role;
        public string Content;

        public LLMMessage() { }
        
        public LLMMessage(MessageRole role, string content)
        {
            Role = role;
            Content = content;
        }

        public static LLMMessage System(string content) => new(MessageRole.System, content);
        public static LLMMessage User(string content) => new(MessageRole.User, content);
        public static LLMMessage Assistant(string content) => new(MessageRole.Assistant, content);
    }

    /// <summary>
    /// 请求参数
    /// </summary>
    [Serializable]
    public class LLMRequest
    {
        public List<LLMMessage> Messages = new();
        public float Temperature = 0.7f;
        public int MaxTokens = 1024;
        public string Model;  // 可选，不填则用 Provider 默认值

        public LLMRequest AddMessage(LLMMessage message)
        {
            Messages.Add(message);
            return this;
        }
        
        public LLMRequest AddSystem(string content) => AddMessage(LLMMessage.System(content));
        public LLMRequest AddUser(string content) => AddMessage(LLMMessage.User(content));
        // TODO: 未来需要实现 LLM 记忆管理模块 (ContextManager)，用于自动维护 User/Assistant 的历史对话窗口
        public LLMRequest AddAssistant(string content) => AddMessage(LLMMessage.Assistant(content));
    }

    /// <summary>
    /// 响应结果
    /// </summary>
    [Serializable]
    public class LLMResponse
    {
        public bool Success;
        public string Content;
        public string ErrorMessage;
        
        // 元信息
        public int PromptTokens;
        public int CompletionTokens;
        public float Latency;  // 秒

        public static LLMResponse Ok(string content) => new() { Success = true, Content = content };
        public static LLMResponse Fail(string error) => new() { Success = false, ErrorMessage = error };
    }
}
