using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using FarmGame.LLMCore.Memory;

namespace FarmGame.LLMCore.Brain
{
    /// <summary>
    /// 统一决策提示词构建器
    /// 用于所有类型的决策场景（对话、行为、战斗等）
    /// 通过 TriggerEvent 区分不同的触发情境
    /// </summary>
    public class UnifiedPromptBuilder : IPromptBuilder
    {
        public string DecisionType => DecisionTypes.Unified;

        // 模板文件路径 (相对于 Assets 目录)
        private const string TEMPLATE_REL_PATH = "Prompts/UnifiedPromptTemplate.md";

        // 记忆条数限制
        private const int MAX_SHORT_TERM = 20;
        private const int MAX_LONG_TERM = 10;
        private const int MAX_PERMANENT = 10;

        public string Build(DecisionContext context)
        {
            // 加载模板
            string fullPath = Path.Combine(Application.dataPath, TEMPLATE_REL_PATH);
            if (!File.Exists(fullPath))
            {
                Debug.LogError($"[UnifiedPromptBuilder] 找不到提示词模板: {fullPath}");
                return string.Empty;
            }

            string template = File.ReadAllText(fullPath);

            // 1. 构建角色设定字符串
            string characterProfile = BuildDictionaryString(context.CharacterProfile, "无特定设定");

            // 2. 构建当前状态字符串
            string currentState = BuildObjectDictionaryString(context.CurrentState, "无状态信息");

            // 3. 构建环境感知字符串
            string perception = BuildObjectDictionaryString(context.Perception, "无特别感知");

            // 4. 构建各层记忆字符串
            string shortTermMemories = BuildMemoryString(context.MemoryStore, "short_term", MAX_SHORT_TERM, "无最近经历");
            string longTermMemories = BuildMemoryString(context.MemoryStore, "long_term", MAX_LONG_TERM, "无重要记忆");
            string permanentMemories = BuildMemoryString(context.MemoryStore, "permanent", MAX_PERMANENT, "无刻骨铭心的记忆");

            // 5. 获取触发事件
            string triggerEvent = string.IsNullOrEmpty(context.TriggerEvent) ? "无特定触发事件" : context.TriggerEvent;

            // 6. 获取可用行为（可从 Extra 中指定特定行为列表，否则获取全部）
            string availableActions = GetAvailableActions(context);

            // 7. 获取表情列表（用于替换 SetExpression 中的占位符）
            string expressionList = ExpressionHintLoader.GetAllExpressionHints();

            // 替换所有占位符
            return template
                .Replace("{{CHARACTER_PROFILE}}", characterProfile)
                .Replace("{{CURRENT_STATE}}", currentState)
                .Replace("{{PERCEPTION}}", perception)
                .Replace("{{SHORT_TERM_MEMORIES}}", shortTermMemories)
                .Replace("{{LONG_TERM_MEMORIES}}", longTermMemories)
                .Replace("{{PERMANENT_MEMORIES}}", permanentMemories)
                .Replace("{{TRIGGER_EVENT}}", triggerEvent)
                .Replace("{{AVAILABLE_ACTIONS}}", availableActions)
                .Replace("{{EXPRESSION_LIST}}", expressionList);
        }

        /// <summary>
        /// 获取可用行为提示词
        /// 如果 context.Extra 中指定了 "AvailableActions"，则只加载指定的行为
        /// 否则加载所有行为
        /// </summary>
        private string GetAvailableActions(DecisionContext context)
        {
            // 检查是否在 Extra 中指定了可用行为列表
            if (context.Extra != null && 
                context.Extra.TryGetValue("AvailableActions", out var actionsObj) &&
                actionsObj is IEnumerable<string> actionTypes)
            {
                return ActionHintLoader.GetActionHints(actionTypes);
            }

            // 默认返回所有行为
            return ActionHintLoader.GetAllActionHints();
        }

        /// <summary>
        /// 构建字符串字典的显示字符串
        /// </summary>
        private string BuildDictionaryString(Dictionary<string, string> dict, string emptyMessage)
        {
            if (dict == null || dict.Count == 0)
            {
                return $"- {emptyMessage}";
            }

            var sb = new StringBuilder();
            foreach (var kv in dict)
            {
                sb.AppendLine($"- {kv.Key}: {kv.Value}");
            }
            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// 构建对象字典的显示字符串
        /// </summary>
        private string BuildObjectDictionaryString(Dictionary<string, object> dict, string emptyMessage)
        {
            if (dict == null || dict.Count == 0)
            {
                return $"- {emptyMessage}";
            }

            var sb = new StringBuilder();
            foreach (var kv in dict)
            {
                sb.AppendLine($"- {kv.Key}: {kv.Value}");
            }
            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// 从指定分区构建记忆字符串
        /// </summary>
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
