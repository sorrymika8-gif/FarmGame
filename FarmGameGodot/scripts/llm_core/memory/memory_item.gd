## 记忆条目
class_name MemoryItem
extends RefCounted

## 记忆内容
var content: String = ""

func _init(p_content: String = "") -> void:
	content = p_content

func _to_string() -> String:
	return content
