## NPC 管理器
## 负责管理所有 NPC 的创建、销毁和查找
extends Node

var _is_initialized: bool = false
var _entities: Dictionary = {} # npc_id -> NPCEntity
var _controllers: Dictionary = {} # npc_id -> NPCController
var _npc_root: Node2D = null

## 共享的 Brain 实例（所有NPC共用）
static var shared_brain: Brain = null

const NPC_PREFAB_DIR = "res://resources/prefabs/npcs/"

signal npc_spawned(npc_id: String, controller: NPCController)
signal npc_removed(npc_id: String)

func initialize() -> void:
	if _is_initialized:
		return
	
	_npc_root = Node2D.new()
	_npc_root.name = "NPCRoot"
	add_child(_npc_root)
	
	# 创建共享 Brain
	shared_brain = BrainFactory.create_unified_brain()
	
	_is_initialized = true
	print("[NpcManager] 初始化完成")

## 注册 NPC 控制器
func register_controller(controller: NPCController) -> void:
	if controller.npc_id.is_empty():
		return
	_controllers[controller.npc_id] = controller

## 注销 NPC 控制器
func unregister_controller(npc_id: String) -> void:
	_controllers.erase(npc_id)

## 获取 NPC 控制器
func get_controller(npc_id: String) -> NPCController:
	return _controllers.get(npc_id)

## 从配置创建并生成 NPC
func spawn_npc_from_config(config: Dictionary) -> NPCController:
	var entity = NPCFactory.create(config, shared_brain)
	var npc_id = entity.id
	_entities[npc_id] = entity
	
	# 加载或创建 NPC 场景
	var model_name = config.get("model_name", npc_id)
	var scene_path = NPC_PREFAB_DIR + model_name + ".tscn"
	
	var controller: NPCController
	if ResourceLoader.exists(scene_path):
		var scene = load(scene_path) as PackedScene
		controller = scene.instantiate() as NPCController
	else:
		# 使用默认的 NPCController
		controller = NPCController.new()
		controller.name = model_name
		# 创建 Sprite
		var sprite = Sprite2D.new()
		sprite.name = "Sprite2D"
		controller.add_child(sprite)
	
	_npc_root.add_child(controller)
	controller.bind(entity)
	
	npc_spawned.emit(npc_id, controller)
	print("[NpcManager] NPC 已生成: %s at %s" % [entity.npc_name, str(entity.position)])
	return controller

## 获取 NPC 实体
func get_entity(npc_id: String) -> NPCEntity:
	return _entities.get(npc_id)

## 移除 NPC
func remove_npc(npc_id: String) -> void:
	if _controllers.has(npc_id):
		var ctrl = _controllers[npc_id]
		if is_instance_valid(ctrl):
			ctrl.queue_free()
		_controllers.erase(npc_id)
	_entities.erase(npc_id)
	npc_removed.emit(npc_id)

## 获取距离最近的 NPC
func get_nearest_npc(pos: Vector2, max_distance: float = INF) -> NPCController:
	var nearest: NPCController = null
	var nearest_dist: float = max_distance
	
	for ctrl in _controllers.values():
		if is_instance_valid(ctrl):
			var dist = pos.distance_to(ctrl.global_position)
			if dist < nearest_dist:
				nearest_dist = dist
				nearest = ctrl
	
	return nearest

## 获取所有 NPC 实体
func get_all_entities() -> Dictionary:
	return _entities.duplicate()

func _notification(what: int) -> void:
	if what == NOTIFICATION_PREDELETE:
		for ctrl in _controllers.values():
			if is_instance_valid(ctrl):
				ctrl.queue_free()
		_controllers.clear()
		_entities.clear()
		if shared_brain:
			shared_brain.dispose()
			shared_brain = null
