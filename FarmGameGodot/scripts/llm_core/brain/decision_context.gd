## 决策上下文
## 包含大脑做决策时需要的所有数据
class_name DecisionContext
extends RefCounted

## 决策类型
var decision_type: String = ""

## 角色设定（名字、性格、背景等）
var character_profile: Dictionary = {}

## 当前属性（血量、攻击力、情绪等）
var current_state: Dictionary = {}

## 环境感知（看到什么、听到什么）
var perception: Dictionary = {}

## 记忆存储引用
var memory_store: MemoryStore = null

## 触发事件（是什么导致了这次决策）
var trigger_event: String = ""

## 额外数据（特定决策类型可能需要的数据）
var extra: Dictionary = {}
