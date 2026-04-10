class_name TrackingHandler
extends RefCounted

## 追踪处理器 - 让投射物追踪最近敌人

const TRACKING_RANGE: float = 15.0 * 64.0
const TRACKING_SENSITIVITY: float = 5.0

## 执行追踪逻辑
static func execute(entity: SkillEntity) -> void:
	if entity == null or entity.data == null:
		return
	if entity.data.tracking <= 0:
		return
	var target_entity := find_nearest_target(entity)
	if target_entity == null:
		return
	var to_target := target_entity.global_position - entity.global_position
	if to_target.length_squared() < 0.01:
		return
	var target_direction := to_target.normalized()
	var current_direction := entity.move_direction
	var angle := rad_to_deg(current_direction.angle_to(target_direction))
	var max_turn_angle := entity.data.tracking
	var turn_angle := clampf(angle, -max_turn_angle, max_turn_angle)
	var delta := entity.get_process_delta_time()
	var smooth_turn := turn_angle * TRACKING_SENSITIVITY * delta
	entity.rotate_direction(smooth_turn)

## 查找最近的敌方目标
static func find_nearest_target(entity: SkillEntity) -> CharEntity:
	var target_type := AtomEnums.EntityType.ENEMY
	if entity.owner_entity != null:
		target_type = AtomEnums.EntityType.ENEMY if entity.owner_entity.entity_type == AtomEnums.EntityType.PLAYER else AtomEnums.EntityType.PLAYER
	return find_nearest_in_range(entity.global_position, TRACKING_RANGE, target_type)

## 查找范围内的指定类型目标
static func find_nearest_in_range(
	center: Vector2,
	range_dist: float,
	target_type: int,
	exclude: CharEntity = null
) -> CharEntity:
	# 使用场景树查找所有 CharEntity
	var tree := Engine.get_main_loop() as SceneTree
	if tree == null:
		return null
	var nearest: CharEntity = null
	var nearest_distance := INF
	for node in tree.get_nodes_in_group("combat_entities"):
		if node is CharEntity:
			var char_entity := node as CharEntity
			if char_entity == exclude:
				continue
			if char_entity.entity_type != target_type:
				continue
			if not char_entity.is_alive():
				continue
			if char_entity.stats != null and char_entity.stats.is_stealthed():
				continue
			var dist := center.distance_to(char_entity.global_position)
			if dist <= range_dist and dist < nearest_distance:
				nearest_distance = dist
				nearest = char_entity
	return nearest
