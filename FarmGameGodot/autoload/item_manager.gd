## 物品管理器
## 负责物品注册和使用逻辑
extends Node

var _is_initialized: bool = false
var _item_use_handlers: Dictionary = {} # item_type -> Callable

func initialize() -> void:
	if _is_initialized:
		return
	
	_register_item_handlers()
	_is_initialized = true
	print("[ItemManager] 初始化完成")

## 使用道具
func use_item(player_data: Dictionary, item) -> bool:
	if item == null:
		return false
	
	var item_type = item.get("item_type", -1)
	if item_type is int and _item_use_handlers.has(item_type):
		return _item_use_handlers[item_type].call(player_data, item)
	
	push_warning("[ItemManager] 未找到物品类型 %s 的处理器" % str(item_type))
	return false

## 检查道具是否可使用
func can_use_item(item) -> bool:
	if item == null:
		return false
	var item_type = item.get("item_type", -1)
	return _item_use_handlers.has(item_type)

## 创建道具实体
func create_item(config_id: int, count: int = 1):
	var ItemEntityScript = load("res://scripts/item/item_entity.gd")
	return ItemEntityScript.new(config_id, count)

## 注册物品使用处理器
func register_handler(item_type: int, handler: Callable) -> void:
	_item_use_handlers[item_type] = handler
	print("[ItemManager] 注册处理器: 类型 %d" % item_type)

# --- 私有方法 ---

func _register_item_handlers() -> void:
	_item_use_handlers.clear()
	# 注册内置处理器
	# 0: 消耗品（回复）
	register_handler(0, _handle_heal_item)
	# 1: 装备
	register_handler(1, _handle_equip_item)
	print("[ItemManager] 已注册 %d 个处理器" % _item_use_handlers.size())

## 回复类物品处理
func _handle_heal_item(player_data: Dictionary, item) -> bool:
	# TODO: 实现回复逻辑
	print("[ItemManager] 使用回复物品: %s" % str(item))
	return true

## 装备类物品处理
func _handle_equip_item(player_data: Dictionary, item) -> bool:
	# TODO: 实现装备逻辑
	print("[ItemManager] 使用装备物品: %s" % str(item))
	return true
