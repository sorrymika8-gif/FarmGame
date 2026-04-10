class_name SplitHandler
extends RefCounted

## 分裂处理器 - 命中时分裂出多个子投射物

const SPLIT_ANGLE_SPREAD: float = 15.0

## 执行分裂逻辑
static func execute(entity: SkillEntity) -> void:
	if entity == null or entity.data == null:
		return
	if entity.data.split <= 0:
		return
	var split_count := entity.data.split
	for i in range(split_count):
		var new_data := entity.data.clone()
		new_data.split = 0  # 子弹不再分裂
		var angle_offset := (i - (split_count - 1) / 2.0) * SPLIT_ANGLE_SPREAD
		var new_rotation := entity.rotation + deg_to_rad(angle_offset)
		var request := SpawnRequest.new()
		request.data = new_data
		request.position = entity.global_position
		request.rotation_angle = new_rotation
		request.scheduled_time = 0.0  # 立即生成
		request.owner = entity.owner_entity
		SpawnQueue.instance.enqueue(request)

## 执行延迟分裂
static func execute_delayed(entity: SkillEntity, p_delay: float) -> void:
	if entity == null or entity.data == null:
		return
	if entity.data.split <= 0:
		return
	var split_count := entity.data.split
	var current_time := Time.get_ticks_msec() / 1000.0
	for i in range(split_count):
		var new_data := entity.data.clone()
		new_data.split = 0
		var angle_offset := (i - (split_count - 1) / 2.0) * SPLIT_ANGLE_SPREAD
		var new_rotation := entity.rotation + deg_to_rad(angle_offset)
		var request := SpawnRequest.new()
		request.data = new_data
		request.position = entity.global_position
		request.rotation_angle = new_rotation
		request.scheduled_time = current_time + p_delay
		request.owner = entity.owner_entity
		SpawnQueue.instance.enqueue(request)
