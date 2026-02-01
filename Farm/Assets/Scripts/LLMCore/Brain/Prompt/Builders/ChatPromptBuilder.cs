using System.IO;
using System.Text;
using UnityEngine;
using FarmGame.LLMCore.Memory;

namespace FarmGame.LLMCore.Brain
{
    /// <summary>
    /// 聊天决策提示词构建器
    /// 用于处理玩家与NPC的聊天对话
    /// </summary>
    public class ChatPromptBuilder : IPromptBuilder
    {
        public string DecisionType => DecisionTypes.Chat;

        // 模板文件路径 (相对于 Assets 目录)
        private const string TEMPLATE_REL_PATH = "Prompts/ChatPromptTemplate.md";

        // 记忆条数限制
        private const int MAX_SHORT_TERM = 20;  // 短期记忆包含对话和行为
        private const int MAX_LONG_TERM = 10;
        private const int MAX_PERMANENT = 10;

        public string Build(DecisionContext context)
        {
            // 加载模板 (使用 IO 直接读取，方便修改)
            string fullPath = Path.Combine(Application.dataPath, TEMPLATE_REL_PATH);
            if (!File.Exists(fullPath))
            {
                Debug.LogError($"[ChatPromptBuilder] 找不到提示词模板: {fullPath}");
                return string.Empty;
            }
            
            string template = File.ReadAllText(fullPath);

            // 1. 构建角色设定字符串
            var sbProfile = new StringBuilder();
            if (context.CharacterProfile != null && context.CharacterProfile.Count > 0)
            {
                foreach (var kv in context.CharacterProfile)
                {
                    sbProfile.AppendLine($"- {kv.Key}: {kv.Value}");
                }
            }
            else
            {
                sbProfile.AppendLine("- 无特定设定");
            }

            // 2. 构建当前状态字符串
            var sbState = new StringBuilder();
            if (context.CurrentState != null && context.CurrentState.Count > 0)
            {
                foreach (var kv in context.CurrentState)
                {
                    sbState.AppendLine($"- {kv.Key}: {kv.Value}");
                }
            }
            else
            {
                sbState.AppendLine("- 无状态信息");
            }

            // 3. 构建各层记忆字符串（统一的记忆系统）
            string shortTermMemories = BuildMemoryString(context.MemoryStore, "short_term", MAX_SHORT_TERM, "无最近经历");
            string longTermMemories = BuildMemoryString(context.MemoryStore, "long_term", MAX_LONG_TERM, "无重要记忆");
            string permanentMemories = BuildMemoryString(context.MemoryStore, "permanent", MAX_PERMANENT, "无刻骨铭心的记忆");

            // 替换所有占位符
            return template
                .Replace("{{CHARACTER_PROFILE}}", sbProfile.ToString().TrimEnd())
                .Replace("{{CURRENT_STATE}}", sbState.ToString().TrimEnd())
                .Replace("{{SHORT_TERM_MEMORIES}}", shortTermMemories)
                .Replace("{{LONG_TERM_MEMORIES}}", longTermMemories)
                .Replace("{{PERMANENT_MEMORIES}}", permanentMemories);
        }

        /// <summary>
        /// 从指定分区构建记忆字符串
        /// </summary>
        /// <param name="store">记忆存储</param>
        /// <param name="partitionName">分区名称</param>
        /// <param name="maxCount">最大条数（取最近的）</param>
        /// <param name="emptyMessage">为空时的默认消息</param>
        private string BuildMemoryString(MemoryStore store, string partitionName, int maxCount, string emptyMessage)
        {
            if (store == null)
            {
                return $"- {emptyMessage}";
            }

            var partition = store.GetPartition(partitionName);
            if (partition == null || partition.Count == 0)
            {
                return $"- {emptyMessage}";
            }

            var memories = partition.GetAll();
            var sb = new StringBuilder();

            // 取最近的 maxCount 条记忆
            int startIdx = memories.Count > maxCount ? memories.Count - maxCount : 0;
            for (int i = startIdx; i < memories.Count; i++)
            {
                sb.AppendLine($"- {memories[i].Content}");
            }

            return sb.ToString().TrimEnd();
        }
    }
}
