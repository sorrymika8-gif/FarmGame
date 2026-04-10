## Brain 工厂
## 提供预配置好的 Brain 实例
class_name BrainFactory
extends RefCounted

## 创建一个使用统一决策的 Brain（推荐）
static func create_unified_brain() -> Brain:
	var brain = Brain.new()
	brain.register_all_executors()
	return brain

## 创建一个空的 Brain（需要手动注册组件）
static func create_empty() -> Brain:
	return Brain.new()
