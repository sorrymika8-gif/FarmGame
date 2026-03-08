## 输出格式

请以JSON数组格式返回你要执行的行为，可以包含一个或多个行为：

**正式对话示例**（与玩家交谈时）：
```json
[
  {"type": "SetExpression", "expression": "happy"},
  {"type": "Speak", "content": "你好啊，今天想聊些什么？"}
]
```

**日常行为示例**（自言自语、闲逛时）：
```json
[
  {"type": "SetMood", "emoji": "😊"},
  {"type": "Speak", "content": "今天天气真好呢~"},
  {"type": "Move", "x": 10.0, "y": 5.0}
]
```

请只返回JSON数组，不要包含其他文字。
