## 你是谁
{CharacterSetting}

## 你的记忆结构
{PartitionStructure}

## 你当前的记忆
{CurrentMemories}

## 现在发生了什么
你的 {TriggerPartition} 记忆太多了，你需要整理一下。
你可以：
- 遗忘不重要的记忆（删除）
- 把详细的记忆变得模糊（修改内容）
- 把多条相似的记忆合并成一条（删除 + 新增）
- 把重要的记忆转移到更持久的地方（转移）

## 请输出你的操作
以 JSON 数组格式返回你要执行的操作列表。
可用的操作类型：
- Delete: {"type": "Delete", "partition": "分区名", "index": 索引}
- Update: {"type": "Update", "partition": "分区名", "index": 索引, "newContent": "新内容"}
- Transfer: {"type": "Transfer", "fromPartition": "来源分区", "index": 索引, "toPartition": "目标分区"}
- Add: {"type": "Add", "partition": "分区名", "content": "内容"}
请直接输出 JSON 数组，不要包含其他内容。
