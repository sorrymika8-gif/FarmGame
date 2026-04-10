class_name BounceHandler
extends RefCounted

## 弹射处理器 - 命中后弹向下一个目标

const BOUNCE_SEARCH_RANGE: float = 10.0 * 64.0

## 执行弹射逻辑，返回是否成功弹射
static func execute(entity: SkillEntity, hit_target: CharEntity) -> bool:
	if entity == null or entity.data == null:
		return false
	if entity.bounce_remaining <= 0:
		return false
	var target_type := AtomEnums.EntityType.ENEMY
	if entity.owner_entity != null:
		target_type = AtomEnums.EntityType.ENEMY if entity.owner_entity.entity_type == AtomEnums.EntityType.PLAYER else AtomEnums.EntityType.PLAYER
	var next_target := TrackingHandler.find_nearest_in_range(
		entity.global_position,
		BOUNCE_SEARCH_RANGE,
		target_type,
		hit_target
	)
	if next_target == null:
		return false
	var to_next := next_target.global_position - entity.global_position
	entity.set_direction(to_next.normalized())
	return true
