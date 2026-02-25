namespace FarmGame.LLMCore.Brain
{
    /// <summary>
    /// Brain 工厂
    /// 提供预配置好的 Brain 实例
    /// </summary>
    public static class BrainFactory
    {
        /// <summary>
        /// 创建一个使用统一决策的 Brain（推荐）
        /// 使用单一的 Unified 决策类型，通过 TriggerEvent 区分不同场景
        /// 已注册所有 Executor
        /// </summary>
        public static Brain CreateUnifiedBrain()
        {
            var brain = new Brain();

            // 注册统一决策的提示词构建器
            brain.RegisterPromptBuilder(new UnifiedPromptBuilder());

            // 注册统一决策的指令解析器
            brain.RegisterCommandParser(new UnifiedCommandParser());

            // 注册所有执行器
            brain.RegisterCommandExecutor(new MoveExecutor());
            brain.RegisterCommandExecutor(new SpeakExecutor());
            brain.RegisterCommandExecutor(new AttackExecutor());
            brain.RegisterCommandExecutor(new SetStateExecutor());
            brain.RegisterCommandExecutor(new MemoryOperationExecutor());
            brain.RegisterCommandExecutor(new SetExpressionExecutor());
            brain.RegisterCommandExecutor(new SetMoodExecutor());

            return brain;
        }

        /// <summary>
        /// 创建一个标准的行为决策 Brain
        /// 已注册 Behavior 相关的 PromptBuilder、CommandParser 和所有 Executor
        /// [已废弃] 请使用 CreateUnifiedBrain()
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
        /// 创建一个支持聊天对话的 Brain
        /// 已注册 Chat 相关的 PromptBuilder、CommandParser 和 SpeakExecutor
        /// [已废弃] 请使用 CreateUnifiedBrain()
        /// </summary>
        public static Brain CreateChatBrain()
        {
            var brain = new Brain();

            // 注册聊天决策的提示词构建器
            brain.RegisterPromptBuilder(new ChatPromptBuilder());

            // 注册聊天决策的指令解析器
            brain.RegisterCommandParser(new ChatCommandParser());

            // 注册说话执行器（聊天主要需要这个）
            brain.RegisterCommandExecutor(new SpeakExecutor());

            return brain;
        }

        /// <summary>
        /// 创建一个完整功能的 Brain
        /// 支持 Unified、Behavior 和 Chat 三种决策类型
        /// </summary>
        public static Brain CreateFullBrain()
        {
            var brain = new Brain();

            // 注册统一决策（推荐使用）
            brain.RegisterPromptBuilder(new UnifiedPromptBuilder());
            brain.RegisterCommandParser(new UnifiedCommandParser());

            // 注册行为决策（向后兼容）
            brain.RegisterPromptBuilder(new BehaviorPromptBuilder());
            brain.RegisterCommandParser(new BehaviorCommandParser());

            // 注册聊天决策（向后兼容）
            brain.RegisterPromptBuilder(new ChatPromptBuilder());
            brain.RegisterCommandParser(new ChatCommandParser());

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
