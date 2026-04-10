## 初始地图场景脚本
## 管理初始地图的加载和初始化
extends Node2D

@onready var soil_tilemap: TileMapLayer = $SoilTileMapLayer
@onready var plant_tilemap: TileMapLayer = $PlantTileMapLayer
@onready var highlight_tilemap: TileMapLayer = $HighlightTileMapLayer

var _farm_view: FarmTilemapView = null

func _ready() -> void:
	# 初始化农场视图
	_farm_view = FarmTilemapView.new()
	_farm_view.name = "FarmTilemapView"
	_farm_view.soil_tilemap = soil_tilemap
	_farm_view.plant_tilemap = plant_tilemap
	_farm_view.highlight_tilemap = highlight_tilemap
	add_child(_farm_view)
	
	# 初始化农场数据
	var map_data = FarmManager.get_or_create_map_data("init_map")
	_farm_view.initialize(map_data)
	
	# 注册到 FarmManager
	FarmManager.set_current_view(_farm_view)
	
	# 生成 NPC
	_spawn_npcs()
	
	print("[InitMap] 地图初始化完成")

func _spawn_npcs() -> void:
	var npc_configs = ConfigManager.get_all("npc")
	for cfg in npc_configs:
		NPCManager.spawn_npc_from_config(cfg)
