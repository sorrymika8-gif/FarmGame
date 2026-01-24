namespace FarmGame.LLMCore.Brain
{
    /// <summary>
    /// 决策类型常量定义
    /// </summary>
    public static class DecisionTypes
    {
        /// <summary>行为决策 - 通用的行为选择</summary>
        public const string Behavior = "Behavior";

        /// <summary>对话决策 - 与玩家或NPC对话</summary>
        public const string Dialogue = "Dialogue";

        /// <summary>战斗决策 - 战斗中的行动选择</summary>
        public const string Combat = "Combat";

        /// <summary>交易决策 - 买卖物品</summary>
        public const string Trade = "Trade";
    }
}
