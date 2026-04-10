class_name AttractHandler
extends RefCounted

## 吸引/排斥处理器 - 对周围敌人施加吸引或排斥力

const ATTRACT_RANGE: float = 8.0 * 64.0
const FORCE_MULTIPLIER: float = 50.0

## 执行吸引/排斥逻辑
static func execute(entity: SkillEntity) -> void:
	if entity == null or entity.data == null:
		return
	if is_zero_approx(entity.data.attract):
		return
	var target_type := AtomEnums.EntityType.ENEMY
	if entity.owner_entity != null:
		target_type = AtomEnums.EntityType.ENEMY if entity.owner_entity.entity_type == AtomEnums.EntityType.PLAYER else AtomEnums.EntityType.PLAYER
	var tree := Engine.get_main_loop() as SceneTree
	if tree == null:
		return
	var attract_force := entity.data.attract * FORCE_MULTIPLIER
	var delta := entity.get_process_delta_time()
	for node in tree.get_nodes_in_group("combat_entities"):
		if node is CharEntity:
			var char_entity := node as CharEntity
			if char_entity.entity_type != target_type:
				continue
			if not char_entity.is_alive():
				continue
			var to_entity := char_entity.global_position - entity.global_position
			var distance := to_entity.length()
			if distance < 0.1 or distance > ATTRACT_RANGE:
				continue
			var force_direction: Vector2
			if attract_force > 0:
				force_direction = -to_entity.normalized()  # 吸引
			else:
				force_direction = to_entity.normalized()   # 排斥
			var force_magnitude := absf(attract_force) / distance
			var force := force_direction * force_magnitude * delta
			char_entity.apply_force(force)

## 在指定位置应用一次性吸引/排斥脉冲
static func apply_pulse(center: Vector2, radius: float, force: float, target_type: int) -> void:
	if is_zero_approx(force):
		return
	var tree := Engine.get_main_loop() as SceneTree
	if tree == null:
		return
	for node in tree.get_nodes_in_group("combat_entities"):
		if node is CharEntity:
			var char_entity := node as CharEntity
			if char_entity.entity_type != target_type:
				continue
			if not char_entity.is_alive():
				continue
			var to_entity := char_entity.global_position - center
			var distance := to_entity.length()
			if distance < 0.1 or distance > radius:
				continue
			var force_direction: Vector2
			if force > 0:
				force_direction = -to_entity.normalized()
			else:
				force_direction = to_entity.normalized()
			var force_magnitude := absf(force) * (1.0 - distance / radius)
			var impulse := force_direction * force_magnitude
			char_entity.apply_force(impulse)
