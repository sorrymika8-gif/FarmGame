using UnityEngine;
using FarmGame.LLMCore.Memory;

namespace FarmGame.LLMCore.Brain
{
    /// <summary>
    /// 记忆操作指令执行器
    /// 添加或删除记忆
    /// </summary>
    public class MemoryOperationExecutor : ICommandExecutor
    {
        public string CommandType => CommandTypes.MemoryOperation;

        /// <summary>记忆操作类型 - 添加</summary>
        public const string OperationAdd = "Add";

        /// <summary>记忆操作类型 - 删除</summary>
        public const string OperationRemove = "Remove";

        public void Execute(ICommand command, DecisionContext context)
        {
            if (command is not MemoryOperationCommand memCmd)
            {
                Debug.LogError("[MemoryOperationExecutor] 收到非MemoryOperationCommand类型的指令");
                return;
            }

            if (context.MemoryStore == null)
            {
                Debug.LogError("[MemoryOperationExecutor] 上下文中未设置MemoryStore");
                return;
            }

            if (string.IsNullOrEmpty(memCmd.Content))
            {
                Debug.LogWarning("[MemoryOperationExecutor] 记忆内容为空");
                return;
            }

            // 确定分区（默认使用 "default"）
            string partitionName = string.IsNullOrEmpty(memCmd.Partition) ? "default" : memCmd.Partition;

            switch (memCmd.Operation)
            {
                case OperationAdd:
                    ExecuteAdd(context.MemoryStore, partitionName, memCmd.Content);
                    break;

                case OperationRemove:
                    ExecuteRemove(context.MemoryStore, partitionName, memCmd.Content);
                    break;

                default:
                    Debug.LogWarning($"[MemoryOperationExecutor] 未知的操作类型: {memCmd.Operation}");
                    break;
            }
        }

        private void ExecuteAdd(MemoryStore store, string partitionName, string content)
        {
            var partition = store.GetOrCreatePartition(partitionName);
            var memory = new Memory.Memory(content);
            partition.Add(memory);

            Debug.Log($"[MemoryOperationExecutor] 添加记忆到分区 '{partitionName}': {content}");
        }

        private void ExecuteRemove(MemoryStore store, string partitionName, string content)
        {
            var partition = store.GetPartition(partitionName);
            if (partition == null)
            {
                Debug.LogWarning($"[MemoryOperationExecutor] 分区 '{partitionName}' 不存在");
                return;
            }

            var memory = new Memory.Memory(content);
            bool removed = partition.Remove(memory);

            if (removed)
            {
                Debug.Log($"[MemoryOperationExecutor] 从分区 '{partitionName}' 删除记忆: {content}");
            }
            else
            {
                Debug.LogWarning($"[MemoryOperationExecutor] 未找到要删除的记忆: {content}");
            }
        }
    }
}
