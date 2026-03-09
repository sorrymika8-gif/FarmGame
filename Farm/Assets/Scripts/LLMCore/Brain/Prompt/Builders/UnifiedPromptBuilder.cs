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
    /// 采用模块化设计：通用模块 + NPC专属人设
    /// </summary>
    public class UnifiedPromptBuilder : IPromptBuilder
    {
        public string DecisionType => DecisionTypes.Unified;

        // 目录路径 (相对于 Assets 目录)
        private const string NPC_PROMPTS_DIR = "Prompts/Npcs";
        private const string COMMON_PROMPTS_DIR = "Prompts/Common";

        // 记忆条数限制
        private const int MAX_SHORT_TERM = 20;
        private const int MAX_LONG_TERM = 10;
        private const int MAX_PERMANENT = 10;

        // 通用模块缓存
        private static Dictionary<string, string> sCommonModuleCache;

        /// <summary>
        /// 初始化通用模块缓存（可在游戏启动时调用）
        /// </summary>
        public static void InitializeCache()
        {
            sCommonModuleCache = new Dictionary<string, string>();
            string commonDir = Path.Combine(Application.dataPath, COMMON_PROMPTS_DIR);

            string[] moduleFiles = { "BaseIdentity.md", "StatePerception.md", "MemorySystem.md", "DecisionRules.md", "OutputFormat.md" };
            foreach (var fileName in moduleFiles)
            {
                string filePath = Path.Combine(commonDir, fileName);
                if (File.Exists(filePath))
                {
                    sCommonModuleCache[fileName] = File.ReadAllText(filePath);
                    Debug.Log($"[UnifiedPromptBuilder] 已缓存通用模块: {fileName}");
                }
                else
                {
                    Debug.LogWarning($"[UnifiedPromptBuilder] 通用模块文件不存在: {filePath}");
                }
            }
        }

        /// <summary>
        /// 加载通用模块内容
        /// </summary>
        private string LoadCommonModule(string fileName)
        {
            // 优先从缓存读取
            if (sCommonModuleCache != null && sCommonModuleCache.TryGetValue(fileName, out var cached))
            {
                return cached;
            }

            // 缓存未初始化则直接读文件
            string filePath = Path.Combine(Application.dataPath, COMMON_PROMPTS_DIR, fileName);
            if (File.Exists(filePath))
            {
                return File.ReadAllText(filePath);
            }

            Debug.LogWarning($"[UnifiedPromptBuilder] 通用模块文件不存在: {filePath}");
            return string.Empty;
        }

        /// <summary>
        /// 加载NPC专属人设文件
        /// </summary>
        private string LoadNpcPrompt(string promptFileName)
        {
            if (string.IsNullOrEmpty(promptFileName))
            {
                Debug.LogError($"[UnifiedPromptBuilder] NPC未配置专属提示词文件！请在npc.xlsx的prompt字段中填写提示词文件名（如 villager.md）");
                return string.Empty;
            }

            string fullPath = Path.Combine(Application.dataPath, NPC_PROMPTS_DIR, promptFileName);
            if (!File.Exists(fullPath))
            {
                Debug.LogError($"[UnifiedPromptBuilder] 找不到NPC专属提示词文件: {fullPath}");
                return string.Empty;
            }

            Debug.Log($"[UnifiedPromptBuilder] 已加载NPC专属人设: {promptFileName}");
            return File.ReadAllText(fullPath);
        }

        public string Build(DecisionContext context)
        {
            // 从上下文获取NPC专属提示词文件名
            string promptFileName = null;
            if (context.Extra != null && context.Extra.TryGetValue("PromptFilePath", out var pathObj))
            {
                promptFileName = pathObj as string;
            }

            var sb = new StringBuilder();

            // ===== 按顺序组装模块 =====

            // 1. 基础身份说明
            sb.AppendLine(LoadCommonModule("BaseIdentity.md"));
            sb.AppendLine();

            // 2. NPC专属人设（性格、说话风格、特殊要求）
            string npcPrompt = LoadNpcPrompt(promptFileName);
            if (string.IsNullOrEmpty(npcPrompt))
            {
                return string.Empty; // NPC专属文件是必须的
            }
            sb.AppendLine(npcPrompt);
            sb.AppendLine();

            // 3. 状态和感知
            sb.AppendLine(LoadCommonModule("StatePerception.md"));
            sb.AppendLine();

            // 4. 记忆系统
            sb.AppendLine(LoadCommonModule("MemorySystem.md"));
            sb.AppendLine();

            // 5. 可用行为和表情
            sb.AppendLine("## 可用行为");
            sb.AppendLine(GetAvailableActions(context));
            sb.AppendLine();
            sb.AppendLine("## 可用表情");
            sb.AppendLine(ExpressionHintLoader.GetAllExpressionHints());
            sb.AppendLine();

            // 6. 通用决策规则
            sb.AppendLine(LoadCommonModule("DecisionRules.md"));
            sb.AppendLine();

            // 7. 输出格式
            sb.AppendLine(LoadCommonModule("OutputFormat.md"));

            // ===== 替换所有占位符 =====
            string finalPrompt = sb.ToString();
            return ReplacePlaceholders(finalPrompt, context);
        }

        /// <summary>
        /// 替换提示词中的所有占位符
        /// </summary>
        private string ReplacePlaceholders(string template, DecisionContext context)
        {
            // 构建各种数据字符串
            string characterProfile = BuildDictionaryString(context.CharacterProfile, "无特定设定");
            string currentState = BuildObjectDictionaryString(context.CurrentState, "无状态信息");
            string perception = BuildObjectDictionaryString(context.Perception, "无特别感知");
            string shortTermMemories = BuildMemoryString(context.MemoryStore, "short_term", MAX_SHORT_TERM, "无最近经历");
            string longTermMemories = BuildMemoryString(context.MemoryStore, "long_term", MAX_LONG_TERM, "无重要记忆");
            string permanentMemories = BuildMemoryString(context.MemoryStore, "permanent", MAX_PERMANENT, "无刻骨铭心的记忆");
            string triggerEvent = string.IsNullOrEmpty(context.TriggerEvent) ? "无特定触发事件" : context.TriggerEvent;

            return template
                .Replace("{{CHARACTER_PROFILE}}", characterProfile)
                .Replace("{{CURRENT_STATE}}", currentState)
                .Replace("{{PERCEPTION}}", perception)
                .Replace("{{SHORT_TERM_MEMORIES}}", shortTermMemories)
                .Replace("{{LONG_TERM_MEMORIES}}", longTermMemories)
                .Replace("{{PERMANENT_MEMORIES}}", permanentMemories)
                .Replace("{{TRIGGER_EVENT}}", triggerEvent);
        }

        /// <summary>
        /// 获取可用行为提示词
        /// </summary>
        private string GetAvailableActions(DecisionContext context)
        {
            if (context.Extra != null && 
                context.Extra.TryGetValue("AvailableActions", out var actionsObj) &&
                actionsObj is IEnumerable<string> actionTypes)
            {
                return ActionHintLoader.GetActionHints(actionTypes);
            }
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

            int startIdx = memories.Count > maxCount ? memories.Count - maxCount : 0;
            for (int i = startIdx; i < memories.Count; i++)
            {
                sb.AppendLine($"- {memories[i].Content}");
            }

            return sb.ToString().TrimEnd();
        }
    }
}
