class_name EnemyAI
extends Node2D

## 敌人AI控制器 - 基于状态机的简单AI行为

enum AIState { IDLE, CHASE, ATTACK, FLEE, DEAD }

@export var detection_range: float = CombatConfig.ENEMY_DETECTION_RANGE
@export var attack_range: float = CombatConfig.ENEMY_ATTACK_RANGE
@export var attack_cooldown: float = CombatConfig.ENEMY_ATTACK_COOLDOWN
@export var flee_hp_threshold: float = CombatConfig.ENEMY_FLEE_HP_THRESHOLD
@export var idle_wander_radius: float = 3.0 * 64.0
@export var idle_wander_interval: float = 2.0

var char_entity: CharEntity
var current_state: int = AIState.IDLE
var target: CharEntity
var default_skill: SkillAtomData
var _last_attack_time: float = 0.0
var _last_state_update_time: float = 0.0
var _last_wander_time: float = 0.0
var _wander_target: Vector2 = Vector2.ZERO
var _spawn_position: Vector2 = Vector2.ZERO

func _ready() -> void:
	_spawn_position = global_position
	_wander_target = _spawn_position
	if default_skill == null:
		default_skill = _create_default_skill()

func bind(entity: CharEntity) -> void:
	char_entity = entity

func _process(delta: float) -> void:
	if char_entity == null or not char_entity.is_alive():
		_set_state(AIState.DEAD)
		return
	var current_time := Time.get_ticks_msec() / 1000.0
	if current_time - _last_state_update_time < CombatConfig.ENEMY_AI_UPDATE_INTERVAL:
		return
	_last_state_update_time = current_time
	_update_state_machine()

func _update_state_machine() -> void:
	_update_target()
	_check_state_transition()
	_execute_current_state()

func _update_target() -> void:
	target = TrackingHandler.find_nearest_in_range(
		global_position, detection_range, AtomEnums.EntityType.PLAYER
	)

func _check_state_transition() -> void:
	if not char_entity.is_alive():
		_set_state(AIState.DEAD)
		return
	if char_entity.stats.get_hp_percent() < flee_hp_threshold and target != null:
		_set_state(AIState.FLEE)
		return
	if target == null or not target.is_alive():
		_set_state(AIState.IDLE)
		return
	var dist := global_position.distance_to(target.global_position)
	if dist <= attack_range:
		_set_state(AIState.ATTACK)
	elif dist <= detection_range:
		_set_state(AIState.CHASE)
	else:
		_set_state(AIState.IDLE)

func _execute_current_state() -> void:
	match current_state:
		AIState.IDLE: _execute_idle()
		AIState.CHASE: _execute_chase()
		AIState.ATTACK: _execute_attack()
		AIState.FLEE: _execute_flee()
		AIState.DEAD: _execute_dead()

func _set_state(new_state: int) -> void:
	if current_state == new_state:
		return
	current_state = new_state
	match new_state:
		AIState.IDLE: _wander_target = _spawn_position
		AIState.DEAD: char_entity.stop_moving()

func _execute_idle() -> void:
	var current_time := Time.get_ticks_msec() / 1000.0
	if current_time - _last_wander_time > idle_wander_interval:
		_last_wander_time = current_time
		var random_offset := Vector2(randf_range(-1, 1), randf_range(-1, 1)).normalized() * idle_wander_radius
		_wander_target = _spawn_position + random_offset
	var to_target := _wander_target - global_position
	if to_target.length_squared() > 16.0:
		char_entity.set_move_direction(to_target.normalized())
	else:
		char_entity.stop_moving()

func _execute_chase() -> void:
	if target == null:
		return
	var to_target := target.global_position - global_position
	char_entity.set_move_direction(to_target.normalized())

func _execute_attack() -> void:
	if target == null:
		return
	char_entity.stop_moving()
	var current_time := Time.get_ticks_msec() / 1000.0
	if current_time - _last_attack_time < attack_cooldown:
		return
	if char_entity.stats.is_silenced():
		return
	_try_attack()

func _execute_flee() -> void:
	if target == null:
		_set_state(AIState.IDLE)
		return
	var away := global_position - target.global_position
	char_entity.set_move_direction(away.normalized())

func _execute_dead() -> void:
	char_entity.stop_moving()

func _try_attack() -> void:
	if default_skill == null or target == null:
		return
	var to_target := target.global_position - global_position
	var direction := to_target.normalized()
	var angle := direction.angle()
	var request := SpawnRequest.new()
	request.data = default_skill.clone()
	request.position = global_position + direction * 32.0
	request.rotation_angle = angle
	request.owner = char_entity
	SpawnQueue.instance.enqueue(request)
	_last_attack_time = Time.get_ticks_msec() / 1000.0

func _create_default_skill() -> SkillAtomData:
	var skill := SkillAtomData.new()
	skill.display_name = "敌人基础攻击"
	skill.direct_hp = -CombatConfig.DEFAULT_ENEMY_ATTACK
	skill.projectile_speed = 8.0
	skill.shape = AtomEnums.ShapeType.POINT
	skill.target = AtomEnums.TargetType.SINGLE_ENEMY
	skill.trigger = AtomEnums.TriggerType.IMMEDIATE
	return skill

## 设置技能
func set_skill(skill: SkillAtomData) -> void:
	if skill != null:
		default_skill = skill

## 重置AI状态
func reset_ai() -> void:
	current_state = AIState.IDLE
	target = null
	_last_attack_time = 0.0
	_last_state_update_time = 0.0
	_last_wander_time = 0.0
	_spawn_position = global_position
	_wander_target = _spawn_position
