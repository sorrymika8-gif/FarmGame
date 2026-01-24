using System.Collections.Generic;
using UnityEngine;

namespace FarmGame.LLMCore.Brain
{
    /// <summary>
    /// 指令队列
    /// 按顺序发起指令执行（不等待完成）
    /// </summary>
    public class CommandQueue
    {
        private readonly Queue<(ICommand command, DecisionContext context)> mQueue = new();
        private readonly CommandExecutorRegistry mExecutorRegistry;

        public CommandQueue(CommandExecutorRegistry executorRegistry)
        {
            mExecutorRegistry = executorRegistry;
        }

        /// <summary>当前队列中的指令数量</summary>
        public int Count => mQueue.Count;

        /// <summary>添加指令到队列</summary>
        public void Enqueue(ICommand command, DecisionContext context)
        {
            mQueue.Enqueue((command, context));
        }

        /// <summary>添加多个指令到队列</summary>
        public void EnqueueRange(IEnumerable<ICommand> commands, DecisionContext context)
        {
            foreach (var command in commands)
            {
                mQueue.Enqueue((command, context));
            }
        }

        /// <summary>处理队列中的所有指令（按顺序发起执行）</summary>
        public void ProcessAll()
        {
            while (mQueue.Count > 0)
            {
                var (command, context) = mQueue.Dequeue();
                ExecuteCommand(command, context);
            }
        }

        /// <summary>处理队列中的下一个指令</summary>
        public bool ProcessNext()
        {
            if (mQueue.Count == 0) return false;

            var (command, context) = mQueue.Dequeue();
            ExecuteCommand(command, context);
            return true;
        }

        /// <summary>清空队列</summary>
        public void Clear() => mQueue.Clear();

        private void ExecuteCommand(ICommand command, DecisionContext context)
        {
            var executor = mExecutorRegistry.Get(command.CommandType);
            if (executor == null)
            {
                Debug.LogWarning($"[CommandQueue] 未找到指令类型 '{command.CommandType}' 的执行器");
                return;
            }

            executor.Execute(command, context);
        }
    }
}
