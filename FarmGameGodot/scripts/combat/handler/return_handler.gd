class_name ReturnHandler
extends RefCounted

## 返回处理器 - 回旋镖效果

const RETURN_TRIGGER_DISTANCE: float = 15.0 * 64.0
const RETURN_ARRIVE_DISTANCE: float = 0.5 * 64.0

## 执行返回逻辑
static func execute(entity: SkillEntity) -> void:
	if entity == null or entity.data == null:
		return
	if not entity.data.returning:
		return
	var owner_pos := entity.get_owner_position()
	if not entity.is_returning:
		var distance_from_start := entity.global_position.distance_to(entity.start_position)
		if distance_from_start >= RETURN_TRIGGER_DISTANCE:
			_start_return(entity)
	else:
		var distance_to_owner := entity.global_position.distance_to(owner_pos)
		if distance_to_owner <= RETURN_ARRIVE_DISTANCE:
			entity.return_to_pool()
			return
		var to_owner := owner_pos - entity.global_position
		entity.set_direction(to_owner.normalized())

## 开始返回
static func _start_return(entity: SkillEntity) -> void:
	entity.set_returning(true)
	var to_owner := entity.get_owner_position() - entity.global_position
	entity.set_direction(to_owner.normalized())

## 强制开始返回
static func force_return(entity: SkillEntity) -> void:
	if entity == null:
		return
	if not entity.data.returning:
		return
	_start_return(entity)
