class_name CombatEntityPool
extends Node

## 战斗实体对象池

static var instance: CombatEntityPool = null

var _skill_entity_scene: PackedScene
var _char_entity_scene: PackedScene
var _is_initialized: bool = false
var _active_skill_entities: Array[SkillEntity] = []
var _inactive_skill_entities: Array[SkillEntity] = []
var _active_char_entities: Array[CharEntity] = []

var active_count: int:
	get: return _active_skill_entities.size()

var remaining_capacity: int:
	get: return AtomConstants.ENTITY_POOL_CAPACITY - active_count

var is_at_capacity: bool:
	get: return active_count >= AtomConstants.ENTITY_POOL_CAPACITY

func _enter_tree() -> void:
	instance = self

func _exit_tree() -> void:
	if instance == self:
		instance = null

## 初始化对象池
func initialize() -> void:
	if _is_initialized:
		return
	_skill_entity_scene = load(CombatConfig.SKILL_ENTITY_SCENE) as PackedScene
	_char_entity_scene = load(CombatConfig.CHAR_ENTITY_SCENE) as PackedScene
	_is_initialized = true
	print("[CombatEntityPool] 已初始化")

## 预热对象池
func prewarm(count: int) -> void:
	if _skill_entity_scene == null:
		return
	for i in range(count):
		var entity := _skill_entity_scene.instantiate() as SkillEntity
		if entity != null:
			add_child(entity)
			entity.visible = false
			entity.set_process(false)
			_inactive_skill_entities.append(entity)

## 从对象池获取技能实体
func get_skill_entity() -> SkillEntity:
	if not _is_initialized:
		push_error("[CombatEntityPool] 未初始化")
		return null
	if is_at_capacity:
		push_warning("[CombatEntityPool] 已达容量上限")
		return null
	var entity: SkillEntity
	if _inactive_skill_entities.size() > 0:
		entity = _inactive_skill_entities.pop_back()
		entity.visible = true
		entity.set_process(true)
	elif _skill_entity_scene != null:
		entity = _skill_entity_scene.instantiate() as SkillEntity
		if entity != null:
			add_child(entity)
	if entity != null:
		_active_skill_entities.append(entity)
	return entity

## 归还技能实体
func return_skill_entity(entity: SkillEntity) -> void:
	if entity == null:
		return
	entity.reset_state()
	entity.visible = false
	entity.set_process(false)
	_active_skill_entities.erase(entity)
	_inactive_skill_entities.append(entity)

## 回收所有技能实体
func return_all_skill_entities() -> void:
	for entity in _active_skill_entities.duplicate():
		return_skill_entity(entity)

## 获取角色实体
func get_char_entity(p_entity_type: int) -> CharEntity:
	if _char_entity_scene == null:
		push_error("[CombatEntityPool] CharEntity 场景未加载")
		return null
	var entity := _char_entity_scene.instantiate() as CharEntity
	if entity != null:
		add_child(entity)
		entity.initialize(p_entity_type)
		entity.add_to_group("combat_entities")
		_active_char_entities.append(entity)
	return entity

## 归还角色实体
func return_char_entity(entity: CharEntity) -> void:
	if entity == null:
		return
	entity.remove_from_group("combat_entities")
	_active_char_entities.erase(entity)
	entity.queue_free()

## 回收所有角色实体
func return_all_char_entities() -> void:
	for entity in _active_char_entities.duplicate():
		return_char_entity(entity)
