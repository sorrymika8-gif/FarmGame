## 装备管理器
## 负责肉鸽装备实例、角色穿戴、属性汇总和伤害公式
extends Node

const SLOTS: Array[String] = [
	"head",
	"chest",
	"wrist",
	"arm",
	"pants",
	"shoes",
	"main_hand",
	"off_hand",
	"ring_1",
	"ring_2",
	"amulet",
]

const STAT_KEYS: Array[String] = [
	"max_hp",
	"move_speed",
	"attack",
	"defense",
	"crit_rate",
	"crit_damage",
	"level",
]

const SLOT_DISPLAY_NAMES := {
	"head": "头",
	"chest": "胸甲",
	"wrist": "护腕",
	"arm": "护臂",
	"pants": "裤子",
	"shoes": "鞋子",
	"main_hand": "主手武器",
	"off_hand": "副手武器",
	"ring_1": "戒指",
	"ring_2": "戒指",
	"ring": "戒指",
	"amulet": "护符",
}

var _is_initialized := false
var _equipment_inventory: Array[Dictionary] = []
var _loadouts: Dictionary = {}
var _rng := RandomNumberGenerator.new()

func initialize() -> void:
	if _is_initialized:
		return
	_rng.randomize()
	_reset_loadouts()
	_is_initialized = true
	print("[EquipmentManager] 初始化完成")

func reset_runtime() -> void:
	_reset_loadouts()

func get_all_slots() -> Array[String]:
	return SLOTS.duplicate()

func get_slot_display_name(slot: String) -> String:
	return str(SLOT_DISPLAY_NAMES.get(slot, slot))

func get_valid_slots_for_equipment(equipment: Dictionary) -> Array[String]:
	return _get_valid_slots_for_equipment(equipment)

func get_base_stats(actor_id: String) -> Dictionary:
	match actor_id:
		"player":
			return _stats(120.0, 190.0, 14.0, 8.0, 0.05, 0.50, 1)
		"melee":
			return _stats(150.0, 285.0, 18.0, 12.0, 0.05, 0.50, 1)
		"ranged":
			return _stats(90.0, 225.0, 13.0, 6.0, 0.10, 0.60, 1)
		"barrage":
			return _stats(100.0, 175.0, 10.0, 7.0, 0.08, 0.70, 1)
		_:
			return _stats(100.0, 190.0, 10.0, 5.0, 0.05, 0.50, 1)

func get_actor_stats(actor_id: String) -> Dictionary:
	var result := get_base_stats(actor_id)
	var loadout: Dictionary = _get_or_create_loadout(actor_id)
	for slot in SLOTS:
		var instance_id := str(loadout.get(slot, ""))
		if instance_id.is_empty():
			continue
		var equipment := get_equipment(instance_id)
		if equipment.is_empty():
			continue
		_apply_bonuses(result, equipment.get("bonuses", {}))
	return _normalize_stats(result)

func get_actor_loadout(actor_id: String) -> Dictionary:
	return _get_or_create_loadout(actor_id).duplicate(true)

func get_inventory() -> Array[Dictionary]:
	return _equipment_inventory.duplicate(true)

func get_equipment(instance_id: String) -> Dictionary:
	for equipment in _equipment_inventory:
		if str(equipment.get("instance_id", "")) == instance_id:
			return equipment
	return {}

func add_equipment(equipment: Dictionary) -> Dictionary:
	var copy := equipment.duplicate(true)
	if str(copy.get("instance_id", "")).is_empty():
		copy["instance_id"] = _generate_instance_id()
	if not copy.has("bonuses"):
		copy["bonuses"] = {}
	_equipment_inventory.append(copy)
	print("[EquipmentManager] 获得装备: %s" % str(copy.get("name", "未知装备")))
	return copy

func equip(actor_id: String, instance_id: String, preferred_slot: String = "") -> bool:
	var equipment := get_equipment(instance_id)
	if equipment.is_empty():
		return false
	var slot := _resolve_slot(equipment, preferred_slot)
	if not SLOTS.has(slot):
		return false
	_unequip_instance_everywhere(instance_id)
	var loadout := _get_or_create_loadout(actor_id)
	loadout[slot] = instance_id
	return true

func unequip(actor_id: String, slot: String) -> void:
	var loadout := _get_or_create_loadout(actor_id)
	if loadout.has(slot):
		loadout[slot] = ""

func create_random_equipment(level: int = 1) -> Dictionary:
	var slot := SLOTS[_rng.randi_range(0, SLOTS.size() - 1)]
	var main_stat := _roll_main_stat(slot)
	var bonuses := {}
	bonuses[main_stat] = _roll_stat_value(main_stat, level)
	if _rng.randf() < 0.35:
		var sub_stat := _roll_secondary_stat(main_stat)
		bonuses[sub_stat] = _roll_stat_value(sub_stat, level) * 0.45
	var equipment_slot := "ring" if slot == "ring_1" or slot == "ring_2" else slot
	var equipment := {
		"instance_id": _generate_instance_id(),
		"name": "%s%s" % [_roll_prefix(), get_slot_display_name(equipment_slot)],
		"slot": equipment_slot,
		"level": level,
		"rarity": "common",
		"bonuses": bonuses,
	}
	return add_equipment(equipment)

func calculate_damage(attacker_stats: Dictionary, defender_stats: Dictionary, skill_multiplier: float = 1.0, rng: RandomNumberGenerator = null) -> Dictionary:
	var attack := maxf(float(attacker_stats.get("attack", 0.0)), 0.0)
	var attacker_level := int(attacker_stats.get("level", 1))
	var defender_level := int(defender_stats.get("level", 1))
	var defense := maxf(float(defender_stats.get("defense", 0.0)), 0.0)
	var defense_multiplier := float(attacker_level + 100) / float((attacker_level + 100) + (defender_level + 100) * defense / 100.0)
	var crit_rate := clampf(float(attacker_stats.get("crit_rate", 0.0)), 0.0, 1.0)
	var crit_damage := maxf(float(attacker_stats.get("crit_damage", 0.0)), 0.0)
	var roller := rng if rng != null else _rng
	var is_crit := roller.randf() < crit_rate
	var damage := attack * maxf(skill_multiplier, 0.0) * defense_multiplier
	if is_crit:
		damage *= 1.0 + crit_damage
	return {
		"damage": maxf(damage, 0.0),
		"is_crit": is_crit,
		"defense_multiplier": defense_multiplier,
	}

func to_save_data() -> Dictionary:
	return {
		"inventory": _equipment_inventory.duplicate(true),
		"loadouts": _loadouts.duplicate(true),
	}

func load_save_data(data: Dictionary) -> void:
	_equipment_inventory.clear()
	var inventory = data.get("inventory", [])
	if inventory is Array:
		for item in inventory:
			if item is Dictionary:
				_equipment_inventory.append(item.duplicate(true))
	_loadouts = {}
	var saved_loadouts = data.get("loadouts", {})
	if saved_loadouts is Dictionary:
		for actor_id in saved_loadouts.keys():
			var loadout = saved_loadouts[actor_id]
			if loadout is Dictionary:
				_loadouts[str(actor_id)] = _sanitize_loadout(loadout)
	for actor_id in ["player", "melee", "ranged", "barrage"]:
		_get_or_create_loadout(actor_id)

func _reset_loadouts() -> void:
	_loadouts = {}
	for actor_id in ["player", "melee", "ranged", "barrage"]:
		_loadouts[actor_id] = _create_empty_loadout()

func _get_or_create_loadout(actor_id: String) -> Dictionary:
	if not _loadouts.has(actor_id):
		_loadouts[actor_id] = _create_empty_loadout()
	return _loadouts[actor_id]

func _unequip_instance_everywhere(instance_id: String) -> void:
	for actor_id in _loadouts.keys():
		var loadout: Dictionary = _loadouts[actor_id]
		for slot in SLOTS:
			if str(loadout.get(slot, "")) == instance_id:
				loadout[slot] = ""

func _create_empty_loadout() -> Dictionary:
	var loadout := {}
	for slot in SLOTS:
		loadout[slot] = ""
	return loadout

func _sanitize_loadout(loadout: Dictionary) -> Dictionary:
	var result := _create_empty_loadout()
	for slot in SLOTS:
		result[slot] = str(loadout.get(slot, ""))
	return result

func _stats(max_hp: float, move_speed: float, attack: float, defense: float, crit_rate: float, crit_damage: float, level: int) -> Dictionary:
	return {
		"max_hp": max_hp,
		"move_speed": move_speed,
		"attack": attack,
		"defense": defense,
		"crit_rate": crit_rate,
		"crit_damage": crit_damage,
		"level": level,
	}

func _apply_bonuses(stats: Dictionary, bonuses: Dictionary) -> void:
	for key in bonuses.keys():
		var stat_key := str(key)
		var value := float(bonuses[key])
		if stat_key.ends_with("_pct"):
			var base_key := stat_key.trim_suffix("_pct")
			stats[base_key] = float(stats.get(base_key, 0.0)) * (1.0 + value)
		else:
			stats[stat_key] = float(stats.get(stat_key, 0.0)) + value

func _normalize_stats(stats: Dictionary) -> Dictionary:
	stats["max_hp"] = maxf(float(stats.get("max_hp", 1.0)), 1.0)
	stats["move_speed"] = maxf(float(stats.get("move_speed", 0.0)), 0.0)
	stats["attack"] = maxf(float(stats.get("attack", 0.0)), 0.0)
	stats["defense"] = maxf(float(stats.get("defense", 0.0)), 0.0)
	stats["crit_rate"] = clampf(float(stats.get("crit_rate", 0.0)), 0.0, 1.0)
	stats["crit_damage"] = maxf(float(stats.get("crit_damage", 0.0)), 0.0)
	stats["level"] = maxi(int(stats.get("level", 1)), 1)
	return stats

func _resolve_slot(equipment: Dictionary, preferred_slot: String) -> String:
	var valid_slots := _get_valid_slots_for_equipment(equipment)
	if not preferred_slot.is_empty() and valid_slots.has(preferred_slot):
		return preferred_slot
	return valid_slots[0] if not valid_slots.is_empty() else ""

func _get_valid_slots_for_equipment(equipment: Dictionary) -> Array[String]:
	var slot := str(equipment.get("slot", ""))
	if slot == "ring":
		return ["ring_1", "ring_2"]
	if SLOTS.has(slot):
		return [slot]
	return []

func _roll_main_stat(slot: String) -> String:
	if slot == "main_hand":
		return "attack"
	if slot == "chest" or slot == "pants":
		return "defense"
	if slot == "shoes":
		return "move_speed"
	if slot == "ring_1" or slot == "ring_2" or slot == "amulet":
		return ["attack", "crit_rate", "crit_damage"][_rng.randi_range(0, 2)]
	return ["max_hp", "attack", "defense"][_rng.randi_range(0, 2)]

func _roll_secondary_stat(excluded: String) -> String:
	var options := ["max_hp", "attack", "defense", "crit_rate", "crit_damage", "move_speed"]
	options.erase(excluded)
	return options[_rng.randi_range(0, options.size() - 1)]

func _roll_stat_value(stat_key: String, level: int) -> float:
	var scale := maxf(float(level), 1.0)
	match stat_key:
		"max_hp":
			return _rng.randf_range(12.0, 24.0) * scale
		"move_speed":
			return _rng.randf_range(8.0, 18.0) * scale
		"attack":
			return _rng.randf_range(2.0, 5.0) * scale
		"defense":
			return _rng.randf_range(2.0, 5.0) * scale
		"crit_rate":
			return _rng.randf_range(0.03, 0.08)
		"crit_damage":
			return _rng.randf_range(0.08, 0.18)
		_:
			return 0.0

func _roll_prefix() -> String:
	var prefixes := ["旧木", "旅人", "晨露", "铁叶", "星尘", "荒野"]
	return prefixes[_rng.randi_range(0, prefixes.size() - 1)]

func _generate_instance_id() -> String:
	return "%d_%d" % [Time.get_ticks_usec(), _rng.randi()]
