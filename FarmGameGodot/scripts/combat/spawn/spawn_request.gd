class_name SpawnRequest
extends RefCounted

## 生成请求 - 描述一个待生成的技能实体

var data: SkillAtomData
var position: Vector2 = Vector2.ZERO
var rotation_angle: float = 0.0
var scheduled_time: float = 0.0  ## 计划生成时间
var owner: CharEntity

## 是否已到达计划生成时间
func is_ready() -> bool:
	return Time.get_ticks_msec() / 1000.0 >= scheduled_time

## 距离计划生成时间还有多久
func time_remaining() -> float:
	return maxf(0.0, scheduled_time - Time.get_ticks_msec() / 1000.0)
