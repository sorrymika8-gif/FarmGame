## 农场管理器
## 负责农场业务逻辑：耕地、种植、收获、生长循环
extends Node

const GROWTH_TICK_INTERVAL_KEY = "growth_tick_interval"
const DEFAULT_GROWTH_TICK_INTERVAL = 1.0 # 秒
const DEFAULT_FARM_WIDTH = 5
const DEFAULT_FARM_HEIGHT = 4
const DEFAULT_FARM_ORIGIN = Vector2i(0, 0)

## 物品类型枚举（对应 Unity 的 ItemType）
const ITEM_TYPE_SEED = 2

var _is_initialized: bool = false
var _maps: Dictionary = {} # map_id -> FarmMapData
var _current_map = null # FarmMapData
var _growth_tick_interval: float = DEFAULT_GROWTH_TICK_INTERVAL
var _growth_timer: Timer = null

signal soil_state_changed(soil)
signal plant_harvested(soil, items: Array)

## 当前地图
var current_map:
	get:
		return _current_map

func initialize() -> void:
	if _is_initialized:
		return
	
	# 从游戏设置中读取生长周期
	var game_settings = ConfigManager.get_all("game_settings")
	for setting in game_settings:
		if setting.get("key", "") == GROWTH_TICK_INTERVAL_KEY:
			_growth_tick_interval = float(setting.get("value", DEFAULT_GROWTH_TICK_INTERVAL)) / 1000.0
			break
	
	print("[FarmManager] 生长周期间隔: %ss" % str(_growth_tick_interval))
	
	# 创建默认农场地图
	var FarmMapDataScript = load("res://scripts/farm/farm_map_data.gd")
	var SoilEntityScript = load("res://scripts/farm/soil_entity.gd")
	
	var default_map = FarmMapDataScript.new("Main")
	var soil_id = 1
	for y in range(DEFAULT_FARM_HEIGHT):
		for x in range(DEFAULT_FARM_WIDTH):
			var grid_x = DEFAULT_FARM_ORIGIN.x + x
			var grid_y = DEFAULT_FARM_ORIGIN.y + y
			var soil = SoilEntityScript.new(soil_id, grid_x, grid_y)
			soil.is_tilled = true
			default_map.add_soil(soil)
			soil.state_changed.connect(_on_soil_state_changed)
			soil_id += 1
	
	register_map(default_map)
	_current_map = default_map
	
	_is_initialized = true
	
	# 启动生长循环
	_start_growth_loop()
	
	# 订阅下雨事件
	if WeatherManager.has_signal("rain_loop"):
		WeatherManager.rain_loop.connect(_on_rain_received)
	
	print("[FarmManager] 初始化完成，默认地图 (%d 块土地)" % default_map.count())

## 注册地图
func register_map(map) -> void:
	if _maps.has(map.map_id):
		return
	_maps[map.map_id] = map

## 获取或创建地图数据
func get_or_create_map_data(map_id: String) -> FarmMapData:
	if _maps.has(map_id):
		return _maps[map_id]
	
	# 创建新的地图数据
	var new_map = FarmMapData.new(map_id)
	var soil_id = 1
	for y in range(DEFAULT_FARM_HEIGHT):
		for x in range(DEFAULT_FARM_WIDTH):
			var grid_x = DEFAULT_FARM_ORIGIN.x + x
			var grid_y = DEFAULT_FARM_ORIGIN.y + y
			var soil = SoilEntity.new(soil_id, grid_x, grid_y)
			soil.is_tilled = true
			new_map.add_soil(soil)
			soil.state_changed.connect(_on_soil_state_changed)
			soil_id += 1
	
	register_map(new_map)
	return new_map

## 设置当前视图
var _current_view: FarmTilemapView = null
func set_current_view(view: FarmTilemapView) -> void:
	_current_view = view

## 获取当前视图
func get_current_view() -> FarmTilemapView:
	return _current_view

## 获取土地
func get_soil(pos: Vector2i):
	if _current_map:
		return _current_map.get_soil(pos)
	return null

## 耕地
func till(soil) -> bool:
	if soil == null:
		return false
	if soil.is_tilled:
		return false
	soil.is_tilled = true
	print("[FarmManager] 土地已耕地: %s" % str(soil.grid_pos))
	return true

## 种植（别名）
func plant_seed(soil, item_id: int, inventory) -> bool:
	return plant(soil, item_id, inventory)

## 种植
func plant(soil, item_id: int, inventory) -> bool:
	if soil == null:
		return false
	
	if soil.has_plant:
		push_warning("[FarmManager] 无法种植: %s 已有作物" % str(soil.grid_pos))
		return false
	
	# 检查物品配置
	var config_info = _get_item_config_info(item_id)
	if config_info.is_empty():
		push_warning("[FarmManager] 物品 %d 配置不存在" % item_id)
		return false
	
	var item_type = config_info.get("item_type", -1)
	if item_type != ITEM_TYPE_SEED:
		push_warning("[FarmManager] 物品 %d 不是种子" % item_id)
		return false
	
	# 扣除种子
	if not inventory.remove_item(item_id, 1):
		return false
	
	# 创建作物
	var PlantEntityScript = load("res://scripts/farm/plant_entity.gd")
	var new_plant = PlantEntityScript.new(item_id)
	soil.plant = new_plant
	
	print("[FarmManager] 种植 %s 在 %s" % [config_info.get("name", ""), str(soil.grid_pos)])
	return true

## 收获
func harvest(soil, inventory) -> bool:
	if soil == null or not soil.has_plant:
		return false
	
	var plant_entity = soil.plant
	if not plant_entity.is_mature:
		push_warning("[FarmManager] 作物未成熟: %s" % str(soil.grid_pos))
		return false
	
	# 计算产出
	var plant_config = _get_seed_config(plant_entity.config_id)
	var harvested_items: Array = []
	
	if plant_config.has("bonus_item"):
		var bonus_items = plant_config["bonus_item"]
		var bonus_amounts = plant_config.get("bonus_amount", [])
		if bonus_items is Array:
			for i in range(bonus_items.size()):
				var reward_item_id = bonus_items[i]
				var reward_count = 1
				if i < bonus_amounts.size():
					reward_count = bonus_amounts[i]
				inventory.add_item(reward_item_id, reward_count)
				harvested_items.append({"item_id": reward_item_id, "count": reward_count})
	
	# 清除作物
	soil.plant = null
	
	plant_harvested.emit(soil, harvested_items)
	print("[FarmManager] 收获: %s" % str(soil.grid_pos))
	return true

# --- 私有方法 ---

func _start_growth_loop() -> void:
	_growth_timer = Timer.new()
	_growth_timer.wait_time = _growth_tick_interval
	_growth_timer.autostart = true
	_growth_timer.timeout.connect(_on_growth_tick)
	add_child(_growth_timer)

func _on_growth_tick() -> void:
	var weather_type = WeatherManager.current_weather if WeatherManager._is_initialized else 0
	var is_rainy = (weather_type == WeatherManager.WeatherType.RAINY) if WeatherManager._is_initialized else false
	var is_sunny = (weather_type == WeatherManager.WeatherType.SUNNY) if WeatherManager._is_initialized else true
	
	for map in _maps.values():
		for soil in map.get_all_soils():
			# 植物生长逻辑
			if soil.has_plant and not soil.plant.is_mature:
				var plant_entity = soil.plant
				var config = _get_seed_config(plant_entity.config_id)
				
				if not config.is_empty():
					var growth_factor: float = config.get("maturity_speed", 1.0)
					
					# 天气与湿度加成
					if soil.moisture < 20.0:
						growth_factor *= 0.3
					elif is_sunny and soil.moisture > 40.0:
						growth_factor *= 1.5
					
					plant_entity.grow(growth_factor)
			
			# 水分蒸发逻辑
			if not is_rainy:
				var evaporation = 2.0 if is_sunny else 0.5
				soil.moisture -= evaporation

func _on_rain_received(rain_amount: float) -> void:
	if _current_map == null:
		return
	for soil in _current_map.get_all_soils():
		if soil.is_tilled:
			soil.moisture += rain_amount

func _on_soil_state_changed(soil) -> void:
	soil_state_changed.emit(soil)

func _get_item_config_info(config_id: int) -> Dictionary:
	var item_config = ConfigManager.get_config("item", config_id)
	if not item_config.is_empty():
		return item_config
	var seed_config = ConfigManager.get_config("seed", config_id)
	if not seed_config.is_empty():
		return seed_config
	return {}

func _get_seed_config(config_id: int) -> Dictionary:
	return ConfigManager.get_config("seed", config_id)

func _notification(what: int) -> void:
	if what == NOTIFICATION_PREDELETE:
		if WeatherManager and WeatherManager.has_signal("rain_loop"):
			if WeatherManager.rain_loop.is_connected(_on_rain_received):
				WeatherManager.rain_loop.disconnect(_on_rain_received)
