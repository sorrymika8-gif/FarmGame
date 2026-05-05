## 商店管理器
## 负责商品的买卖逻辑
extends Node

## 商店类型枚举
enum ShopType {
	NONE = 0,
	SEED_SHOP = 1,
	TOOL_SHOP = 2,
	GENERAL_SHOP = 3,
}

var _is_initialized: bool = false
var _shop_items_by_type: Dictionary = {} # shop_type -> Array[ShopConfig]

signal item_bought(item_id: int, count: int, cost: int)
signal item_sold(item_id: int, count: int, income: int)

func initialize() -> void:
	if _is_initialized:
		return
	
	_shop_items_by_type = {}
	
	# 加载商店配置
	var shop_configs = ConfigManager.get_all("shop")
	for config in shop_configs:
		var shop_type = int(config.get("shop_type", 0))
		if not _shop_items_by_type.has(shop_type):
			_shop_items_by_type[shop_type] = []
		_shop_items_by_type[shop_type].append(config)
	
	_is_initialized = true
	print("[ShopManager] 初始化完成，%d 个商店类型" % _shop_items_by_type.size())

## 获取指定商店的商品列表
func get_shop_items(shop_type: int) -> Array:
	if not _is_initialized:
		push_warning("[ShopManager] 未初始化，无法获取商店商品")
		return []
	
	var normalized_shop_type = int(shop_type)
	var result: Array = []
	if _shop_items_by_type.has(normalized_shop_type):
		for shop_config in _shop_items_by_type[normalized_shop_type]:
			var item_id = int(shop_config.get("item_id", 0))
			var item_config = _get_item_config_info(item_id)
			if not item_config.is_empty():
				result.append({
					"shop_config": shop_config,
					"item_config": item_config,
					"buy_price": int(shop_config.get("buy_price", 0)),
					"item_id": item_id,
					"item_name": item_config.get("name", ""),
					"item_icon": item_config.get("icon", ""),
					"item_description": item_config.get("description", ""),
				})
			else:
				push_warning("[ShopManager] 未找到商品配置: %d" % item_id)
	else:
		push_warning("[ShopManager] 未找到商店类型: %d" % normalized_shop_type)
	
	return result

## 获取购买价格
func get_buy_price(item_id: int, shop_type: int) -> int:
	if not _is_initialized:
		return -1
	
	var normalized_item_id = int(item_id)
	var normalized_shop_type = int(shop_type)
	if _shop_items_by_type.has(normalized_shop_type):
		for config in _shop_items_by_type[normalized_shop_type]:
			if int(config.get("item_id", 0)) == normalized_item_id:
				return int(config.get("buy_price", -1))
	return -1

## 获取出售价格
func get_sell_price(item_id: int) -> int:
	if not _is_initialized:
		return 0
	var item_config = ConfigManager.get_config("item", int(item_id))
	return int(item_config.get("sell_price", 0))

## 检查是否能购买
func can_buy(item_id: int, count: int, shop_type: int) -> bool:
	if not _is_initialized or count <= 0:
		return false
	var buy_price = get_buy_price(item_id, shop_type)
	if buy_price < 0:
		return false
	return PlayerManager.has_enough_gold(buy_price * count)

## 检查是否能出售
func can_sell(item_id: int, count: int) -> bool:
	if not _is_initialized or count <= 0:
		return false
	var sell_price = get_sell_price(item_id)
	if sell_price <= 0:
		return false
	var inventory = PlayerManager.get_player_inventory()
	if inventory == null:
		return false
	return inventory.has_item(item_id, count)

## 购买物品
func buy_item(item_id: int, count: int, shop_type: int) -> bool:
	if not can_buy(item_id, count, shop_type):
		push_warning("[ShopManager] 无法购买 %d x%d" % [item_id, count])
		return false
	
	var buy_price = get_buy_price(item_id, shop_type)
	var total_cost = buy_price * count
	
	if not PlayerManager.spend_gold(total_cost):
		return false
	
	var inventory = PlayerManager.get_player_inventory()
	if inventory:
		inventory.add_item(item_id, count)
	
	item_bought.emit(item_id, count, total_cost)
	print("[ShopManager] 购买成功: 物品%d x%d, 花费%d金币" % [item_id, count, total_cost])
	return true

## 出售物品
func sell_item(item_id: int, count: int) -> bool:
	if not can_sell(item_id, count):
		push_warning("[ShopManager] 无法出售 %d x%d" % [item_id, count])
		return false
	
	var sell_price = get_sell_price(item_id)
	var total_income = sell_price * count
	
	var inventory = PlayerManager.get_player_inventory()
	if inventory == null or not inventory.remove_item(item_id, count):
		return false
	
	PlayerManager.add_gold(total_income)
	
	item_sold.emit(item_id, count, total_income)
	print("[ShopManager] 出售成功: 物品%d x%d, 获得%d金币" % [item_id, count, total_income])
	return true

# --- 私有方法 ---

## 获取物品统一配置信息（从多个配置表查询）
func _get_item_config_info(config_id: int) -> Dictionary:
	# 先尝试 item 配置表
	var item_config = ConfigManager.get_config("item", int(config_id))
	if not item_config.is_empty():
		return item_config
	
	# 再尝试 seed 配置表
	var seed_config = ConfigManager.get_config("seed", int(config_id))
	if not seed_config.is_empty():
		return seed_config
	
	return {}
