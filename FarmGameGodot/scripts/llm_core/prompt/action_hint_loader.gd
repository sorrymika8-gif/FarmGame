## 行为提示词片段加载器
## 负责从 res://prompts/actions/ 目录加载各种行为的提示词片段
class_name ActionHintLoader
extends RefCounted

## 行为片段缓存
static var _action_hint_cache: Dictionary = {}
static var _is_loaded: bool = false

## 行为类型与文件名映射
static var _action_file_names: Dictionary = {
	CommandTypes.Speak: "Speak.md",
	CommandTypes.Move: "Move.md",
	CommandTypes.Attack: "Attack.md",
	CommandTypes.SetState: "SetState.md",
	CommandTypes.MemoryOperation: "MemoryOperation.md",
	CommandTypes.SetExpression: "SetExpression.md",
	CommandTypes.SetMood: "SetMood.md",
}

## 确保已加载
static func ensure_loaded() -> void:
	if _is_loaded:
		return
	_load_all()
	_is_loaded = true

## 强制重新加载
static func reload() -> void:
	_action_hint_cache.clear()
	_is_loaded = false
	ensure_loaded()

## 获取指定行为的提示词片段
static func get_action_hint(action_type: String) -> String:
	ensure_loaded()
	return _action_hint_cache.get(action_type, "")

## 获取所有行为的提示词片段
static func get_all_action_hints() -> String:
	ensure_loaded()
	var result = "你可以在回复中执行以下一种或多种行为：\n\n"
	for action_type in _action_file_names.keys():
		var hint = _action_hint_cache.get(action_type, "")
		if not hint.is_empty():
			result += hint + "\n\n"
	return result.strip_edges()

## 获取指定行为列表的提示词片段
static func get_action_hints(action_types: Array) -> String:
	ensure_loaded()
	var result = "你可以在回复中执行以下一种或多种行为：\n\n"
	for action_type in action_types:
		var hint = _action_hint_cache.get(action_type, "")
		if not hint.is_empty():
			result += hint + "\n\n"
	return result.strip_edges()

## 从目录加载所有行为片段
static func _load_all() -> void:
	var actions_dir = "res://prompts/actions/"
	
	for action_type in _action_file_names:
		var file_name = _action_file_names[action_type]
		var path = actions_dir + file_name
		
		if FileAccess.file_exists(path):
			var file = FileAccess.open(path, FileAccess.READ)
			if file:
				_action_hint_cache[action_type] = file.get_as_text()
				file.close()
				print("[ActionHintLoader] 已加载行为片段: %s" % action_type)
		else:
			push_warning("[ActionHintLoader] 行为片段文件不存在: %s" % path)
	
	print("[ActionHintLoader] 共加载 %d 个行为片段" % _action_hint_cache.size())
