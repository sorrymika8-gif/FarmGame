class_name EntityStats
extends RefCounted

## 实体属性容器 - 管理角色的基础属性和状态效果

# 基础属性
var max_hp: float = 100.0
var current_hp: float = 100.0
var base_move_speed: float = 5.0
var base_attack: float = 10.0
var base_defense: float = 5.0
var crit_rate: float = 0.05
var crit_damage: float = 0.5
var level: int = 1

# 状态效果列表
var _active_effects: Array[StatusEffect] = []

# 信号式回调（用Array[Callable]模拟）
var on_effect_tick: Array[Callable] = []
var on_effect_expired: Array[Callable] = []
var on_hp_changed: Array[Callable] = []
var on_death: Array[Callable] = []

## 计算后属性
func get_move_speed() -> float:
	var speed := base_move_speed
	speed *= (1.0 + get_effect_value(AtomEnums.StatusEffectType.MOVE_SPEED_MOD) / 100.0)
	speed *= (1.0 - get_effect_value(AtomEnums.StatusEffectType.SLOW) / 100.0)
	return maxf(0.0, speed)

func get_attack() -> float:
	var attack := base_attack
	attack *= (1.0 + get_effect_value(AtomEnums.StatusEffectType.ATTACK_MOD) / 100.0)
	return maxf(0.0, attack)

func get_defense() -> float:
	var defense := base_defense
	defense *= (1.0 + get_effect_value(AtomEnums.StatusEffectType.DEFENSE_MOD) / 100.0)
	return maxf(0.0, defense)

func get_crit_rate() -> float:
	return clampf(crit_rate, 0.0, 1.0)

func get_crit_damage() -> float:
	return maxf(crit_damage, 0.0)

func get_defense_multiplier_against(attacker_level: int = 1) -> float:
	var atk_level := maxi(attacker_level, 1)
	var def_level := maxi(level, 1)
	var defense := get_defense()
	return float(atk_level + 100) / float((atk_level + 100) + (def_level + 100) * defense / 100.0)

func is_silenced() -> bool:
	return has_effect(AtomEnums.StatusEffectType.SILENCE)

func is_stealthed() -> bool:
	return has_effect(AtomEnums.StatusEffectType.STEALTH)

func get_damage_multiplier() -> float:
	var multiplier := 1.0
	multiplier += get_effect_value(AtomEnums.StatusEffectType.VULNERABLE)
	multiplier -= get_effect_value(AtomEnums.StatusEffectType.DAMAGE_REDUCTION)
	return maxf(0.0, multiplier)

func get_hp_percent() -> float:
	return current_hp / max_hp if max_hp > 0 else 0.0

func is_alive() -> bool:
	return current_hp > 0.0

## 每帧更新状态效果
func tick(delta_time: float) -> void:
	for i in range(_active_effects.size() - 1, -1, -1):
		var effect := _active_effects[i]
		if effect.tick(delta_time):
			for cb in on_effect_tick:
				cb.call(effect)
			if effect.effect_type == AtomEnums.StatusEffectType.DAMAGE_OVER_TIME:
				apply_hp_change(effect.value)
		if effect.is_expired():
			for cb in on_effect_expired:
				cb.call(effect)
			_active_effects.remove_at(i)

## 添加状态效果
func apply_effect(effect: StatusEffect, stackable: bool = false) -> void:
	if effect == null:
		return
	var existing: StatusEffect = null
	for e in _active_effects:
		if e.effect_type == effect.effect_type:
			existing = e
			break
	if existing != null:
		if stackable:
			existing.stack(effect.value)
			existing.refresh(effect.remaining_duration)
		else:
			existing.refresh(maxf(existing.remaining_duration, effect.remaining_duration))
			existing.value = maxf(existing.value, effect.value)
	else:
		_active_effects.append(effect)

## 移除指定类型的状态效果
func remove_effect(effect_type: int) -> bool:
	var removed := false
	for i in range(_active_effects.size() - 1, -1, -1):
		if _active_effects[i].effect_type == effect_type:
			_active_effects.remove_at(i)
			removed = true
	return removed

## 移除所有状态效果
func clear_all_effects() -> void:
	_active_effects.clear()

## 检查是否有指定类型的状态效果
func has_effect(effect_type: int) -> bool:
	for e in _active_effects:
		if e.effect_type == effect_type:
			return true
	return false

## 获取指定类型状态效果的总数值
func get_effect_value(effect_type: int) -> float:
	var total := 0.0
	for e in _active_effects:
		if e.effect_type == effect_type:
			total += e.value
	return total

## 应用生命值变化
func apply_hp_change(amount: float) -> void:
	if not is_alive() and amount < 0:
		return
	var previous_hp := current_hp
	current_hp = clampf(current_hp + amount, 0.0, max_hp)
	var actual_change := current_hp - previous_hp
	if absf(actual_change) > 0.001:
		for cb in on_hp_changed:
			cb.call(actual_change)
	if previous_hp > 0.0 and current_hp <= 0.0:
		for cb in on_death:
			cb.call()

## 重置属性到初始状态
func reset() -> void:
	current_hp = max_hp
	clear_all_effects()
