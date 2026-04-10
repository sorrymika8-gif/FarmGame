# NPC 专属人设目录

此目录存放每个NPC的**专属人设文件**（性格、说话风格、特殊要求）。

## 模块化架构

提示词系统采用模块化设计，最终发给LLM的提示词由以下部分组装：

```
┌─────────────────────────────────────────────────────────┐
│  1. Common/BaseIdentity.md    — 基础身份说明            │
│  2. Npcs/xxx.md               — NPC专属人设 ⬅ 本目录    │
│  3. Common/StatePerception.md — 状态和感知              │
│  4. Common/MemorySystem.md    — 记忆系统                │
│  5. Actions/*.md              — 可用行为                │
│  6. Expressions/*.md          — 可用表情                │
│  7. Common/DecisionRules.md   — 通用决策规则            │
│  8. Common/OutputFormat.md    — 输出格式                │
└─────────────────────────────────────────────────────────┘
```

**好处**：
- NPC文件只需关注人设，无需重复通用内容
- 修改通用规则只需改一处，所有NPC生效
- 新建NPC更简单

## 配置表字段对应关系

`npc.json` 配置表字段：

| 配置表字段 | NPCEntity属性 | 说明 | 示例 |
|-----------|--------------|------|------|
| class_id | id | NPC唯一标识 | 1 |
| name | name | NPC名字 | 哈曼 |
| gender | gender | 性别 | 女 |
| prompt | prompt_file_path | 专属人设文件名 | haman.md |
| model_name | - | 模型预制体名 | haman |
| init_pos | position | 初始位置[x,y] | [7, 7] |
| interaction_dis | interaction_distance | 交互距离 | 3 |

## 使用方法

1. 在此目录下为**每个NPC**创建独立的 `.md` 文件
2. 在 `npc.json` 配置表的 `prompt` 字段中填写文件名（如 `haman.md`）
3. 每个NPC **必须** 配置专属人设文件，否则运行时会报错

## NPC专属文件格式

NPC文件只需包含**该NPC独有的内容**：

```markdown
# NPC名字

一句话角色描述。

## 性格特点
- **核心特质**：展开说明
- 其他性格特点
- 口癖或标志性动作

## 说话风格
- 语气特点
- 常用词汇或句式

## 特殊决策要求
1. 角色专属的行为规则
2. 特定情境下的反应方式
```

**注意**：不需要写记忆系统、输出格式等通用内容，这些由 `Common/` 目录的模块自动添加。

## 示例文件

参考 `example_npc.md` 作为模板创建新的NPC专属人设。

## 注意事项

- 文件名区分大小写
- 文件必须是 UTF-8 编码
- 通用内容请修改 `../Common/` 目录下的文件
