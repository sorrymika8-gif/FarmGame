using System.Collections.Generic;
using FarmGame.LLMCore.Brain;
using FarmGame.LLMCore.Memory;

namespace FarmGame.LLMCore.Brain.Prompts
{
    /// <summary>
    /// 记忆整理提示词上下文
    /// </summary>
    public class OrganizeMemoryContext
    {
        public CharacterSetting Character;
        public List<PartitionConfig> Configs;
        public MemoryStore Store;
        public string TriggerPartitionName;

        public OrganizeMemoryContext(CharacterSetting character, List<PartitionConfig> configs, MemoryStore store, string triggerPartitionName)
        {
            Character = character;
            Configs = configs;
            Store = store;
            TriggerPartitionName = triggerPartitionName;
        }
    }
}
