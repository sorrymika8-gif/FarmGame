using System;
using System.Collections.Generic;

namespace FarmGame.LLMCore.Brain
{
    /// <summary>
    /// 提示词构建器注册表
    /// </summary>
    public class PromptBuilderRegistry
    {
        private readonly Dictionary<string, IPromptBuilder> mBuilders = new();

        /// <summary>注册一个构建器</summary>
        public void Register(IPromptBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            mBuilders[builder.DecisionType] = builder;
        }

        /// <summary>获取指定决策类型的构建器</summary>
        public IPromptBuilder Get(string decisionType)
        {
            return mBuilders.TryGetValue(decisionType, out var builder) ? builder : null;
        }

        /// <summary>是否已注册指定决策类型</summary>
        public bool Has(string decisionType) => mBuilders.ContainsKey(decisionType);
    }
}
