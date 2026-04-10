## 可移动组件
## 挂载在任何需要移动能力的节点上（玩家、NPC等）
## 对应 Unity 的 Movable
class_name Movable
extends Node

var _move_speed: float = 100.0
var _stopping_distance: float = 2.0
var _is_moving: bool = false
var _target_position: Vector2 = Vector2.ZERO
var _move_direction: Vector2 = Vector2.ZERO
var _facing_direction: Vector2 = Vector2.DOWN

## 移动的目标节点（通常是父节点）
@export var target_node: Node2D = null

## 移动速度
var move_speed: float:
	get:
		return _move_speed
	set(value):
		_move_speed = maxf(0, value)

## 是否正在移动
var is_moving: bool:
	get:
		return _is_moving

## 当前移动方向
var move_direction: Vector2:
	get:
		return _move_direction

## 当前朝向
var facing_direction: Vector2:
	get:
		return _facing_direction

## 目标位置
var target_position: Vector2:
	get:
		return _target_position

signal move_started
signal move_stopped
signal direction_changed(direction: Vector2)

func _ready() -> void:
	_facing_direction = Vector2.DOWN
	if target_node == null:
		target_node = get_parent() as Node2D
	
	# 注册到移动管理器
	if MovementManager:
		MovementManager.register(self)

func _exit_tree() -> void:
	if MovementManager:
		MovementManager.unregister(self)

func _process(delta: float) -> void:
	if _is_moving:
		_update_movement(delta)

## 移动到目标位置
func move_to(target_pos: Vector2) -> void:
	if target_node == null:
		return
	
	_target_position = target_pos
	
	var current_pos = target_node.position
	var new_direction = (target_pos - current_pos).normalized()
	
	if new_direction != Vector2.ZERO:
		_facing_direction = new_direction
	
	var direction_changed_flag = _move_direction != new_direction
	_move_direction = new_direction
	
	if not _is_moving:
		_is_moving = true
		move_started.emit()
	
	if direction_changed_flag:
		direction_changed.emit(_move_direction)

## 停止移动
func stop_movement() -> void:
	if _is_moving:
		_is_moving = false
		_move_direction = Vector2.ZERO
		move_stopped.emit()

## 传送到指定位置
func teleport_to(position: Vector2) -> void:
	stop_movement()
	if target_node:
		target_node.position = position

## 设置朝向
func set_facing_direction(direction: Vector2) -> void:
	if direction != Vector2.ZERO:
		_facing_direction = direction.normalized()
		direction_changed.emit(_facing_direction)

# --- 私有方法 ---

func _update_movement(delta: float) -> void:
	if target_node == null:
		stop_movement()
		return
	
	var current_pos = target_node.position
	var distance = current_pos.distance_to(_target_position)
	
	# 到达目标
	if distance < _stopping_distance:
		target_node.position = _target_position
		stop_movement()
		return
	
	# 平滑移动
	var new_pos = current_pos.move_toward(_target_position, _move_speed * delta)
	target_node.position = new_pos
