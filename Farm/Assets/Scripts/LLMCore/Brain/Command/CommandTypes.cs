namespace FarmGame.LLMCore.Brain
{
    /// <summary>
    /// 指令类型常量定义
    /// </summary>
    public static class CommandTypes
    {
        /// <summary>移动指令</summary>
        public const string Move = "Move";

        /// <summary>说话指令</summary>
        public const string Speak = "Speak";

        /// <summary>攻击指令</summary>
        public const string Attack = "Attack";

        /// <summary>设置状态指令</summary>
        public const string SetState = "SetState";

        /// <summary>记忆操作指令</summary>
        public const string MemoryOperation = "MemoryOperation";

        /// <summary>耕地指令</summary>
        public const string Till = "Till";

        /// <summary>种植指令</summary>
        public const string Plant = "Plant";

        /// <summary>收获指令</summary>
        public const string Harvest = "Harvest";

        /// <summary>设置表情指令</summary>
        public const string SetExpression = "SetExpression";

        /// <summary>设置心情指令（emoji）</summary>
        public const string SetMood = "SetMood";
    }
}
