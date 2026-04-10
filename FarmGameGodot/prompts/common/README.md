# 通用提示词模块目录

此目录存放所有NPC共用的提示词模块。这些模块会在构建提示词时自动组装。

## 模块列表

| 文件 | 说明 | 加载顺序 |
|------|------|----------|
| `BaseIdentity.md` | 基础身份说明和角色设定占位符 | 1 |
| `StatePerception.md` | 当前状态、环境感知、触发事件 | 3 |
| `MemorySystem.md` | 三层记忆系统说明 | 4 |
| `DecisionRules.md` | 通用决策规则和表情使用规则 | 6 |
| `OutputFormat.md` | JSON输出格式说明和示例 | 7 |

**注意**：NPC专属人设文件（顺序2）和可用行为/表情（顺序5）由代码动态插入。

## 组装顺序

```
1. BaseIdentity.md      — 基础身份
2. Npcs/xxx.md          — NPC专属人设（性格、说话风格、特殊要求）
3. StatePerception.md   — 状态和感知
4. MemorySystem.md      — 记忆系统
5. Actions/*.md         — 可用行为（代码动态加载）
   Expressions/*.md     — 可用表情（代码动态加载）
6. DecisionRules.md     — 通用决策规则
7. OutputFormat.md      — 输出格式
```

## 修改注意事项

- 修改这些文件会影响**所有NPC**的行为
- 占位符（如 `{{CURRENT_STATE}}`）会在运行时被实际数据替换
- 新增占位符需要同步修改 `unified_prompt_builder.gd`
