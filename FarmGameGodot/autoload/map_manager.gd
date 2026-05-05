## 地图管理器
## 负责地图资源的加载、卸载
extends Node

var _is_initialized: bool = false
var _current_map: Node2D = null
var _map_root: Node2D = null
var _tile_size: float = 32.0

const MAP_DIR = "res://resources/maps/"

## 当前地图实例
var current_map: Node2D:
	get:
		return _current_map

## 地块尺寸
var tile_size: float:
	get:
		return _tile_size
	set(value):
		_tile_size = value

func initialize() -> void:
	if _is_initialized:
		return
	
	_map_root = Node2D.new()
	_map_root.name = "MapRoot"
	add_child(_map_root)
	
	_is_initialized = true
	print("[MapManager] 初始化完成")

## 加载地图
func load_map(map_name: String) -> bool:
	if not _is_initialized:
		push_error("[MapManager] 未初始化")
		return false
	
	# 先卸载当前地图
	unload_map()
	
	# 加载地图场景
	var map_path = MAP_DIR + map_name + ".tscn"
	if not ResourceLoader.exists(map_path):
		push_error("[MapManager] 地图不存在: %s" % map_path)
		return false
	
	var map_scene = load(map_path) as PackedScene
	if map_scene == null:
		push_error("[MapManager] 加载地图失败: %s" % map_name)
		return false
	
	_current_map = map_scene.instantiate() as Node2D
	if _current_map == null:
		push_error("[MapManager] 地图实例化失败: %s" % map_name)
		return false
	
	_current_map.name = map_name
	_map_root.add_child(_current_map)
	
	print("[MapManager] 地图加载成功: %s" % map_name)
	return true

## 卸载当前地图
func unload_map() -> void:
	if _current_map != null:
		_current_map.queue_free()
		_current_map = null

## 世界坐标转网格坐标
func world_to_grid(world_pos: Vector2) -> Vector2i:
	return Vector2i(
		floori(world_pos.x / _tile_size),
		floori(world_pos.y / _tile_size)
	)

## 网格坐标转世界坐标（返回格子中心）
func grid_to_world(grid_pos: Vector2i) -> Vector2:
	return Vector2(
		grid_pos.x * _tile_size + _tile_size / 2.0,
		grid_pos.y * _tile_size + _tile_size / 2.0
	)

func _notification(what: int) -> void:
	if what == NOTIFICATION_PREDELETE:
		unload_map()
