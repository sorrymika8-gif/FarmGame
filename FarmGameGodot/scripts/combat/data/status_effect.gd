class_name StatusEffect
extends RefCounted

## 状态效果数据

var effect_type: int = AtomEnums.StatusEffectType.DAMAGE_OVER_TIME
var value: float = 0.0
var remaining_duration: float = 0.0
var tick_interval: float = 1.0
var time_to_next_tick: float = 1.0
var source_id: String = ""

func _init(
	p_effect_type: int = AtomEnums.StatusEffectType.DAMAGE_OVER_TIME,
	p_value: float = 0.0,
	p_duration: float = 0.0,
	p_tick_interval: float = 1.0,
	p_source_id: String = ""
) -> void:
	effect_type = p_effect_type
	value = p_value
	remaining_duration = p_duration
	tick_interval = p_tick_interval
	time_to_next_tick = p_tick_interval
	if p_source_id.is_empty():
		source_id = str(randi())
	else:
		source_id = p_source_id

## 检查效果是否已过期
func is_expired() -> bool:
	return remaining_duration <= 0.0

## 更新效果时间，返回是否触发了 Tick
func tick(delta_time: float) -> bool:
	remaining_duration -= delta_time
	time_to_next_tick -= delta_time
	if time_to_next_tick <= 0.0:
		time_to_next_tick += tick_interval
		return true
	return false

## 刷新效果持续时间
func refresh(new_duration: float) -> void:
	remaining_duration = new_duration

## 叠加效果数值
func stack(additional_value: float, max_value: float = -1.0) -> void:
	value += additional_value
	if max_value >= 0.0 and value > max_value:
		value = max_value
