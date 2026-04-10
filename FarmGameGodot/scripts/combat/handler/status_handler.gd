class_name StatusHandler
extends RefCounted

## 状态效果处理器 - 辅助创建和管理状态效果

static func create_slow_effect(data: SkillAtomData) -> StatusEffect:
	if data == null or data.slow_percent <= 0 or data.duration <= 0:
		return null
	return StatusEffect.new(AtomEnums.StatusEffectType.SLOW, data.slow_percent, data.duration)

static func create_silence_effect(data: SkillAtomData) -> StatusEffect:
	if data == null or data.silence_duration <= 0:
		return null
	return StatusEffect.new(AtomEnums.StatusEffectType.SILENCE, 1.0, data.silence_duration)

static func create_dot_effect(data: SkillAtomData, tick_interval: float = 1.0) -> StatusEffect:
	if data == null or is_zero_approx(data.dot_hp) or data.duration <= 0:
		return null
	return StatusEffect.new(AtomEnums.StatusEffectType.DAMAGE_OVER_TIME, data.dot_hp, data.duration, tick_interval)

static func create_stealth_effect(data: SkillAtomData) -> StatusEffect:
	if data == null or data.stealth_duration <= 0:
		return null
	return StatusEffect.new(AtomEnums.StatusEffectType.STEALTH, 1.0, data.stealth_duration)

static func create_move_speed_effect(percent_change: float, p_duration: float) -> StatusEffect:
	if p_duration <= 0 or is_zero_approx(percent_change):
		return null
	return StatusEffect.new(AtomEnums.StatusEffectType.MOVE_SPEED_MOD, percent_change, p_duration)

static func create_attack_effect(percent_change: float, p_duration: float) -> StatusEffect:
	if p_duration <= 0 or is_zero_approx(percent_change):
		return null
	return StatusEffect.new(AtomEnums.StatusEffectType.ATTACK_MOD, percent_change, p_duration)

static func create_defense_effect(percent_change: float, p_duration: float) -> StatusEffect:
	if p_duration <= 0 or is_zero_approx(percent_change):
		return null
	return StatusEffect.new(AtomEnums.StatusEffectType.DEFENSE_MOD, percent_change, p_duration)

static func create_vulnerable_effect(multiplier: float, p_duration: float) -> StatusEffect:
	if p_duration <= 0 or multiplier <= 0:
		return null
	return StatusEffect.new(AtomEnums.StatusEffectType.VULNERABLE, multiplier, p_duration)

static func create_damage_reduction_effect(reduction: float, p_duration: float) -> StatusEffect:
	if p_duration <= 0 or reduction <= 0:
		return null
	return StatusEffect.new(AtomEnums.StatusEffectType.DAMAGE_REDUCTION, reduction, p_duration)

## 批量应用状态效果到目标
static func apply_effects_to(target_entity: CharEntity, effects: Array, stackable: bool = false) -> void:
	if target_entity == null or effects.is_empty():
		return
	for effect in effects:
		if effect != null:
			target_entity.apply_status(effect, stackable)
