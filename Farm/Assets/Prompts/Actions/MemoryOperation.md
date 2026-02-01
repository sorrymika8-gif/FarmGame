### 记忆操作 (MemoryOperation)
用于主动记住某件重要的事情，或选择遗忘某件事。
当发生了值得铭记的事情时使用。

**格式**:
```json
{"type": "MemoryOperation", "operation": "Add或Remove", "partition": "记忆分区", "content": "记忆内容"}
```

**参数说明**:
- operation: "Add"（添加记忆）或 "Remove"（移除记忆）
- partition: "long_term"（长期记忆）或 "permanent"（永久记忆）
- content: 要记住或遗忘的内容

**示例**:
```json
{"type": "MemoryOperation", "operation": "Add", "partition": "long_term", "content": "玩家今天夸我的花很漂亮"}
```

**使用场景**:
- 玩家做了让你感动或印象深刻的事
- 得知了重要的信息
- 发生了改变关系的事件
- 想要忘记不愉快的经历
