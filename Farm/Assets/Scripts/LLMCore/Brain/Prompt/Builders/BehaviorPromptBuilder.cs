using System.IO;
using System.Text;
using UnityEngine;
using FarmGame.LLMCore.Memory;

namespace FarmGame.LLMCore.Brain
{
    /// <summary>
    /// 行为决策提示词构建器
    /// 将 DecisionContext 格式化为 LLM 能理解的提示词
    /// </summary>
    public class BehaviorPromptBuilder : IPromptBuilder
    {
        public string DecisionType => DecisionTypes.Behavior;

        // 模板文件路径 (相对于 Assets 目录)
        private const string TEMPLATE_REL_PATH = "Prompts/BehaviorPromptTemplate.md";

        public string Build(DecisionContext context)
        {
            // 加载模板 (使用 IO 直接读取，方便修改)
            string fullPath = Path.Combine(Application.dataPath, TEMPLATE_REL_PATH);
            if (!File.Exists(fullPath))
            {
                Debug.LogError($"[BehaviorPromptBuilder] 找不到提示词模板: {fullPath}");
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

            // 3. 构建感知字符串
            var sbPerception = new StringBuilder();
            if (context.Perception != null && context.Perception.Count > 0)
            {
                foreach (var kv in context.Perception)
                {
                    sbPerception.AppendLine($"- {kv.Key}: {kv.Value}");
                }
            }
            else
            {
                sbPerception.AppendLine("- 无感知信息");
            }

            // 4. 构建记忆字符串
            var sbMemories = new StringBuilder();
            if (context.MemoryStore != null)
            {
                var memories = context.MemoryStore.GetAllMemories();
                if (memories.Count > 0)
                {
                    foreach (var memory in memories)
                    {
                        sbMemories.AppendLine($"- {memory.Content}");
                    }
                }
                else
                {
                    sbMemories.AppendLine("- 无相关记忆");
                }
            }
            else
            {
                sbMemories.AppendLine("- 无记忆存储");
            }

            // 5. 构建触发事件字符串
            string triggerEvent = string.IsNullOrEmpty(context.TriggerEvent) ? "- 无特定触发" : $"- {context.TriggerEvent}";

            // 替换所有占位符
            return template
                .Replace("{{CHARACTER_PROFILE}}", sbProfile.ToString().TrimEnd())
                .Replace("{{CURRENT_STATE}}", sbState.ToString().TrimEnd())
                .Replace("{{PERCEPTION}}", sbPerception.ToString().TrimEnd())
                .Replace("{{MEMORIES}}", sbMemories.ToString().TrimEnd())
                .Replace("{{TRIGGER_EVENT}}", triggerEvent);
        }
    }
}
