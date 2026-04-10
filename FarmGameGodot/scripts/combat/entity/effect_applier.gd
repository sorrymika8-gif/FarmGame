class_name EffectApplier
extends RefCounted

## 效果应用器 - 将技能原子数据应用到目标角色

## 将技能效果应用到目标
static func apply_effects(data: SkillAtomData, target_entity: CharEntity, source: CharEntity = null) -> void:
	if data == null or target_entity == null or not target_entity.is_alive():
		return
	_apply_direct_hp(data, target_entity)
	_apply_dot_hp(data, target_entity)
	_apply_slow(data, target_entity)
	_apply_silence(data, target_entity)
	_apply_damage_multiplier(data, target_entity)
	_apply_stealth(data, target_entity)
	_apply_stat_modifiers(data, target_entity)

## 仅应用即时伤害/治疗
static func apply_instant_only(data: SkillAtomData, target_entity: CharEntity) -> float:
	if data == null or target_entity == null or not target_entity.is_alive():
		return 0.0
	return _apply_direct_hp(data, target_entity)

static func _apply_direct_hp(data: SkillAtomData, target_entity: CharEntity) -> float:
	if is_zero_approx(data.direct_hp):
		return 0.0
	if data.direct_hp < 0:
		return target_entity.take_damage(-data.direct_hp)
	else:
		return -target_entity.heal(data.direct_hp)

static func _apply_dot_hp(data: SkillAtomData, target_entity: CharEntity) -> void:
	if is_zero_approx(data.dot_hp) or data.duration <= 0.0:
		return
	var effect := StatusEffect.new(
		AtomEnums.StatusEffectType.DAMAGE_OVER_TIME,
		data.dot_hp,
		data.duration,
		1.0
	)
	target_entity.apply_status(effect, true)

static func _apply_slow(data: SkillAtomData, target_entity: CharEntity) -> void:
	if data.slow_percent <= 0.0 or data.duration <= 0.0:
		return
	var effect := StatusEffect.new(
		AtomEnums.StatusEffectType.SLOW,
		data.slow_percent,
		data.duration
	)
	target_entity.apply_status(effect)

static func _apply_silence(data: SkillAtomData, target_entity: CharEntity) -> void:
	if data.silence_duration <= 0.0:
		return
	var effect := StatusEffect.new(
		AtomEnums.StatusEffectType.SILENCE,
		1.0,
		data.silence_duration
	)
	target_entity.apply_status(effect)

static func _apply_damage_multiplier(data: SkillAtomData, target_entity: CharEntity) -> void:
	if is_zero_approx(data.damage_multiplier - 1.0) or data.duration <= 0.0:
		return
	var effect_type: int
	var value: float
	if data.damage_multiplier > 1.0:
		effect_type = AtomEnums.StatusEffectType.VULNERABLE
		value = data.damage_multiplier - 1.0
	else:
		effect_type = AtomEnums.StatusEffectType.DAMAGE_REDUCTION
		value = 1.0 - data.damage_multiplier
	var effect := StatusEffect.new(effect_type, value, data.duration)
	target_entity.apply_status(effect)

static func _apply_stealth(data: SkillAtomData, target_entity: CharEntity) -> void:
	if data.stealth_duration <= 0.0:
		return
	var effect := StatusEffect.new(
		AtomEnums.StatusEffectType.STEALTH,
		1.0,
		data.stealth_duration
	)
	target_entity.apply_status(effect)

static func _apply_stat_modifiers(data: SkillAtomData, target_entity: CharEntity) -> void:
	if data.duration <= 0.0:
		return
	if not is_zero_approx(data.move_speed_mod):
		var effect := StatusEffect.new(
			AtomEnums.StatusEffectType.MOVE_SPEED_MOD,
			data.move_speed_mod,
			data.duration
		)
		target_entity.apply_status(effect, true)
	if not is_zero_approx(data.attack_mod):
		var effect := StatusEffect.new(
			AtomEnums.StatusEffectType.ATTACK_MOD,
			data.attack_mod,
			data.duration
		)
		target_entity.apply_status(effect, true)
	if not is_zero_approx(data.defense_mod):
		var effect := StatusEffect.new(
			AtomEnums.StatusEffectType.DEFENSE_MOD,
			data.defense_mod,
			data.duration
		)
		target_entity.apply_status(effect, true)
