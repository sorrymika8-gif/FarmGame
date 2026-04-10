## 表情提示词片段加载器
## 负责从 res://prompts/expressions/ 目录加载各种表情的描述
class_name ExpressionHintLoader
extends RefCounted

## 缓存
static var _expression_hint_cache: Dictionary = {}
static var _expression_ids: Array[String] = []
static var _is_loaded: bool = false

## 确保已加载
static func ensure_loaded() -> void:
	if _is_loaded:
		return
	_load_all()
	_is_loaded = true

## 强制重新加载
static func reload() -> void:
	_expression_hint_cache.clear()
	_expression_ids.clear()
	_is_loaded = false
	ensure_loaded()

## 获取指定表情的提示词片段
static func get_expression_hint(expression_id: String) -> String:
	ensure_loaded()
	return _expression_hint_cache.get(expression_id, "")

## 获取所有表情的提示词片段
static func get_all_expression_hints() -> String:
	ensure_loaded()
	var result = ""
	for eid in _expression_ids:
		var hint = _expression_hint_cache.get(eid, "")
		if not hint.is_empty():
			result += "- %s\n" % hint
	return result.strip_edges()

## 检查指定表情是否已加载
static func has_expression(expression_id: String) -> bool:
	ensure_loaded()
	return _expression_hint_cache.has(expression_id)

## 从目录加载所有表情片段
static func _load_all() -> void:
	var expressions_dir = "res://prompts/expressions/"
	var dir = DirAccess.open(expressions_dir)
	if dir == null:
		push_warning("[ExpressionHintLoader] 表情片段目录不存在: %s" % expressions_dir)
		return
	
	dir.list_dir_begin()
	var file_name = dir.get_next()
	while not file_name.is_empty():
		if file_name.ends_with(".md") and not dir.current_is_dir():
			var expression_id = file_name.get_basename()
			var path = expressions_dir + file_name
			var file = FileAccess.open(path, FileAccess.READ)
			if file:
				_expression_hint_cache[expression_id] = file.get_as_text().strip_edges()
				_expression_ids.append(expression_id)
				file.close()
				print("[ExpressionHintLoader] 已加载表情片段: %s" % expression_id)
		file_name = dir.get_next()
	dir.list_dir_end()
	
	print("[ExpressionHintLoader] 共加载 %d 个表情片段" % _expression_hint_cache.size())
