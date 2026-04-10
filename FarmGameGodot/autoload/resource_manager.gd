## 资源管理器
## 封装 Godot 的资源加载功能，提供资源加载、卸载、预制体对象池管理
extends Node

var _is_initialized: bool = false
var _prefab_pools: Dictionary = {} # path -> Array[PackedScene instances]
var _loaded_resources: Dictionary = {} # path -> Resource

func initialize() -> void:
	if _is_initialized:
		return
	_prefab_pools = {}
	_loaded_resources = {}
	_is_initialized = true
	print("[ResourceManager] 初始化完成")

## 同步加载资源
func load_resource(asset_path: String) -> Resource:
	if not _validate_load(asset_path):
		return null
	
	# 检查缓存
	if _loaded_resources.has(asset_path):
		return _loaded_resources[asset_path]
	
	var full_path = _format_resource_path(asset_path)
	if not ResourceLoader.exists(full_path):
		push_error("[ResourceManager] 资源不存在: %s" % asset_path)
		return null
	
	var resource = load(full_path)
	if resource == null:
		push_error("[ResourceManager] 加载资源失败: %s" % asset_path)
		return null
	
	_loaded_resources[asset_path] = resource
	return resource

## 异步加载资源
func load_resource_async(asset_path: String) -> Resource:
	if not _validate_load(asset_path):
		return null
	
	if _loaded_resources.has(asset_path):
		return _loaded_resources[asset_path]
	
	var full_path = _format_resource_path(asset_path)
	ResourceLoader.load_threaded_request(full_path)
	
	while true:
		var status = ResourceLoader.load_threaded_get_status(full_path)
		if status == ResourceLoader.THREAD_LOAD_LOADED:
			var resource = ResourceLoader.load_threaded_get(full_path)
			_loaded_resources[asset_path] = resource
			return resource
		elif status == ResourceLoader.THREAD_LOAD_FAILED:
			push_error("[ResourceManager] 异步加载资源失败: %s" % asset_path)
			return null
		await get_tree().process_frame
	
	return null

## 从对象池生成场景实例
func spawn(prefab_path: String, parent: Node = null) -> Node:
	if not _validate_load(prefab_path):
		return null
	
	var pool = _get_or_create_pool(prefab_path)
	if pool == null:
		return null
	
	var instance: Node
	if pool.size() > 0:
		instance = pool.pop_back()
	else:
		var packed_scene = load_resource(prefab_path) as PackedScene
		if packed_scene == null:
			return null
		instance = packed_scene.instantiate()
	
	if parent:
		parent.add_child(instance)
	
	if instance is CanvasItem:
		instance.visible = true
	elif instance is Node3D:
		instance.visible = true
	
	return instance

## 从对象池生成场景实例并设置位置
func spawn_at(prefab_path: String, position: Vector2, parent: Node = null) -> Node:
	var instance = spawn(prefab_path, parent)
	if instance and instance is Node2D:
		(instance as Node2D).position = position
	return instance

## 回收实例到对象池
func despawn(prefab_path: String, obj: Node) -> void:
	if obj == null:
		return
	
	if not _prefab_pools.has(prefab_path):
		push_warning("[ResourceManager] 对象池未找到: %s，直接销毁" % prefab_path)
		obj.queue_free()
		return
	
	if obj.get_parent():
		obj.get_parent().remove_child(obj)
	
	if obj is CanvasItem:
		obj.visible = false
	elif obj is Node3D:
		obj.visible = false
	
	_prefab_pools[prefab_path].append(obj)

## 预热对象池
func prewarm_pool(prefab_path: String, count: int) -> void:
	if prefab_path.is_empty() or count <= 0:
		return
	
	var pool = _get_or_create_pool(prefab_path)
	if pool == null:
		return
	
	var packed_scene = load_resource(prefab_path) as PackedScene
	if packed_scene == null:
		return
	
	for i in range(count):
		var instance = packed_scene.instantiate()
		if instance is CanvasItem:
			instance.visible = false
		pool.append(instance)

## 清理指定对象池
func clear_pool(prefab_path: String) -> void:
	if _prefab_pools.has(prefab_path):
		var pool = _prefab_pools[prefab_path]
		for obj in pool:
			if is_instance_valid(obj):
				obj.queue_free()
		_prefab_pools.erase(prefab_path)

## 清理所有对象池
func clear_all_pools() -> void:
	for path in _prefab_pools:
		var pool = _prefab_pools[path]
		for obj in pool:
			if is_instance_valid(obj):
				obj.queue_free()
	_prefab_pools.clear()

## 释放已缓存的资源
func release(asset_path: String) -> void:
	_loaded_resources.erase(asset_path)

## 释放所有已缓存资源
func release_all() -> void:
	_loaded_resources.clear()

# --- 私有方法 ---

func _format_resource_path(asset_path: String) -> String:
	if asset_path.begins_with("res://"):
		return asset_path
	return "res://resources/" + asset_path

func _validate_load(asset_path: String) -> bool:
	if asset_path.is_empty():
		push_error("[ResourceManager] 加载失败: 路径为空")
		return false
	if not _is_initialized:
		push_error("[ResourceManager] 加载失败: 未初始化")
		return false
	return true

func _get_or_create_pool(prefab_path: String) -> Array:
	if _prefab_pools.has(prefab_path):
		return _prefab_pools[prefab_path]
	
	var packed_scene = load_resource(prefab_path) as PackedScene
	if packed_scene == null:
		push_error("[ResourceManager] 创建对象池失败: 加载预制体失败 %s" % prefab_path)
		return []
	
	var pool: Array = []
	_prefab_pools[prefab_path] = pool
	return pool

func _notification(what: int) -> void:
	if what == NOTIFICATION_PREDELETE:
		clear_all_pools()
		_loaded_resources.clear()
