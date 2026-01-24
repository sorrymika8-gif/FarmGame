using System;
using System.Collections.Generic;

namespace FarmGame.LLMCore.Brain
{
    /// <summary>
    /// 指令执行器注册表
    /// </summary>
    public class CommandExecutorRegistry
    {
        private readonly Dictionary<string, ICommandExecutor> mExecutors = new();

        /// <summary>注册一个执行器</summary>
        public void Register(ICommandExecutor executor)
        {
            if (executor == null)
                throw new ArgumentNullException(nameof(executor));

            mExecutors[executor.CommandType] = executor;
        }

        /// <summary>获取指定指令类型的执行器</summary>
        public ICommandExecutor Get(string commandType)
        {
            return mExecutors.TryGetValue(commandType, out var executor) ? executor : null;
        }
    }
}
