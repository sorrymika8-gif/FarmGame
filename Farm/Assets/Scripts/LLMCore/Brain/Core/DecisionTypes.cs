namespace FarmGame.LLMCore.Brain
{
    /// <summary>
    /// 决策类型常量定义
    /// </summary>
    public static class DecisionTypes
    {
        /// <summary>统一决策 - 推荐使用，支持所有场景</summary>
        public const string Unified = "Unified";

        /// <summary>行为决策 - 通用的行为选择 [已废弃，请使用 Unified]</summary>
        public const string Behavior = "Behavior";

        /// <summary>对话决策 - 与玩家或NPC对话 [已废弃，请使用 Unified]</summary>
        public const string Dialogue = "Dialogue";

        /// <summary>聊天决策 - 响应玩家聊天消息 [已废弃，请使用 Unified]</summary>
        public const string Chat = "Chat";

        /// <summary>战斗决策 - 战斗中的行动选择 [已废弃，请使用 Unified]</summary>
        public const string Combat = "Combat";

        /// <summary>交易决策 - 买卖物品 [已废弃，请使用 Unified]</summary>
        public const string Trade = "Trade";
    }
}
