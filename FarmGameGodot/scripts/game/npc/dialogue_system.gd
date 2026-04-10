class_name DialogueSystem
extends Node

## 对话系统组件 - 挂载在 NPC 上，管理与该 NPC 的 UI 交互

var _entity: NPCEntity

## 绑定实体
func bind(entity: NPCEntity) -> void:
	_entity = entity

## 开始对话
func start_dialogue() -> void:
	if _entity == null:
		return
	# 打开对话面板
	UIManager.open_dialogue_panel(_entity)

## 结束对话
func end_dialogue() -> void:
	pass

## 接收用户输入
func receive_input(content: String) -> void:
	if _entity == null:
		return
	_entity.receive_chat_async(content)
