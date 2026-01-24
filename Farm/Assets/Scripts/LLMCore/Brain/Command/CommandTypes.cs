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
    }
}
