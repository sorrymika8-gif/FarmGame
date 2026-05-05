## 农场Tilemap视图组件
## 负责管理土地和作物的TileMap渲染
## 对应 Unity 的 FarmTilemapView
class_name FarmTilemapView
extends Node2D

## Tilemap 引用
@export var soil_tilemap: TileMapLayer = null
@export var plant_tilemap: TileMapLayer = null
@export var highlight_tilemap: TileMapLayer = null

## Tile Atlas 坐标配置
@export var tilled_tile_atlas: Vector2i = Vector2i(0, 0)
@export var highlight_tile_atlas: Vector2i = Vector2i(1, 0)

const TILLED_TILE_TEXTURE_PATH = "res://assets/sprites/farm/tile_tilled_clean.svg"
const UNTILLED_TILE_TEXTURE_PATH = "res://assets/sprites/farm/tile_untilled_clean.svg"

var _map_data: FarmMapData = null
var _soil_cache: Dictionary = {} # Vector2i -> SoilEntity
var _soil_views: Dictionary = {} # Vector2i -> Sprite2D
var _crop_views: Dictionary = {} # Vector2i -> CropView (Node2D)
var _current_highlight: Vector2i = Vector2i(-9999, -9999)
var _is_initialized: bool = false
var _tilled_texture: Texture2D = null
var _untilled_texture: Texture2D = null

func _ready() -> void:
	add_to_group("farm_view")

func _exit_tree() -> void:
	# 取消订阅
	if _map_data:
		for soil in _map_data.get_all_soils():
			if soil.state_changed.is_connected(_on_soil_state_changed):
				soil.state_changed.disconnect(_on_soil_state_changed)
	
	# 清理 CropView
	for crop_view in _crop_views.values():
		if is_instance_valid(crop_view):
			crop_view.queue_free()
	_crop_views.clear()

	for soil_view in _soil_views.values():
		if is_instance_valid(soil_view):
			soil_view.queue_free()
	_soil_views.clear()

## 初始化视图
func initialize(map_data: FarmMapData) -> void:
	if _is_initialized:
		push_warning("[FarmTilemapView] 已经初始化过了")
		return
	
	if map_data == null:
		push_error("[FarmTilemapView] map_data 为空!")
		return
	
	_map_data = map_data
	_soil_cache.clear()
	_load_tile_textures()
	
	for soil in _map_data.get_all_soils():
		_soil_cache[soil.grid_pos] = soil
		soil.state_changed.connect(_on_soil_state_changed)
		_update_soil_tile(soil)
		_update_plant_tile(soil)
	
	_is_initialized = true
	print("[FarmTilemapView] 初始化完成，共 %d 块土地" % _soil_cache.size())

## 根据世界坐标获取土地实体
func get_soil_at_world_pos(world_pos: Vector2) -> SoilEntity:
	var grid_pos = world_to_grid(world_pos)
	return _soil_cache.get(grid_pos)

## 根据网格坐标获取土地实体
func get_soil_at_grid_pos(grid_pos: Vector2i) -> SoilEntity:
	return _soil_cache.get(grid_pos)

## 世界坐标转网格坐标
func world_to_grid(world_pos: Vector2) -> Vector2i:
	if soil_tilemap == null or soil_tilemap.tile_set == null:
		return MapManager.world_to_grid(world_pos)
	var cell_pos = soil_tilemap.local_to_map(soil_tilemap.to_local(world_pos))
	return Vector2i(cell_pos.x, cell_pos.y)

## 网格坐标转世界坐标
func grid_to_world(grid_pos: Vector2i) -> Vector2:
	if soil_tilemap == null or soil_tilemap.tile_set == null:
		return MapManager.grid_to_world(grid_pos)
	var cell_pos = Vector2i(grid_pos.x, grid_pos.y)
	return soil_tilemap.to_global(soil_tilemap.map_to_local(cell_pos))

## 设置高亮显示
func set_highlight(grid_pos: Vector2i) -> void:
	if highlight_tilemap == null:
		return
	clear_highlight()
	if not _soil_cache.has(grid_pos):
		return
	highlight_tilemap.set_cell(Vector2i(grid_pos.x, grid_pos.y), 0, highlight_tile_atlas)
	_current_highlight = grid_pos

## 清除高亮显示
func clear_highlight() -> void:
	if highlight_tilemap == null:
		return
	if _current_highlight != Vector2i(-9999, -9999):
		highlight_tilemap.erase_cell(_current_highlight)
		_current_highlight = Vector2i(-9999, -9999)

## 检查指定位置是否是有效的农田
func is_valid_farmland(world_pos: Vector2) -> bool:
	return get_soil_at_world_pos(world_pos) != null

## 土地状态变化回调
func _on_soil_state_changed(soil: SoilEntity) -> void:
	_update_soil_tile(soil)
	_update_plant_tile(soil)

## 更新土地Tile
func _update_soil_tile(soil: SoilEntity) -> void:
	if soil_tilemap != null and soil_tilemap.tile_set != null:
		soil_tilemap.set_cell(soil.grid_pos, 0, tilled_tile_atlas)
	_update_soil_sprite(soil)

func _load_tile_textures() -> void:
	_tilled_texture = load(TILLED_TILE_TEXTURE_PATH) as Texture2D
	_untilled_texture = load(UNTILLED_TILE_TEXTURE_PATH) as Texture2D

func _update_soil_sprite(soil: SoilEntity) -> void:
	var grid_pos = soil.grid_pos
	var soil_view = _soil_views.get(grid_pos) as Sprite2D
	if soil_view == null or not is_instance_valid(soil_view):
		soil_view = Sprite2D.new()
		soil_view.name = "SoilTile_%d_%d" % [grid_pos.x, grid_pos.y]
		soil_view.z_index = 0
		add_child(soil_view)
		_soil_views[grid_pos] = soil_view

	soil_view.texture = _tilled_texture if soil.is_tilled else _untilled_texture
	soil_view.position = to_local(grid_to_world(grid_pos))
	_fit_sprite_to_tile(soil_view)

func _fit_sprite_to_tile(sprite: Sprite2D) -> void:
	if sprite.texture == null:
		return
	var texture_size = sprite.texture.get_size()
	if texture_size.x <= 0.0 or texture_size.y <= 0.0:
		return
	var tile_size = MapManager.tile_size
	sprite.scale = Vector2(tile_size / texture_size.x, tile_size / texture_size.y)

## 更新作物显示
func _update_plant_tile(soil: SoilEntity) -> void:
	var grid_pos = soil.grid_pos
	
	if soil.has_plant:
		if not _crop_views.has(grid_pos):
			# 创建作物视图
			var crop_view = CropView.new()
			crop_view.name = "CropView_%d_%d" % [grid_pos.x, grid_pos.y]
			crop_view.position = to_local(grid_to_world(grid_pos))
			add_child(crop_view)
			crop_view.bind(soil)
			_crop_views[grid_pos] = crop_view
		else:
			_crop_views[grid_pos].bind(soil)
	else:
		if _crop_views.has(grid_pos):
			var crop_view = _crop_views[grid_pos]
			if is_instance_valid(crop_view):
				crop_view.queue_free()
			_crop_views.erase(grid_pos)
	
	# 清除Tilemap上的植物Tile
	if plant_tilemap:
		plant_tilemap.erase_cell(grid_pos)

## 获取指定位置的作物视图
func get_crop_view_at(grid_pos: Vector2i) -> CropView:
	return _crop_views.get(grid_pos)
