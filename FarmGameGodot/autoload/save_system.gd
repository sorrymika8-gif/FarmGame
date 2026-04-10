## 存档系统
## 负责游戏数据的保存和加载
extends Node

const SAVE_DIR = "user://saves/"
const MAX_SAVE_SLOTS = 5

var _is_initialized: bool = false

signal save_completed(slot_index: int)
signal load_completed(slot_index: int)

func initialize() -> void:
	if _is_initialized:
		return
	
	# 确保存档目录存在
	if not DirAccess.dir_exists_absolute(SAVE_DIR):
		DirAccess.make_dir_absolute(SAVE_DIR)
	
	_is_initialized = true
	print("[SaveSystem] 初始化完成")

## 保存游戏到指定槽位
func save_game(slot_index: int) -> bool:
	if not _is_initialized:
		push_error("[SaveSystem] 未初始化")
		return false
	
	var save_data = _collect_save_data()
	var file_path = _get_save_path(slot_index)
	
	var file = FileAccess.open(file_path, FileAccess.WRITE)
	if file == null:
		push_error("[SaveSystem] 无法创建存档文件: %s" % file_path)
		return false
	
	var json_text = JSON.stringify(save_data, "\t")
	file.store_string(json_text)
	file.close()
	
	save_completed.emit(slot_index)
	print("[SaveSystem] 存档保存成功: 槽位 %d" % slot_index)
	return true

## 加载指定槽位的存档
func load_game(slot_index: int) -> bool:
	if not _is_initialized:
		push_error("[SaveSystem] 未初始化")
		return false
	
	var file_path = _get_save_path(slot_index)
	
	if not FileAccess.file_exists(file_path):
		push_warning("[SaveSystem] 存档不存在: %s" % file_path)
		return false
	
	var file = FileAccess.open(file_path, FileAccess.READ)
	if file == null:
		push_error("[SaveSystem] 无法读取存档文件: %s" % file_path)
		return false
	
	var json_text = file.get_as_text()
	file.close()
	
	var json = JSON.new()
	if json.parse(json_text) != OK:
		push_error("[SaveSystem] 存档解析失败")
		return false
	
	var save_data = json.data as Dictionary
	if save_data == null:
		push_error("[SaveSystem] 存档数据格式错误")
		return false
	
	_apply_save_data(save_data)
	
	load_completed.emit(slot_index)
	print("[SaveSystem] 存档加载成功: 槽位 %d" % slot_index)
	return true

## 检查指定槽位是否有存档
func has_save(slot_index: int) -> bool:
	return FileAccess.file_exists(_get_save_path(slot_index))

## 删除指定槽位的存档
func delete_save(slot_index: int) -> bool:
	var file_path = _get_save_path(slot_index)
	if FileAccess.file_exists(file_path):
		DirAccess.remove_absolute(file_path)
		print("[SaveSystem] 存档已删除: 槽位 %d" % slot_index)
		return true
	return false

## 获取所有存档槽位信息
func get_save_slots_info() -> Array:
	var slots: Array = []
	for i in range(MAX_SAVE_SLOTS):
		var info = {
			"slot_index": i,
			"has_save": has_save(i),
			"save_time": ""
		}
		if info["has_save"]:
			var path = _get_save_path(i)
			info["save_time"] = Time.get_datetime_string_from_unix_time(
				FileAccess.get_modified_time(path)
			)
		slots.append(info)
	return slots

# --- 私有方法 ---

func _get_save_path(slot_index: int) -> String:
	return SAVE_DIR + "save_%d.json" % slot_index

func _collect_save_data() -> Dictionary:
	var data: Dictionary = {
		"version": 1,
		"timestamp": Time.get_unix_time_from_system(),
		"player": {},
		"farm": {},
		"inventory": {},
		"gold": 0,
	}
	
	# 收集玩家数据
	if PlayerManager._is_initialized and PlayerManager._player != null:
		var player = PlayerManager._player
		data["player"] = {
			"position_x": player.position.x,
			"position_y": player.position.y,
			"is_new_player": false
		}
		data["gold"] = PlayerManager._gold
		
		# 收集背包数据
		var inventory_data: Array = []
		if PlayerManager._player.has_method("get_inventory"):
			var inventory = PlayerManager._player.get_inventory()
			if inventory:
				for item in inventory.get_all_items():
					inventory_data.append({
						"config_id": item.config_id,
						"count": item.count
					})
		data["inventory"] = inventory_data
	
	# 收集农场数据
	if FarmManager._is_initialized and FarmManager._current_map != null:
		var farm_data: Array = []
		for soil in FarmManager._current_map.get_all_soils():
			var soil_data = {
				"grid_x": soil.grid_pos.x,
				"grid_y": soil.grid_pos.y,
				"is_tilled": soil.is_tilled,
				"moisture": soil.moisture,
			}
			if soil.has_plant:
				soil_data["plant"] = {
					"config_id": soil.plant.config_id,
					"current_maturity": soil.plant.current_maturity,
				}
			farm_data.append(soil_data)
		data["farm"] = farm_data
	
	return data

func _apply_save_data(data: Dictionary) -> void:
	# 恢复玩家数据
	if data.has("player"):
		var player_data = data["player"]
		var pos = Vector2(
			player_data.get("position_x", 0),
			player_data.get("position_y", 0)
		)
		PlayerManager.set_player_position(pos)
	
	if data.has("gold"):
		PlayerManager.set_gold(data["gold"])
	
	# 恢复背包数据
	if data.has("inventory"):
		var inventory = PlayerManager.get_player_inventory()
		if inventory:
			inventory.clear()
			for item_data in data["inventory"]:
				inventory.add_item(item_data["config_id"], item_data["count"])
	
	# 恢复农场数据
	if data.has("farm"):
		for soil_data in data["farm"]:
			var pos = Vector2i(soil_data["grid_x"], soil_data["grid_y"])
			var soil = FarmManager.get_soil(pos)
			if soil:
				soil.is_tilled = soil_data.get("is_tilled", false)
				soil.moisture = soil_data.get("moisture", 0.0)
				if soil_data.has("plant"):
					var plant_info = soil_data["plant"]
					var PlantEntity = load("res://scripts/farm/plant_entity.gd")
					var plant = PlantEntity.new(plant_info["config_id"])
					plant.current_maturity = plant_info.get("current_maturity", 0.0)
					soil.plant = plant
