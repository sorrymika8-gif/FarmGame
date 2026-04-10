### 设置表情 (SetExpression)
用于改变你的面部表情，让对话更加生动。
表情会反映在你的立绘显示上。

**格式**:
```json
{"type": "SetExpression", "expression": "表情ID"}
```

**示例**:
```json
{"type": "SetExpression", "expression": "happy"}
```

**可用表情**:
{{EXPRESSION_LIST}}

**使用建议**:
- 表情变化应该自然、符合对话内容
- 不需要每句话都切换表情，只在情绪有明显变化时使用
- 如果对话内容没有引发情绪变化，可以不输出此指令
- expression 字段必须使用上述表情ID（严格字符串匹配）
