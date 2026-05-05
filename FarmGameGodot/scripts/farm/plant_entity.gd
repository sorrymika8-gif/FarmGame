## 作物运行时实体
## 对应 Unity 的 PlantEntity
class_name PlantEntity
extends RefCounted

var config_id: int = 0
var count: int = 1
var instance_id: String = ""

var _current_maturity: float = 0.0
var _current_stage_index: int = 0
var _is_mature: bool = false

signal stage_changed(plant)

## 当前成熟度
var current_maturity: float:
	get:
		return _current_maturity
	set(value):
		_current_maturity = value
		_update_stage()

## 当前生长阶段索引
var current_stage_index: int:
	get:
		return _current_stage_index

## 是否已成熟
var is_mature: bool:
	get:
		return _is_mature

## 作物配置数据（种子配置）
var plant_data: Dictionary:
	get:
		return ConfigManager.get_config("seed", config_id)

## 成熟进度百分比
var maturity_percent: float:
	get:
		var data = plant_data
		var need_maturity = data.get("need_maturity", 0)
		if need_maturity <= 0:
			return 0.0
		return (_current_maturity / need_maturity) * 100.0

## 生长阶段名称
var stage_name: String:
	get:
		match _current_stage_index:
			0:
				return "幼苗期"
			1:
				return "生长期"
			2:
				return "成熟期"
			_:
				return "未知"

## 作物名称
var plant_name: String:
	get:
		return plant_data.get("name", "未知作物")

func get_description_type() -> String:
	return "Crop"

func get_display_name() -> String:
	return plant_name

func get_cache_key() -> String:
	return "crop_%d_stage_%d_maturity_%d" % [config_id, _current_stage_index, int(maturity_percent)]

func get_describable_properties() -> Dictionary:
	return {
		"Name": plant_name,
		"StageName": stage_name,
		"MaturityPercent": maturity_percent,
		"IsHarvestable": _is_mature,
	}

func _init(p_config_id: int = 0) -> void:
	config_id = p_config_id
	count = 1
	_current_maturity = 0.0
	_current_stage_index = 0
	_is_mature = false
	instance_id = str(randi())
	_update_stage()

## 应用生长周期
func grow(delta: float) -> void:
	if _is_mature:
		return
	
	var data = plant_data
	if data.is_empty():
		return
	
	_current_maturity += delta
	
	var need_maturity = data.get("need_maturity", 100.0)
	if _current_maturity >= need_maturity:
		_current_maturity = need_maturity
		_is_mature = true
	
	_update_stage()

## 更新生长阶段
func _update_stage() -> void:
	var data = plant_data
	if data.is_empty():
		return
	
	var maturity_stages = data.get("maturity_stage", [])
	if not maturity_stages is Array:
		return
	
	var old_stage = _current_stage_index
	var new_stage = 0
	
	for i in range(maturity_stages.size()):
		if _current_maturity >= maturity_stages[i]:
			new_stage = i + 1
		else:
			break
	
	_current_stage_index = new_stage
	
	if old_stage != new_stage:
		stage_changed.emit(self)
