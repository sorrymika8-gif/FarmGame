// ============================================================
// 文件: LLMCore/Memory/Memory.cs
// 描述: 一条记忆，本质就是一段文本
// ============================================================

using System;

namespace GameLLM.Memory
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
        /// 记忆内容（通常是LLM生成的总结）
        /// </summary>
        public string Content { get; }

        /// <summary>
        /// 创建一条记忆
        /// </summary>
        /// <param name="content">记忆内容</param>
        /// <exception cref="ArgumentNullException">内容不能为空</exception>
        public Memory(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                throw new ArgumentNullException(nameof(content), "记忆内容不能为空");
            }
            
            Content = content;
        }

        public override string ToString()
        {
            return Content;
        }

        public override bool Equals(object obj)
        {
            if (obj is Memory other)
            {
                return Content == other.Content;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Content.GetHashCode();
        }
    }
}
