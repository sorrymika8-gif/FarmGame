## 移动管理器
## 负责管理所有可移动实体，提供全局移动控制接口
extends Node

var _is_initialized: bool = false
var _movables: Array = [] # Array[Movable nodes]

## 是否启用全局移动
var global_movement_enabled: bool = true

## 当前注册的可移动实体数量
var movable_count: int:
	get:
		return _movables.size()

func initialize() -> void:
	if _is_initialized:
		return
	_movables.clear()
	_is_initialized = true
	print("[MovementManager] 初始化完成")

## 注册可移动实体
func register(movable: Node) -> void:
	if movable == null:
		return
	if movable not in _movables:
		_movables.append(movable)

## 注销可移动实体
func unregister(movable: Node) -> void:
	if movable == null:
		return
	_movables.erase(movable)

## 停止所有实体移动
func stop_all() -> void:
	for movable in _movables:
		if is_instance_valid(movable) and movable.has_method("stop_movement"):
			movable.stop_movement()

## 获取所有正在移动的实体
func get_moving_entities() -> Array:
	var moving: Array = []
	for movable in _movables:
		if is_instance_valid(movable) and movable.has_method("is_moving") and movable.is_moving:
			moving.append(movable)
	return moving

## 检查是否有任何实体正在移动
func is_any_moving() -> bool:
	for movable in _movables:
		if is_instance_valid(movable) and movable.get("is_moving") == true:
			return true
	return false

func _notification(what: int) -> void:
	if what == NOTIFICATION_PREDELETE:
		_movables.clear()
