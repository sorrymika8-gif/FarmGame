class_name SkillSlot
extends RefCounted

## 技能槽 - 管理单个技能及其冷却

var skill: SkillAtomData
var remaining_cooldown: float = 0.0

var cooldown_progress: float:
	get:
		if skill != null and skill.cooldown > 0:
			return 1.0 - remaining_cooldown / skill.cooldown
		return 1.0

var is_empty: bool:
	get: return skill == null

var is_ready: bool:
	get: return skill != null and remaining_cooldown <= 0.0

func set_skill(p_skill: SkillAtomData) -> void:
	skill = p_skill
	remaining_cooldown = 0.0

func clear() -> void:
	skill = null
	remaining_cooldown = 0.0

func start_cooldown() -> void:
	if skill != null:
		remaining_cooldown = skill.cooldown

func update_cooldown(delta: float) -> void:
	if remaining_cooldown > 0.0:
		remaining_cooldown = maxf(0.0, remaining_cooldown - delta)

func reset_cooldown() -> void:
	remaining_cooldown = 0.0
