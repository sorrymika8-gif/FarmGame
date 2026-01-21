namespace GameLLM.Prompts
{
    /// <summary>
    /// 提示词构建器接口
    /// </summary>
    /// <typeparam name="TContext">构建提示词所需的上下文数据类型</typeparam>
    public interface IPromptBuilder<TContext>
    {
        /// <summary>
        /// 构建提示词
        /// </summary>
        /// <param name="context">上下文数据</param>
        /// <returns>构建完成的提示词</returns>
        string Build(TContext context);
    }
}
