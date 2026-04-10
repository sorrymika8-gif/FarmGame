## 战斗管理器 - 战斗循环、状态机、全局调度
extends Node

enum CombatState { IDLE, PREPARING, COUNTDOWN, FIGHTING, PAUSED, ENDED }
enum CombatResult { NONE, VICTORY, DEFEAT, DRAW }

signal state_changed(new_state: int)
signal combat_ended(result: int)
signal enemy_died(entity: CharEntity)
signal player_damaged(amount: float)

var _is_initialized: bool = false
var _current_state: int = CombatState.IDLE
var _combat_result: int = CombatResult.NONE
var _player_entity: CharEntity
var _enemies: Array[CharEntity] = []
var _combat_timer: float = 0.0

## 子系统节点
var _entity_pool: CombatEntityPool
var _spawn_scheduler: SpawnScheduler

var current_state: int:
	get: return _current_state

var result: int:
	get: return _combat_result

var player_entity: CharEntity:
	get: return _player_entity

var combat_timer: float:
	get: return _combat_timer

var is_in_combat: bool:
	get: return _current_state == CombatState.FIGHTING

func initialize() -> void:
	if _is_initialized:
		return
	# 创建子系统
	_entity_pool = CombatEntityPool.new()
	_entity_pool.name = "CombatEntityPool"
	add_child(_entity_pool)
	_entity_pool.initialize()

	_spawn_scheduler = SpawnScheduler.new()
	_spawn_scheduler.name = "SpawnScheduler"
	add_child(_spawn_scheduler)
	_spawn_scheduler.initialize()

	LLMBridge.instance.initialize()

	_is_initialized = true
	print("[CombatManager] 初始化完成")

func _process(_delta: float) -> void:
	if not _is_initialized:
		return
	if _current_state == CombatState.FIGHTING:
		_update_combat(_delta)

## 开始战斗
func start_combat_async(enemy_count: int = 3) -> void:
	if _current_state != CombatState.IDLE:
		push_warning("[CombatManager] 无法开始战斗，当前状态: %d" % _current_state)
		return
	_set_state(CombatState.PREPARING)
	# 准备战斗
	_entity_pool.prewarm(20)
	_player_entity = _create_player_entity()
	for i in range(enemy_count):
		var enemy_entity := _create_enemy_entity(i, enemy_count)
		_enemies.append(enemy_entity)
	# 倒计时
	_set_state(CombatState.COUNTDOWN)
	await get_tree().create_timer(CombatConfig.BATTLE_START_COUNTDOWN).timeout
	# 开始战斗
	_set_state(CombatState.FIGHTING)
	_combat_timer = 0.0
	_combat_result = CombatResult.NONE

## 结束战斗
func end_combat(p_result: int) -> void:
	if _current_state == CombatState.IDLE or _current_state == CombatState.ENDED:
		return
	_combat_result = p_result
	_set_state(CombatState.ENDED)
	_spawn_scheduler.pause()
	SpawnQueue.instance.clear()
	combat_ended.emit(p_result)
	print("[CombatManager] 战斗结束，结果: %d" % p_result)
	# 延迟清理
	await get_tree().create_timer(CombatConfig.BATTLE_END_DELAY).timeout
	_cleanup()
	_set_state(CombatState.IDLE)

## 暂停战斗
func pause_combat() -> void:
	if _current_state != CombatState.FIGHTING:
		return
	_set_state(CombatState.PAUSED)
	_spawn_scheduler.pause()
	get_tree().paused = true

## 恢复战斗
func resume_combat() -> void:
	if _current_state != CombatState.PAUSED:
		return
	_set_state(CombatState.FIGHTING)
	_spawn_scheduler.resume()
	get_tree().paused = false

## 释放技能
func cast_skill(data: SkillAtomData, pos: Vector2, direction: Vector2) -> void:
	if not is_in_combat or data == null:
		return
	var request := SpawnRequest.new()
	request.data = data
	request.position = pos
	request.rotation_angle = direction.angle()
	request.scheduled_time = Time.get_ticks_msec() / 1000.0 + data.delay
	request.owner = _player_entity
	SpawnQueue.instance.enqueue(request)

func _update_combat(delta: float) -> void:
	_combat_timer += delta
	_check_combat_conditions()

func _check_combat_conditions() -> void:
	if _player_entity != null and not _player_entity.is_alive():
		end_combat(CombatResult.DEFEAT)
		return
	var all_dead := true
	for enemy_entity in _enemies:
		if enemy_entity != null and enemy_entity.is_alive():
			all_dead = false
			break
	if all_dead and _enemies.size() > 0:
		end_combat(CombatResult.VICTORY)

func _cleanup() -> void:
	_entity_pool.return_all_skill_entities()
	if _player_entity != null:
		_entity_pool.return_char_entity(_player_entity)
		_player_entity = null
	for enemy_entity in _enemies:
		if enemy_entity != null:
			_entity_pool.return_char_entity(enemy_entity)
	_enemies.clear()
	SpawnQueue.instance.clear()

func _create_player_entity() -> CharEntity:
	var entity := _entity_pool.get_char_entity(AtomEnums.EntityType.PLAYER)
	if entity == null:
		return null
	var stats := EntityStats.new()
	stats.max_hp = CombatConfig.DEFAULT_PLAYER_HP
	stats.current_hp = CombatConfig.DEFAULT_PLAYER_HP
	stats.base_attack = CombatConfig.DEFAULT_PLAYER_ATTACK
	stats.base_defense = CombatConfig.DEFAULT_PLAYER_DEFENSE
	stats.base_move_speed = CombatConfig.DEFAULT_PLAYER_MOVE_SPEED
	entity.initialize(AtomEnums.EntityType.PLAYER, stats)
	entity.global_position = Vector2(CombatConfig.PLAYER_SPAWN_X, 0)
	stats.on_hp_changed.append(func(change: float) -> void:
		if change < 0:
			player_damaged.emit(-change)
	)
	return entity

func _create_enemy_entity(index: int, total: int) -> CharEntity:
	var entity := _entity_pool.get_char_entity(AtomEnums.EntityType.ENEMY)
	if entity == null:
		return null
	var stats := EntityStats.new()
	stats.max_hp = CombatConfig.DEFAULT_ENEMY_HP
	stats.current_hp = CombatConfig.DEFAULT_ENEMY_HP
	stats.base_attack = CombatConfig.DEFAULT_ENEMY_ATTACK
	stats.base_defense = CombatConfig.DEFAULT_ENEMY_DEFENSE
	stats.base_move_speed = CombatConfig.DEFAULT_ENEMY_MOVE_SPEED
	entity.initialize(AtomEnums.EntityType.ENEMY, stats)
	var y_spread := CombatConfig.BATTLE_AREA_HEIGHT * 0.6 * 64.0
	var y_offset := ((float(index) / maxf(total - 1, 1)) - 0.5) * y_spread if total > 1 else 0.0
	entity.global_position = Vector2(CombatConfig.ENEMY_SPAWN_X, y_offset)
	stats.on_death.append(func() -> void:
		enemy_died.emit(entity)
	)
	return entity

func _set_state(new_state: int) -> void:
	if _current_state == new_state:
		return
	_current_state = new_state
	state_changed.emit(new_state)
