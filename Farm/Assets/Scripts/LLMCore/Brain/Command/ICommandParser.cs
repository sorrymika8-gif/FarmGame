using System.Collections.Generic;

namespace FarmGame.LLMCore.Brain
{
    /// <summary>
    /// 指令解析器接口
    /// </summary>
    public interface ICommandParser
    {
        /// <summary>该解析器处理的决策类型</summary>
        string DecisionType { get; }

        /// <summary>解析 LLM 返回的文本，生成指令列表</summary>
        IEnumerable<ICommand> Parse(string llmOutput);
    }
}
