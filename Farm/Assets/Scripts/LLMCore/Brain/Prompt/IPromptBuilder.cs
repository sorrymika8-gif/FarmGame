namespace FarmGame.LLMCore.Brain
{
    /// <summary>
    /// 提示词构建器接口
    /// 每种决策类型有对应的构建器实现
    /// </summary>
    public interface IPromptBuilder
    {
        /// <summary>该构建器处理的决策类型</summary>
        string DecisionType { get; }

        /// <summary>根据上下文构建提示词</summary>
        string Build(DecisionContext context);
    }
}
