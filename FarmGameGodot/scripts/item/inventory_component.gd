## 背包组件
## 管理玩家持有的物品
## 对应 Unity 的 InventoryComponent
class_name InventoryComponent
extends RefCounted

var _items: Dictionary = {} # config_id -> ItemEntity

signal item_changed(config_id: int, new_count: int)

## 添加物品
func add_item(config_id: int, count: int = 1) -> void:
	if _items.has(config_id):
		_items[config_id].count += count
	else:
		var ItemEntityScript = load("res://scripts/item/item_entity.gd")
		_items[config_id] = ItemEntityScript.new(config_id, count)
	
	print("[Inventory] 添加物品 %d x%d, 当前: %d" % [config_id, count, _items[config_id].count])
	item_changed.emit(config_id, _items[config_id].count)

## 移除物品
func remove_item(config_id: int, count: int = 1) -> bool:
	if _items.has(config_id):
		var item = _items[config_id]
		if item.count >= count:
			item.count -= count
			var remaining = item.count
			if item.count <= 0:
				_items.erase(config_id)
			print("[Inventory] 移除物品 %d x%d" % [config_id, count])
			item_changed.emit(config_id, remaining)
			return true
	return false

## 检查是否拥有指定数量的物品
func has_item(config_id: int, count: int = 1) -> bool:
	return _items.has(config_id) and _items[config_id].count >= count

## 获取指定物品
func get_item(config_id: int):
	if _items.has(config_id):
		return _items[config_id]
	return null

## 获取指定物品数量
func get_item_count(config_id: int) -> int:
	if _items.has(config_id):
		return _items[config_id].count
	return 0

## 获取所有物品
func get_all_items() -> Array:
	return _items.values()

## 清空背包
func clear() -> void:
	var ids = _items.keys()
	for config_id in ids:
		_items.erase(config_id)
		item_changed.emit(config_id, 0)
	print("[Inventory] 已清空")
