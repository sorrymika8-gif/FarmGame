using System.Collections.Generic;
using System.Text;
using FarmGame.LLMCore.Memory;
using GameLLM.Prompts;
using UnityEngine;
using FarmGame.LLMCore.Brain;

namespace FarmGame.LLMCore.Brain.Prompts
{
    /// <summary>
    /// 记忆整理提示词构建器
    /// </summary>
    public class OrganizeMemoryPromptBuilder : IPromptBuilder<OrganizeMemoryContext>
    {
        private const string PROMPT_PATH = "GameLLM/Prompts/OrganizeMemory"; // 不需要后缀，Unity Resource.Load 不需要

        public string Build(OrganizeMemoryContext context)
        {
            // 1. 加载模板
            var templateAsset = Resources.Load<TextAsset>(PROMPT_PATH);
            if (templateAsset == null)
            {
                Debug.LogError($"[OrganizeMemoryPromptBuilder] 找不到提示词模板: {PROMPT_PATH}");
                return string.Empty;
            }
            string template = templateAsset.text;

            // 2. 准备数据
            string charSettingStr = context.Character.ToPrompt();
            string partitionStructureStr = BuildPartitionStructure(context.Configs);
            string currentMemoriesStr = BuildCurrentMemories(context.Configs, context.Store);

            // 3. 替换变量
            return template
                .Replace("{CharacterSetting}", charSettingStr)
                .Replace("{PartitionStructure}", partitionStructureStr)
                .Replace("{CurrentMemories}", currentMemoriesStr)
                .Replace("{TriggerPartition}", context.TriggerPartitionName);
        }

        private string BuildPartitionStructure(List<PartitionConfig> configs)
        {
            var sb = new StringBuilder();
            foreach (var config in configs)
            {
                sb.AppendLine($"### {config.Name}");
                sb.AppendLine(config.Description);
                sb.AppendLine();
            }
            return sb.ToString().TrimEnd();
        }

        private string BuildCurrentMemories(List<PartitionConfig> configs, MemoryStore store)
        {
            var sb = new StringBuilder();
            foreach (var config in configs)
            {
                var partition = store.GetPartition(config.Name);
                if (partition != null && !partition.IsEmpty)
                {
                    sb.AppendLine($"### {config.Name}");
                    var memories = partition.GetAll();
                    for (int i = 0; i < memories.Count; i++)
                    {
                        sb.AppendLine($"[{i}] {memories[i].Content}");
                    }
                    sb.AppendLine();
                }
            }
            return sb.ToString().TrimEnd();
        }
    }
}
