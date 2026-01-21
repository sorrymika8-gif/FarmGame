using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using FarmGame.GameLLM;
using GameLLM.Memory;
using UnityEngine;

using GameLLM.Prompts;
using GameLLM.Brain.Prompts;

namespace GameLLM.Brain
{
    /// <summary>
    /// 大脑记忆模块
    /// 负责记忆的写入、容量监控和整理
    /// </summary>
    public class MemoryBrain
    {
        private readonly MemoryStore _store;
        private readonly LLMClient _llmClient;
        private readonly List<PartitionConfig> _configs;
        private readonly CharacterSetting _characterSetting;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="store">记忆存储</param>
        /// <param name="llmClient">LLM客户端</param>
        /// <param name="characterSetting">角色设定</param>
        /// <param name="configs">分区配置列表</param>
        public MemoryBrain(MemoryStore store, LLMClient llmClient, CharacterSetting characterSetting, List<PartitionConfig> configs)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _llmClient = llmClient ?? throw new ArgumentNullException(nameof(llmClient));
            _characterSetting = characterSetting ?? new CharacterSetting();
            _configs = configs ?? new List<PartitionConfig>();

            // 确保分区存在
            foreach (var config in _configs)
            {
                _store.GetOrCreatePartition(config.Name);
            }
        }

        /// <summary>
        /// 添加记忆
        /// </summary>
        /// <param name="partitionName">分区名称</param>
        /// <param name="content">记忆内容</param>
        public async UniTask AddMemoryAsync(string partitionName, string content)
        {
            var partition = _store.GetOrCreatePartition(partitionName);
            partition.Append(content);

            // 检查容量
            var config = _configs.FirstOrDefault(c => c.Name == partitionName);
            if (config != null && config.Capacity.HasValue && partition.Count > config.Capacity.Value)
            {
                await OrganizeMemoriesAsync(partitionName);
            }
        }

        /// <summary>
        /// 触发记忆整理
        /// </summary>
        /// <param name="triggerPartitionName">触发整理的分区名称</param>
        private async UniTask OrganizeMemoriesAsync(string triggerPartitionName)
        {
            Debug.Log($"[MemoryBrain] 触发记忆整理: {triggerPartitionName}");

            // 1. 构建 Prompt
            var context = new OrganizeMemoryContext(
                _characterSetting,
                _configs,
                _store,
                triggerPartitionName);

            var builder = PromptFactory.Get<OrganizeMemoryPromptBuilder>();
            var prompt = builder.Build(context);

            // 2. 调用 LLM
            var request = new LLMRequest()
                .AddSystem(prompt)
                .AddUser("请根据新的事件进行记忆整理");
                //.SetJsonMode(true); // 如果支持 JSON 模式

            var (success, operations, error) = await _llmClient.SendAsync<List<MemoryOperation>>(request);

            if (!success)
            {
                Debug.LogError($"[MemoryBrain] 记忆整理失败: {error}");
                return;
            }

            // 3. 执行操作
            ExecuteOperations(operations);
        }

        /// <summary>
        /// 执行操作
        /// </summary>
        private void ExecuteOperations(List<MemoryOperation> operations)
        {
            if (operations == null || operations.Count == 0) return;

            Debug.Log($"[MemoryBrain] 执行 {operations.Count} 条整理操作");

            // 按索引倒序处理删除操作，避免索引偏移问题？
            // 不，LLM 返回的操作是基于当时的 snapshot。如果操作有多个针对同一分区的 Delete/Transfer，直接执行会导致索引偏移。
            // 简单处理：倒序执行？或者假设 LLM 够聪明？
            // 通常 LLM 输出的操作是每一条独立的。如果删除了 index 0，以前的 index 1 就变成了 0。
            // 如果 LLM 输出了：Delete 0, Delete 1 (原索引)。
            // 执行 Delete 0 后，原 1 变成 0。再执行 Delete 1 删除的是原 2。这通常不是 LLM 的意图。
            // 要么要求 LLM 倒序输出，要么我们在执行前对操作进行排序和调整。
            // 鉴于复杂性，这里先简单按顺序执行，但实际应用中最好先处理 Delete 操作（倒序），再处理 Update/Transfer。
            
            // 更好的策略：
            // 分区 -> 操作列表
            // 对每个分区，先收集所有要删除/移出的 Index。
            // 排序（从大到小）执行删除。
            // 这样会破坏 Add/Update 的逻辑吗？
            // Update 依赖 Index。Delete 依赖 Index。Move 依赖 Index。
            
            // 这是一个经典的并发修改问题。
            // 假设我们严格按照 LLM 给出的顺序执行，但是我们告诉 LLM "这些操作是基于当前状态的"。
            // 如果 LLM 给了 Delete 0, Delete 1。意味着它想删掉第0个和第1个。
            // 如果我们执行 Delete 0，数组缩短。再执行 Delete 1 (现在就是原来的第2个)，那就错了。
            // 所以必须处理索引偏移。
            
            // 方案：
            // 将所有操作按 Partition 分组。
            // 对于每个 Partition：
            // 1. 找出所有涉及 Index 的操作 (Delete, Update, Transfer-From sources)。
            // 2. 按照 Index 从大到小排序这些操作。
            // 3. 依次执行。这是最安全的，因为后面的操作不会影响前面的索引。
            // Add 操作通常是 Append，不受索引影响。但是如果在 Transfer-To 中指定了 Index (本设计中没有)，那就有影响。
            // Add 是追加，放在最后执行即可。

            // 然而，Update和Transfer-Source并不改变数组长度（Transfer可能会移除？）。
            // Transfer 定义：fromPartition, index, toPartition. 
            // 在设计文档中：Transfer = 从一个分区移动到另一个。意味着从原分区移除，加到新分区。
            // 所以 Delete 和 Transfer-From 会改变索引。Update 不会。
            
            // 只有 Delete 和 Transfer(From) 会导致后续索引失效。
            // 必须按 Index 倒序执行所有操作。
            
            // 步骤：
            // 1. 分离出 Add 操作（因为它们不依赖 Index，且只追加）。
            // 2. 将 Delete, Update, Transfer 视为 "IndexBasedOperations"。
            // 3. 对所有 IndexBasedOperations 按 (Partition, Index Descending) 排序。
            // 4. 执行。

            var addOps = new List<MemoryOperation>();
            var indexOps = new List<MemoryOperation>();

            foreach (var op in operations)
            {
                if (op.type == "Add")
                {
                    addOps.Add(op);
                }
                else
                {
                    indexOps.Add(op);
                }
            }

            // 按 Partition 分组，组内按 Index 倒序
            // 注意：MemoryOperation 有 partition 和 fromPartition 两个字段可能表示目标分区。
            // Delete: partition
            // Update: partition
            // Transfer: fromPartition
            
            var sortedIndexOps = indexOps.OrderByDescending(op => op.index).ToList();

            // 执行 Index 相关操作 (倒序)
            foreach (var op in sortedIndexOps)
            {
                try
                {
                    switch (op.type)
                    {
                        case "Delete":
                            {
                                var p = _store.GetPartition(op.partition);
                                if (p != null) p.RemoveAt(op.index);
                            }
                            break;
                        case "Update":
                            {
                                var p = _store.GetPartition(op.partition);
                                if (p != null) p.UpdateAt(op.index, op.newContent);
                            }
                            break;
                        case "Transfer":
                            {
                                var fromP = _store.GetPartition(op.fromPartition);
                                var toP = _store.GetOrCreatePartition(op.toPartition);
                                
                                if (fromP != null)
                                {
                                    var mem = fromP.GetAt(op.index);
                                    // 先加到目标（Append），再从源移除
                                    toP.Append(mem.Content);
                                    fromP.RemoveAt(op.index); 
                                }
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[MemoryBrain] 执行操作 {op.type} 失败: {ex.Message}");
                }
            }

            // 执行 Add 操作
            foreach (var op in addOps)
            {
                try
                {
                    var p = _store.GetOrCreatePartition(op.partition);
                    p.Append(op.content);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[MemoryBrain] 执行 Add 失败: {ex.Message}");
                }
            }
            
            Debug.Log("[MemoryBrain] 整理完成");
        }
    }
}
