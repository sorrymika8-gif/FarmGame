## 农场地图数据
## 一个农场区域中土地实体的集合
## 对应 Unity 的 FarmMapData
class_name FarmMapData
extends RefCounted

var map_id: String = ""
var _soils: Dictionary = {} # Vector2i -> SoilEntity

func _init(p_map_id: String = "") -> void:
	map_id = p_map_id
	_soils = {}

## 添加土地
func add_soil(soil) -> void:
	_soils[soil.grid_pos] = soil

## 获取指定位置的土地
func get_soil(pos: Vector2i):
	if _soils.has(pos):
		return _soils[pos]
	return null

## 获取所有土地
func get_all_soils() -> Array:
	return _soils.values()

## 获取土地数量
func count() -> int:
	return _soils.size()
