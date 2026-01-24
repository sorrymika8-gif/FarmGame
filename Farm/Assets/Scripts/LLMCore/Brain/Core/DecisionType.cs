namespace FarmGame.LLMCore.Brain
{
    /// <summary>
    /// 决策类型
    /// 使用字符串而非枚举，便于扩展
    /// </summary>
    public static class DecisionType
    {
        /// <summary>行为决策：NPC 决定下一步做什么</summary>
        public const string Behavior = "behavior";

        /// <summary>对话决策：NPC 如何回应玩家</summary>
        public const string Dialogue = "dialogue";

        /// <summary>反应决策：NPC 对突发事件的反应</summary>
        public const string Reaction = "reaction";

        /// <summary>记忆整理：整理记忆分区</summary>
        public const string MemoryOrganization = "memory_organization";
    }
}
