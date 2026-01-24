using System;

namespace FarmGame.LLMCore.Memory
{
    /// <summary>
    /// 一条记忆
    /// 本质就是一段文本，没有额外的元数据
    /// 时间、归属者等信息如果重要，由使用者包含在文本内容中
    /// </summary>
    [Serializable]
    public class Memory
    {
        /// <summary>
        /// 记忆内容
        /// </summary>
        public string Content { get; }

        public Memory(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                throw new ArgumentNullException(nameof(content), "记忆内容不能为空");
            }
            Content = content;
        }

        public override string ToString() => Content;

        public override bool Equals(object obj)
        {
            return obj is Memory other && Content == other.Content;
        }

        public override int GetHashCode() => Content.GetHashCode();
    }
}
