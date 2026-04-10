## 物品实体
## 运行时物品数据
## 对应 Unity 的 ItemEntity
class_name ItemEntity
extends RefCounted

var config_id: int = 0
var count: int = 0
var instance_id: String = ""

## 获取物品配置
var config: Dictionary:
	get:
		return ConfigManager.get_config("item", config_id)

## 获取统一的物品配置信息
var config_info: Dictionary:
	get:
		return _get_config_info()

func _init(p_config_id: int = 0, p_count: int = 1) -> void:
	config_id = p_config_id
	count = p_count
	instance_id = _generate_uuid()

func _get_config_info() -> Dictionary:
	# 先尝试 item 配置
	var item_config = ConfigManager.get_config("item", config_id)
	if not item_config.is_empty():
		return item_config
	# 再尝试 seed 配置
	var seed_config = ConfigManager.get_config("seed", config_id)
	if not seed_config.is_empty():
		return seed_config
	return {}

func _generate_uuid() -> String:
	# 简单 UUID 生成
	var chars = "0123456789abcdef"
	var uuid = ""
	for i in range(32):
		uuid += chars[randi() % chars.length()]
		if i in [7, 11, 15, 19]:
			uuid += "-"
	return uuid
