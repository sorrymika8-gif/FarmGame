using System;
using System.Collections.Generic;

namespace FarmGame.LLMCore.Brain
{
    /// <summary>
    /// 指令解析器注册表
    /// </summary>
    public class CommandParserRegistry
    {
        private readonly Dictionary<string, ICommandParser> mParsers = new();

        /// <summary>注册一个解析器</summary>
        public void Register(ICommandParser parser)
        {
            if (parser == null)
                throw new ArgumentNullException(nameof(parser));

            mParsers[parser.DecisionType] = parser;
        }

        /// <summary>获取指定决策类型的解析器</summary>
        public ICommandParser Get(string decisionType)
        {
            return mParsers.TryGetValue(decisionType, out var parser) ? parser : null;
        }
    }
}
