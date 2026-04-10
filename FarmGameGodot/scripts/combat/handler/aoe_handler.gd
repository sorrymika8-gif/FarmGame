class_name AOEHandler
extends RefCounted

## AOE 处理器 - 范围效果处理

## 执行 AOE 效果
static func execute(entity: SkillEntity, center: Vector2) -> void:
	if entity == null or entity.data == null:
		return
	if entity.data.aoe_radius <= 0:
		return
	var target_type := AtomEnums.EntityType.ENEMY
	if entity.owner_entity != null:
		target_type = AtomEnums.EntityType.ENEMY if entity.owner_entity.entity_type == AtomEnums.EntityType.PLAYER else AtomEnums.EntityType.PLAYER
	apply_aoe(entity.data, center, target_type, entity.owner_entity)

## 在指定位置应用 AOE 效果
static func apply_aoe(data: SkillAtomData, center: Vector2, target_type: int, source: CharEntity = null) -> void:
	if data == null or data.aoe_radius <= 0:
		return
	var targets := get_targets_in_area(center, data.aoe_radius * 64.0, target_type)
	for target_entity in targets:
		if target_entity == null or not target_entity.is_alive():
			continue
		EffectApplier.apply_effects(data, target_entity, source)

## 获取区域内的所有目标
static func get_targets_in_area(center: Vector2, radius: float, target_type: int) -> Array[CharEntity]:
	var result: Array[CharEntity] = []
	var tree := Engine.get_main_loop() as SceneTree
	if tree == null:
		return result
	for node in tree.get_nodes_in_group("combat_entities"):
		if node is CharEntity:
			var char_entity := node as CharEntity
			if char_entity.entity_type != target_type:
				continue
			if not char_entity.is_alive():
				continue
			var dist := center.distance_to(char_entity.global_position)
			if dist <= radius:
				result.append(char_entity)
	return result

## 获取区域内目标数量
static func count_targets_in_area(center: Vector2, radius: float, target_type: int) -> int:
	return get_targets_in_area(center, radius, target_type).size()
