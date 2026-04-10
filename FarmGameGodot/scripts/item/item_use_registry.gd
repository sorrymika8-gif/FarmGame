class_name ItemUseRegistry
extends RefCounted

## 道具使用处理器注册表

static var _handlers: Dictionary = {}

## 注册处理器
static func register(handler_name: String, handler: Callable) -> void:
	if handler_name.is_empty():
		push_warning("[ItemUseRegistry] 处理器名称为空")
		return
	_handlers[handler_name] = handler
	print("[ItemUseRegistry] 注册处理器: %s" % handler_name)

## 注销处理器
static func unregister(handler_name: String) -> void:
	if _handlers.erase(handler_name):
		print("[ItemUseRegistry] 注销处理器: %s" % handler_name)

## 清空所有处理器
static func clear() -> void:
	_handlers.clear()

## 尝试使用道具
static func try_use(player_data: Dictionary, item: Dictionary) -> bool:
	if player_data.is_empty() or item.is_empty():
		push_warning("[ItemUseRegistry] 玩家或道具数据为空")
		return false
	var use_handler: String = item.get("use", "")
	if use_handler.is_empty():
		return false
	if not _handlers.has(use_handler):
		push_warning("[ItemUseRegistry] 未找到处理器: '%s'" % use_handler)
		return false
	var args: Dictionary = item.get("use_arg", {})
	return _handlers[use_handler].call(player_data, item, args)

## 检查道具是否可使用
static func can_use(item: Dictionary) -> bool:
	var use_handler: String = item.get("use", "")
	return not use_handler.is_empty() and _handlers.has(use_handler)

## 初始化默认处理器
static func register_defaults() -> void:
	register("heal", HealHandler.execute)
	register("equip", EquipHandler.execute)
