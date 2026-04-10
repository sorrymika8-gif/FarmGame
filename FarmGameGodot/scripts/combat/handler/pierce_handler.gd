class_name PierceHandler
extends RefCounted

## 穿透处理器 - 管理穿透计数

## 检查是否可以继续穿透
static func can_pierce(entity: SkillEntity) -> bool:
	if entity == null:
		return false
	return entity.pierce_remaining > 0

## 穿透后是否应该销毁
static func should_destroy_after_hit(entity: SkillEntity) -> bool:
	if entity == null:
		return true
	return entity.pierce_remaining <= 0
