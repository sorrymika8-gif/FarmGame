### 设置状态 (SetState)
用于改变自己的情绪、姿态或内部状态。
可以用来表达情绪变化、切换行为模式等。

**格式**:
```json
{"type": "SetState", "key": "状态名", "value": "状态值"}
```

**示例**:
```json
{"type": "SetState", "key": "mood", "value": "happy"}
```

**常用状态**:
- mood（情绪）: happy, sad, angry, nervous, excited, calm, tsundere
- pose（姿态）: standing, sitting, working, resting
- attitude（态度）: friendly, hostile, neutral, curious
