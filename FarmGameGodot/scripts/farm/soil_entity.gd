## 土地实体
## 代表世界中的一块土地，持有状态数据
## 对应 Unity 的 SoilEntity
class_name SoilEntity
extends RefCounted

var _config_id: int = 0
var _grid_pos: Vector2i = Vector2i.ZERO
var _moisture: float = 0.0
var _is_tilled: bool = false
var _plant = null # PlantEntity

signal state_changed(soil)

## 土地配置ID
var config_id: int:
	get:
		return _config_id

## 网格坐标
var grid_pos: Vector2i:
	get:
		return _grid_pos

## 湿度 (0-100)
var moisture: float:
	get:
		return _moisture
	set(value):
		_moisture = clampf(value, 0.0, 100.0)
		state_changed.emit(self)

## 是否已耕地
var is_tilled: bool:
	get:
		return _is_tilled
	set(value):
		if _is_tilled != value:
			_is_tilled = value
			state_changed.emit(self)

## 当前种植的作物
var plant:
	get:
		return _plant
	set(value):
		if _plant != value:
			# 取消订阅旧作物事件
			if _plant and _plant.has_signal("stage_changed"):
				if _plant.stage_changed.is_connected(_on_plant_stage_changed):
					_plant.stage_changed.disconnect(_on_plant_stage_changed)
			
			_plant = value
			
			# 订阅新作物事件
			if _plant and _plant.has_signal("stage_changed"):
				_plant.stage_changed.connect(_on_plant_stage_changed)
			
			state_changed.emit(self)

## 是否有作物
var has_plant: bool:
	get:
		return _plant != null

func _init(p_config_id: int = 0, p_x: int = 0, p_y: int = 0) -> void:
	_config_id = p_config_id
	_grid_pos = Vector2i(p_x, p_y)

func _on_plant_stage_changed(_plant_entity) -> void:
	state_changed.emit(self)
