using System;
using System.Collections.Generic;

namespace GameLLM.Prompts
{
    /// <summary>
    /// 提示词工厂
    /// 负责创建和管理提示词构建器实例
    /// </summary>
    public static class PromptFactory
    {
        private static readonly Dictionary<Type, object> _cache = new();

        /// <summary>
        /// 获取指定类型的提示词构建器
        /// </summary>
        /// <typeparam name="TBuilder">构建器类型</typeparam>
        /// <returns>构建器实例</returns>
        public static TBuilder Get<TBuilder>() where TBuilder : new()
        {
            var type = typeof(TBuilder);
            
            if (_cache.TryGetValue(type, out var cached))
            {
                return (TBuilder)cached;
            }

            var builder = new TBuilder();
            _cache[type] = builder;
            return builder;
        }
    }
}
