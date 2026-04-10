class_name SpawnScheduler
extends Node

## 生成调度器 - 每帧从队列中取出请求并生成实体

var _is_initialized: bool = false
var _is_paused: bool = false
var _spawned_this_frame: int = 0
var _max_per_frame: int = AtomConstants.MAX_SPAWN_PER_FRAME
var _adaptive_floor: int = AtomConstants.ADAPTIVE_SPAWN_FLOOR
var _adaptive_ceiling: int = AtomConstants.ADAPTIVE_SPAWN_CEILING
var _fps_threshold: float = AtomConstants.FPS_ADAPTIVE_THRESHOLD
var current_budget: int = 0

## 初始化调度器
func initialize() -> void:
	if _is_initialized:
		return
	_is_initialized = true
	_is_paused = false
	print("[SpawnScheduler] 已初始化")

func _process(delta: float) -> void:
	if not _is_initialized or _is_paused:
		return
	# 处理延迟请求
	SpawnQueue.instance.process_delayed_requests()
	_spawned_this_frame = 0
	# 计算本帧预算
	_calculate_budget(delta)
	# 计算实际可生成数量
	var pool_remaining := AtomConstants.ENTITY_POOL_CAPACITY
	if CombatEntityPool.instance != null:
		pool_remaining = AtomConstants.ENTITY_POOL_CAPACITY - CombatEntityPool.instance.active_count
	var spawn_count := mini(mini(SpawnQueue.instance.count, current_budget), pool_remaining)
	# 生成实体
	for i in range(spawn_count):
		var request := SpawnQueue.instance.try_dequeue()
		if request == null:
			break
		_spawn_entity(request)
		_spawned_this_frame += 1

func _calculate_budget(delta: float) -> void:
	var fps := 1.0 / delta if delta > 0 else 60.0
	if fps > _fps_threshold:
		current_budget = _adaptive_ceiling
	elif fps < _fps_threshold * 0.5:
		current_budget = _adaptive_floor
	else:
		current_budget = _max_per_frame

func _spawn_entity(request: SpawnRequest) -> void:
	if request.data == null:
		push_warning("[SpawnScheduler] SpawnRequest 数据为空，跳过")
		return
	var entity := CombatEntityPool.instance.get_skill_entity() if CombatEntityPool.instance != null else null
	if entity == null:
		push_warning("[SpawnScheduler] 无法从对象池获取 SkillEntity")
		return
	entity.global_position = request.position
	entity.rotation = request.rotation_angle
	entity.init_skill(request.data, request.owner)

## 暂停调度
func pause() -> void:
	_is_paused = true

## 恢复调度
func resume() -> void:
	_is_paused = false

## 设置每帧上限
func set_max_per_frame(max_val: int) -> void:
	_max_per_frame = maxi(1, max_val)
