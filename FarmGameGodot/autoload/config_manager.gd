## 配置管理器
## 从 CSV/JSON 文件加载游戏配置数据
extends Node

var _is_initialized: bool = false
var _configs: Dictionary = {} # config_name -> Array[Dictionary]
var _config_maps: Dictionary = {} # config_name -> { key_field -> { key_value -> dict } }

func initialize() -> void:
	if _is_initialized:
		return
	_configs = {}
	_config_maps = {}
	_is_initialized = true
	print("[ConfigManager] 初始化完成")

## 加载所有配置文件
func load_all_configs(config_folder: String = "res://configs") -> void:
	var dir = DirAccess.open(config_folder)
	if dir == null:
		push_warning("[ConfigManager] 配置目录不存在: %s" % config_folder)
		return
	
	dir.list_dir_begin()
	var file_name = dir.get_next()
	while file_name != "":
		if not dir.current_is_dir():
			if file_name.ends_with(".json"):
				var config_name = file_name.get_basename()
				_load_json_config(config_folder + "/" + file_name, config_name)
			elif file_name.ends_with(".csv"):
				var config_name = file_name.get_basename()
				_load_csv_config(config_folder + "/" + file_name, config_name)
		file_name = dir.get_next()
	dir.list_dir_end()
	
	print("[ConfigManager] 所有配置加载完成，共 %d 个配置表" % _configs.size())

## 获取配置表的所有记录
func get_all(config_name: String) -> Array:
	if _configs.has(config_name):
		return _configs[config_name]
	push_warning("[ConfigManager] 配置表不存在: %s" % config_name)
	return []

## 根据主键获取单条配置
func get_config(config_name: String, key_value) -> Dictionary:
	var key_field = _get_key_field(config_name)
	var map_key = config_name + "_" + key_field
	var normalized_key = _normalize_lookup_key(key_value)
	
	if _config_maps.has(map_key):
		var map = _config_maps[map_key]
		if map.has(normalized_key):
			return map[normalized_key]
	
	# 如果还没有建立索引，先建一个
	if _configs.has(config_name):
		_build_map(config_name, key_field)
		if _config_maps.has(map_key) and _config_maps[map_key].has(normalized_key):
			return _config_maps[map_key][normalized_key]
	
	return {}

## 获取配置表中按指定字段分组的数据
func get_group(config_name: String, group_field: String, group_value) -> Array:
	var result: Array = []
	var all = get_all(config_name)
	for item in all:
		if item.has(group_field) and item[group_field] == group_value:
			result.append(item)
	return result

## 手动注册配置数据（用于代码中动态创建的配置）
func register_config(config_name: String, data: Array) -> void:
	_configs[config_name] = data

# --- 私有方法 ---

func _load_json_config(file_path: String, config_name: String) -> void:
	var file = FileAccess.open(file_path, FileAccess.READ)
	if file == null:
		push_error("[ConfigManager] 无法打开文件: %s" % file_path)
		return
	
	var json_text = file.get_as_text()
	file.close()
	
	var json = JSON.new()
	var error = json.parse(json_text)
	if error != OK:
		push_error("[ConfigManager] JSON 解析失败: %s, 行 %d" % [json.get_error_message(), json.get_error_line()])
		return
	
	var data = _normalize_json_value(json.data)
	if data is Array:
		_configs[config_name] = data
		print("[ConfigManager] 加载 JSON 配置: %s (%d 条)" % [config_name, data.size()])
	elif data is Dictionary:
		# 如果是字典格式，包装成数组
		_configs[config_name] = [data]
		print("[ConfigManager] 加载 JSON 配置: %s (1 条)" % config_name)

func _load_csv_config(file_path: String, config_name: String) -> void:
	var file = FileAccess.open(file_path, FileAccess.READ)
	if file == null:
		push_error("[ConfigManager] 无法打开文件: %s" % file_path)
		return
	
	var headers: Array = []
	var data: Array = []
	var line_num = 0
	
	while not file.eof_reached():
		var line = file.get_csv_line()
		if line.size() == 0 or (line.size() == 1 and line[0].is_empty()):
			continue
		
		if line_num == 0:
			headers = Array(line)
		else:
			var record: Dictionary = {}
			for i in range(mini(headers.size(), line.size())):
				record[headers[i]] = _parse_csv_value(line[i])
			data.append(record)
		line_num += 1
	
	file.close()
	_configs[config_name] = data
	print("[ConfigManager] 加载 CSV 配置: %s (%d 条)" % [config_name, data.size()])

func _parse_csv_value(value: String):
	# 尝试转为整数
	if value.is_valid_int():
		return value.to_int()
	# 尝试转为浮点数
	if value.is_valid_float():
		return value.to_float()
	# 布尔值
	if value.to_lower() == "true":
		return true
	if value.to_lower() == "false":
		return false
	# JSON 数组或对象
	if (value.begins_with("[") and value.ends_with("]")) or \
	   (value.begins_with("{") and value.ends_with("}")):
		var json = JSON.new()
		if json.parse(value) == OK:
			return json.data
	# 默认返回字符串
	return value

func _normalize_json_value(value):
	match typeof(value):
		TYPE_DICTIONARY:
			var normalized: Dictionary = {}
			for key in value.keys():
				normalized[key] = _normalize_json_value(value[key])
			return normalized
		TYPE_ARRAY:
			var normalized: Array = []
			for item in value:
				normalized.append(_normalize_json_value(item))
			return normalized
		TYPE_FLOAT:
			return _normalize_lookup_key(value)
		_:
			return value

func _normalize_lookup_key(value):
	if typeof(value) == TYPE_FLOAT and is_equal_approx(value, float(int(value))):
		return int(value)
	return value

func _get_key_field(config_name: String) -> String:
	# 默认主键字段名
	if _configs.has(config_name) and _configs[config_name].size() > 0:
		var first = _configs[config_name][0] as Dictionary
		if first.has("class_id"):
			return "class_id"
		if first.has("id"):
			return "id"
	return "id"

func _build_map(config_name: String, key_field: String) -> void:
	var map_key = config_name + "_" + key_field
	var map: Dictionary = {}
	for item in _configs[config_name]:
		if item.has(key_field):
			map[_normalize_lookup_key(item[key_field])] = item
	_config_maps[map_key] = map
