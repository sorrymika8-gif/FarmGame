namespace FarmGame.LLMCore.Brain
{
    /// <summary>
    /// Brain 工厂
    /// 提供预配置好的 Brain 实例
    /// </summary>
    public static class BrainFactory
    {
        /// <summary>
        /// 创建一个标准的行为决策 Brain
        /// 已注册 Behavior 相关的 PromptBuilder、CommandParser 和所有 Executor
        /// </summary>
        public static Brain CreateBehaviorBrain()
        {
            var brain = new Brain();

            // 注册行为决策的提示词构建器
            brain.RegisterPromptBuilder(new BehaviorPromptBuilder());

            // 注册行为决策的指令解析器
            brain.RegisterCommandParser(new BehaviorCommandParser());

            // 注册所有执行器
            brain.RegisterCommandExecutor(new MoveExecutor());
            brain.RegisterCommandExecutor(new SpeakExecutor());
            brain.RegisterCommandExecutor(new AttackExecutor());
            brain.RegisterCommandExecutor(new SetStateExecutor());
            brain.RegisterCommandExecutor(new MemoryOperationExecutor());

            return brain;
        }

        /// <summary>
        /// 创建一个空的 Brain（需要手动注册组件）
        /// </summary>
        public static Brain CreateEmpty()
        {
            return new Brain();
        }
    }
}
