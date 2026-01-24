using System.Text;
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

        public string Build(DecisionContext context)
        {
            var sb = new StringBuilder();

            // 系统指令
            sb.AppendLine("你是一个游戏中的AI角色。根据以下信息做出行为决策。");
            sb.AppendLine("你必须以JSON格式返回一个指令列表。");
            sb.AppendLine();

            // 角色设定
            sb.AppendLine("## 角色设定");
            if (context.CharacterProfile != null && context.CharacterProfile.Count > 0)
            {
                foreach (var kv in context.CharacterProfile)
                {
                    sb.AppendLine($"- {kv.Key}: {kv.Value}");
                }
            }
            else
            {
                sb.AppendLine("- 无特定设定");
            }
            sb.AppendLine();

            // 当前状态
            sb.AppendLine("## 当前状态");
            if (context.CurrentState != null && context.CurrentState.Count > 0)
            {
                foreach (var kv in context.CurrentState)
                {
                    sb.AppendLine($"- {kv.Key}: {kv.Value}");
                }
            }
            else
            {
                sb.AppendLine("- 无状态信息");
            }
            sb.AppendLine();

            // 环境感知
            sb.AppendLine("## 环境感知");
            if (context.Perception != null && context.Perception.Count > 0)
            {
                foreach (var kv in context.Perception)
                {
                    sb.AppendLine($"- {kv.Key}: {kv.Value}");
                }
            }
            else
            {
                sb.AppendLine("- 无感知信息");
            }
            sb.AppendLine();

            // 记忆
            sb.AppendLine("## 相关记忆");
            if (context.MemoryStore != null)
            {
                var memories = context.MemoryStore.GetAllMemories();
                if (memories.Count > 0)
                {
                    foreach (var memory in memories)
                    {
                        sb.AppendLine($"- {memory.Content}");
                    }
                }
                else
                {
                    sb.AppendLine("- 无相关记忆");
                }
            }
            else
            {
                sb.AppendLine("- 无记忆存储");
            }
            sb.AppendLine();

            // 触发事件
            sb.AppendLine("## 触发事件");
            sb.AppendLine(string.IsNullOrEmpty(context.TriggerEvent) ? "- 无特定触发" : $"- {context.TriggerEvent}");
            sb.AppendLine();

            // 输出格式要求
            sb.AppendLine("## 输出格式要求");
            sb.AppendLine("请以JSON数组格式返回你的决策，每个元素是一个指令对象。");
            sb.AppendLine("可用的指令类型:");
            sb.AppendLine("1. Move: 移动到某个位置");
            sb.AppendLine("   {\"type\": \"Move\", \"x\": 数字, \"y\": 数字}");
            sb.AppendLine("2. Speak: 说话");
            sb.AppendLine("   {\"type\": \"Speak\", \"content\": \"要说的话\"}");
            sb.AppendLine("3. Attack: 攻击目标");
            sb.AppendLine("   {\"type\": \"Attack\", \"targetId\": \"目标ID\"}");
            sb.AppendLine("4. SetState: 设置自身状态");
            sb.AppendLine("   {\"type\": \"SetState\", \"key\": \"状态名\", \"value\": 值}");
            sb.AppendLine("5. MemoryOperation: 记忆操作");
            sb.AppendLine("   {\"type\": \"MemoryOperation\", \"operation\": \"Add\"|\"Remove\", \"partition\": \"分区名\", \"content\": \"记忆内容\"}");
            sb.AppendLine();
            sb.AppendLine("示例输出:");
            sb.AppendLine("[");
            sb.AppendLine("  {\"type\": \"Speak\", \"content\": \"你好！\"},");
            sb.AppendLine("  {\"type\": \"Move\", \"x\": 10, \"y\": 5}");
            sb.AppendLine("]");
            sb.AppendLine();
            sb.AppendLine("请只返回JSON数组，不要包含其他文字。");

            return sb.ToString();
        }
    }
}
