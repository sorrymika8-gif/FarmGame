class_name SkillAtomData
extends RefCounted

## 技能原子数据 - 核心数据结构
## 字段直接对应正交原子池，从 LLM 返回的 JSON 一行反序列化

# 弹道行为
var bounce: int = 0            ## 弹射次数
var tracking: float = 0.0      ## 追踪角度（0=不追踪，360=全向追踪）
var pierce: int = 0            ## 穿透目标数
var split: int = 0             ## 分裂数量
var returning: bool = false    ## 是否返回（回旋镖效果）
var attract: float = 0.0       ## 吸引/排斥强度

# 范围规则
var aoe_radius: float = 0.0
var shape: int = AtomEnums.ShapeType.POINT
var projectile_width: float = 0.0

# 数值效果
var direct_hp: float = 0.0       ## 正=治疗，负=伤害
var dot_hp: float = 0.0          ## 每秒持续生命变化
var move_speed_mod: float = 0.0  ## 移速变化百分比
var attack_mod: float = 0.0      ## 攻击力变化百分比
var defense_mod: float = 0.0     ## 防御变化百分比

# 状态效果
var slow_percent: float = 0.0
var silence_duration: float = 0.0
var damage_multiplier: float = 0.0
var stealth_duration: float = 0.0

# 触发条件
var trigger: int = AtomEnums.TriggerType.IMMEDIATE
var target: int = AtomEnums.TargetType.SINGLE_ENEMY

# 时间参数
var delay: float = 0.0
var duration: float = 0.0
var cooldown: float = 1.0

# 元信息
var display_name: String = ""
var projectile_speed: float = 10.0

## 深拷贝
func clone() -> SkillAtomData:
	var c := SkillAtomData.new()
	c.bounce = bounce
	c.tracking = tracking
	c.pierce = pierce
	c.split = split
	c.returning = returning
	c.attract = attract
	c.aoe_radius = aoe_radius
	c.shape = shape
	c.projectile_width = projectile_width
	c.direct_hp = direct_hp
	c.dot_hp = dot_hp
	c.move_speed_mod = move_speed_mod
	c.attack_mod = attack_mod
	c.defense_mod = defense_mod
	c.slow_percent = slow_percent
	c.silence_duration = silence_duration
	c.damage_multiplier = damage_multiplier
	c.stealth_duration = stealth_duration
	c.trigger = trigger
	c.target = target
	c.delay = delay
	c.duration = duration
	c.cooldown = cooldown
	c.display_name = display_name
	c.projectile_speed = projectile_speed
	return c

## 应用数值限制（安全阀）
func clamp_values() -> void:
	bounce = clampi(bounce, 0, AtomConstants.MAX_BOUNCE)
	tracking = clampf(tracking, 0.0, AtomConstants.MAX_TRACKING)
	pierce = clampi(pierce, 0, AtomConstants.MAX_PIERCE)
	split = clampi(split, 0, AtomConstants.MAX_SPLIT)
	attract = clampf(attract, -AtomConstants.MAX_ATTRACT, AtomConstants.MAX_ATTRACT)
	aoe_radius = clampf(aoe_radius, 0.0, AtomConstants.MAX_AOE)
	projectile_width = clampf(projectile_width, 0.0, AtomConstants.MAX_PROJECTILE_WIDTH)
	direct_hp = clampf(direct_hp, -AtomConstants.MAX_DIRECT_HP, AtomConstants.MAX_DIRECT_HP)
	dot_hp = clampf(dot_hp, -AtomConstants.MAX_DOT_HP, AtomConstants.MAX_DOT_HP)
	move_speed_mod = clampf(move_speed_mod, -AtomConstants.MAX_MOVE_SPEED_MOD, AtomConstants.MAX_MOVE_SPEED_MOD)
	attack_mod = clampf(attack_mod, -AtomConstants.MAX_ATTACK_MOD, AtomConstants.MAX_ATTACK_MOD)
	defense_mod = clampf(defense_mod, -AtomConstants.MAX_DEFENSE_MOD, AtomConstants.MAX_DEFENSE_MOD)
	slow_percent = clampf(slow_percent, 0.0, AtomConstants.MAX_SLOW_PERCENT)
	silence_duration = clampf(silence_duration, 0.0, AtomConstants.MAX_SILENCE_DURATION)
	damage_multiplier = clampf(damage_multiplier, 0.0, AtomConstants.MAX_DAMAGE_MULTIPLIER)
	stealth_duration = clampf(stealth_duration, 0.0, AtomConstants.MAX_STEALTH_DURATION)
	delay = clampf(delay, 0.0, AtomConstants.MAX_DELAY)
	duration = clampf(duration, 0.0, AtomConstants.MAX_DURATION)
	cooldown = clampf(cooldown, AtomConstants.MIN_COOLDOWN, AtomConstants.MAX_COOLDOWN)
	if projectile_speed <= 0.0:
		projectile_speed = AtomConstants.DEFAULT_PROJECTILE_SPEED
	projectile_speed = clampf(projectile_speed, AtomConstants.MIN_PROJECTILE_SPEED, AtomConstants.MAX_PROJECTILE_SPEED)

## 从字典创建（用于 JSON 反序列化）
static func from_dict(d: Dictionary) -> SkillAtomData:
	var s := SkillAtomData.new()
	s.bounce = int(d.get("bounce", 0))
	s.tracking = float(d.get("tracking", 0.0))
	s.pierce = int(d.get("pierce", 0))
	s.split = int(d.get("split", 0))
	s.returning = bool(d.get("returning", false))
	s.attract = float(d.get("attract", 0.0))
	s.aoe_radius = float(d.get("aoeRadius", d.get("aoe_radius", 0.0)))
	# shape
	var shape_val = d.get("shape", "Point")
	if shape_val is String:
		match shape_val:
			"Circle": s.shape = AtomEnums.ShapeType.CIRCLE
			"Fan": s.shape = AtomEnums.ShapeType.FAN
			"Line": s.shape = AtomEnums.ShapeType.LINE
			_: s.shape = AtomEnums.ShapeType.POINT
	else:
		s.shape = int(shape_val)
	s.projectile_width = float(d.get("projectileWidth", d.get("projectile_width", 0.0)))
	s.direct_hp = float(d.get("directHP", d.get("direct_hp", 0.0)))
	s.dot_hp = float(d.get("dotHP", d.get("dot_hp", 0.0)))
	s.move_speed_mod = float(d.get("moveSpeedMod", d.get("move_speed_mod", 0.0)))
	s.attack_mod = float(d.get("attackMod", d.get("attack_mod", 0.0)))
	s.defense_mod = float(d.get("defenseMod", d.get("defense_mod", 0.0)))
	s.slow_percent = float(d.get("slowPercent", d.get("slow_percent", 0.0)))
	s.silence_duration = float(d.get("silenceDuration", d.get("silence_duration", 0.0)))
	s.damage_multiplier = float(d.get("damageMultiplier", d.get("damage_multiplier", 0.0)))
	s.stealth_duration = float(d.get("stealthDuration", d.get("stealth_duration", 0.0)))
	# trigger
	var trigger_val = d.get("trigger", "Immediate")
	if trigger_val is String:
		match trigger_val:
			"OnHit": s.trigger = AtomEnums.TriggerType.ON_HIT
			"HPThreshold": s.trigger = AtomEnums.TriggerType.HP_THRESHOLD
			"Interval": s.trigger = AtomEnums.TriggerType.INTERVAL
			"OnKill": s.trigger = AtomEnums.TriggerType.ON_KILL
			_: s.trigger = AtomEnums.TriggerType.IMMEDIATE
	else:
		s.trigger = int(trigger_val)
	# target
	var target_val = d.get("target", "SingleEnemy")
	if target_val is String:
		match target_val:
			"Self": s.target = AtomEnums.TargetType.SELF
			"Area": s.target = AtomEnums.TargetType.AREA
			"AllEnemies": s.target = AtomEnums.TargetType.ALL_ENEMIES
			"Nearest": s.target = AtomEnums.TargetType.NEAREST
			_: s.target = AtomEnums.TargetType.SINGLE_ENEMY
	else:
		s.target = int(target_val)
	s.delay = float(d.get("delay", 0.0))
	s.duration = float(d.get("duration", 0.0))
	s.cooldown = float(d.get("cooldown", 1.0))
	s.display_name = str(d.get("displayName", d.get("display_name", "")))
	s.projectile_speed = float(d.get("projectileSpeed", d.get("projectile_speed", 10.0)))
	s.clamp_values()
	return s
