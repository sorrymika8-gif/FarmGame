## 相机跟随组件
## 实现平滑跟随目标
## 对应 Unity 的 CameraFollow
class_name CameraFollow
extends Camera2D

## 跟随目标
@export var target: Node2D = null
## 相机偏移量
@export var follow_offset: Vector2 = Vector2.ZERO
## 平滑跟随速度
@export_range(0.1, 20.0) var smooth_speed: float = 5.0
## 是否启用跟随
@export var is_following: bool = true

## 设置跟随目标
func set_target(new_target: Node2D) -> void:
	target = new_target
	if target:
		global_position = target.global_position + follow_offset

## 设置跟随目标（带偏移量）
func set_target_with_offset(new_target: Node2D, new_offset: Vector2) -> void:
	follow_offset = new_offset
	set_target(new_target)

## 立即跳转到目标位置
func snap_to_target() -> void:
	if target:
		global_position = target.global_position + follow_offset

## 暂停跟随
func pause_follow() -> void:
	is_following = false

## 恢复跟随
func resume_follow() -> void:
	is_following = true

func _process(delta: float) -> void:
	if not is_following or target == null:
		return
	
	var target_pos := target.global_position + follow_offset
	global_position = global_position.lerp(target_pos, smooth_speed * delta)
