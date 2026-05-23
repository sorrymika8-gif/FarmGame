## 城镇管理器
## 负责城镇地点语义、游戏内时间和 NPC 日程调度
extends Node

const DEFAULT_START_MINUTE = 8 * 60
const MINUTES_PER_DAY = 24 * 60
const SCHEDULE_APPLY_INTERVAL = 2.0
const NEARBY_NPC_DISTANCE = 4.0 * 32.0

var _is_initialized: bool = false
var _locations: Dictionary = {} # location_id -> Dictionary
var _schedules_by_npc: Dictionary = {} # npc_id -> Array[Dictionary]
var _day: int = 1
var _minute_of_day: int = DEFAULT_START_MINUTE
var _time_accumulator: float = 0.0
var _schedule_accumulator: float = 0.0
var _last_period: String = ""
var _last_schedule_keys: Dictionary = {} # npc_id -> location/activity key

## 游戏内每秒流逝的分钟数
var minutes_per_real_second: float = 4.0

signal time_changed(day: int, hour: int, minute: int)
signal period_changed(period: String)
signal npc_schedule_changed(npc_id: String, location_id: String, activity: String)

func initialize() -> void:
	if _is_initialized:
		return
	_load_locations()
	_load_schedules()
	_last_period = get_current_period()
	_is_initialized = true
	set_process(true)
	print("[TownManager] 初始化完成，地点 %d 个，日程 NPC %d 个" % [_locations.size(), _schedules_by_npc.size()])

func reset_state() -> void:
	_day = 1
	_minute_of_day = DEFAULT_START_MINUTE
	_time_accumulator = 0.0
	_schedule_accumulator = 0.0
	_last_period = get_current_period()
	_last_schedule_keys.clear()

func _process(delta: float) -> void:
	if not _is_initialized:
		return
	if GameInitManager == null or not GameInitManager.is_game_running():
		return

	_advance_time(delta)
	_schedule_accumulator += delta
	if _schedule_accumulator >= SCHEDULE_APPLY_INTERVAL:
		_schedule_accumulator = 0.0
		apply_npc_schedules()

func _advance_time(delta: float) -> void:
	_time_accumulator += delta * minutes_per_real_second
	var passed_minutes = int(_time_accumulator)
	if passed_minutes <= 0:
		return

	_time_accumulator -= passed_minutes
	_minute_of_day += passed_minutes
	while _minute_of_day >= MINUTES_PER_DAY:
		_minute_of_day -= MINUTES_PER_DAY
		_day += 1

	var current_period = get_current_period()
	if current_period != _last_period:
		_last_period = current_period
		period_changed.emit(current_period)
		apply_npc_schedules()

	time_changed.emit(_day, get_current_hour(), get_current_minute())

func apply_npc_schedules() -> void:
	if NPCManager == null:
		return

	var entities = NPCManager.get_all_entities()
	for npc_id in entities.keys():
		var entity: NPCEntity = entities[npc_id]
		var schedule = get_active_schedule(str(npc_id))
		if schedule.is_empty():
			continue

		var location_id = str(schedule.get("location_id", ""))
		if not _locations.has(location_id):
			continue

		var activity = str(schedule.get("activity", "idle"))
		var schedule_key = "%s|%s" % [location_id, activity]
		var schedule_changed = _last_schedule_keys.get(str(npc_id), "") != schedule_key
		entity.current_location_id = location_id
		entity.current_activity = activity
		if schedule_changed:
			_last_schedule_keys[str(npc_id)] = schedule_key
			entity.record_memory("我现在在%s，正在%s。" % [get_location_name(location_id), activity])
			npc_schedule_changed.emit(str(npc_id), location_id, activity)

		var controller = NPCManager.get_controller(str(npc_id))
		if controller == null or not is_instance_valid(controller):
			continue

		var target_position = get_location_world_position(location_id)
		if target_position == Vector2.INF:
			continue

		if controller.global_position.distance_to(target_position) > MapManager.tile_size * 0.5:
			controller.move_to(target_position)

func get_active_schedule(npc_id: String) -> Dictionary:
	var schedules = _schedules_by_npc.get(npc_id, [])
	if not (schedules is Array):
		return {}

	var current_period = get_current_period()
	var weather_name = _get_weather_name()
	var fallback: Dictionary = {}

	for schedule in schedules:
		var condition = str(schedule.get("condition", "default"))
		var period = str(schedule.get("period", "any"))
		var period_matches = period == "any" or period == current_period
		if not period_matches:
			continue

		if condition == "rainy" and weather_name == "雨天":
			return schedule
		if condition == "default" and fallback.is_empty():
			fallback = schedule

	return fallback

func build_npc_perception(entity: NPCEntity) -> Dictionary:
	var map_name = _get_current_map_name()
	var npc_location_id = entity.current_location_id
	if npc_location_id.is_empty():
		npc_location_id = get_location_id_for_position(map_name, entity.position)

	var active_schedule = get_active_schedule(entity.id)
	var perception: Dictionary = {
		"Day": _day,
		"Time": get_current_time_label(),
		"TimePeriod": get_current_period_name(),
		"Weather": _get_weather_name(),
		"Location": get_location_name(npc_location_id),
		"LocationDescription": get_location_description(npc_location_id),
		"CurrentScheduleActivity": active_schedule.get("activity", entity.current_activity),
		"CurrentScheduleDestination": get_location_name(str(active_schedule.get("location_id", npc_location_id))),
		"NearbyNPCs": _get_nearby_npc_names(entity),
	}

	var player = PlayerManager.player
	if player != null:
		perception["PlayerDistance"] = int(entity.position.distance_to(player.global_position))
		perception["PlayerLocation"] = get_location_name(get_location_id_for_position(map_name, player.global_position))

	return perception

func get_location(location_id: String) -> Dictionary:
	return _locations.get(location_id, {})

func get_location_name(location_id: String) -> String:
	var location = get_location(location_id)
	if location.is_empty():
		return "未知地点"
	return str(location.get("name", location_id))

func get_location_description(location_id: String) -> String:
	var location = get_location(location_id)
	return str(location.get("description", ""))

func get_location_world_position(location_id: String) -> Vector2:
	var location = get_location(location_id)
	if location.is_empty():
		return Vector2.INF

	var grid_pos = location.get("grid_pos", [])
	if not (grid_pos is Array) or grid_pos.size() < 2:
		return Vector2.INF

	return MapManager.grid_to_world(Vector2i(int(grid_pos[0]), int(grid_pos[1])))

func get_location_id_for_position(map_name: String, world_position: Vector2) -> String:
	var closest_id = ""
	var closest_distance = INF
	for location_id in _locations.keys():
		var location = _locations[location_id]
		if str(location.get("map_name", "")) != map_name:
			continue

		var world_center = get_location_world_position(str(location_id))
		if world_center == Vector2.INF:
			continue

		var radius = float(location.get("radius_tiles", 1)) * MapManager.tile_size
		var distance = world_position.distance_to(world_center)
		if distance <= radius and distance < closest_distance:
			closest_distance = distance
			closest_id = str(location_id)

	return closest_id

func get_current_time_label() -> String:
	return "%02d:%02d" % [get_current_hour(), get_current_minute()]

func get_current_hour() -> int:
	return int(_minute_of_day / 60)

func get_current_minute() -> int:
	return _minute_of_day % 60

func get_current_period() -> String:
	var hour = get_current_hour()
	if hour >= 6 and hour < 12:
		return "morning"
	if hour >= 12 and hour < 17:
		return "afternoon"
	if hour >= 17 and hour < 22:
		return "evening"
	return "night"

func get_current_period_name() -> String:
	match get_current_period():
		"morning":
			return "上午"
		"afternoon":
			return "下午"
		"evening":
			return "傍晚"
		_:
			return "夜晚"

func to_save_data() -> Dictionary:
	return {
		"day": _day,
		"minute_of_day": _minute_of_day,
	}

func load_save_data(data: Dictionary) -> void:
	_day = int(data.get("day", 1))
	_minute_of_day = int(data.get("minute_of_day", DEFAULT_START_MINUTE))
	_time_accumulator = 0.0
	_schedule_accumulator = 0.0
	_last_period = get_current_period()
	_last_schedule_keys.clear()
	apply_npc_schedules()

func _load_locations() -> void:
	_locations.clear()
	var records = ConfigManager.get_all("town_locations")
	for record in records:
		var location_id = str(record.get("id", ""))
		if location_id.is_empty():
			continue
		_locations[location_id] = record

func _load_schedules() -> void:
	_schedules_by_npc.clear()
	var records = ConfigManager.get_all("npc_schedule")
	for record in records:
		var npc_id = str(record.get("npc_id", ""))
		if npc_id.is_empty():
			continue
		if not _schedules_by_npc.has(npc_id):
			_schedules_by_npc[npc_id] = []
		_schedules_by_npc[npc_id].append(record)

func _get_weather_name() -> String:
	if WeatherManager == null:
		return "未知"
	return WeatherManager.get_weather_name()

func _get_current_map_name() -> String:
	if MapManager.current_map == null:
		return "init_map"
	return str(MapManager.current_map.name)

func _get_nearby_npc_names(entity: NPCEntity) -> Array[String]:
	var names: Array[String] = []
	if NPCManager == null:
		return names

	var entities = NPCManager.get_all_entities()
	for npc_id in entities.keys():
		var other: NPCEntity = entities[npc_id]
		if other == entity:
			continue
		if other.position.distance_to(entity.position) <= NEARBY_NPC_DISTANCE:
			names.append(other.npc_name)

	return names