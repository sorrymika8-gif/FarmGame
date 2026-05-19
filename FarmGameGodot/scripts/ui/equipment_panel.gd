extends Control

const ACTORS := [
	{"id": "player", "name": "玩家"},
	{"id": "melee", "name": "Rowan"},
	{"id": "ranged", "name": "Mira"},
	{"id": "barrage", "name": "Nox"},
]

var _selected_actor_id := "player"
var _selected_equipment_id := ""

@onready var close_button: Button = $Panel/MarginContainer/VBoxContainer/TopBar/CloseButton
@onready var actor_tabs: HBoxContainer = $Panel/MarginContainer/VBoxContainer/ActorTabs
@onready var stats_label: Label = $Panel/MarginContainer/VBoxContainer/Content/LeftPane/StatsLabel
@onready var slot_container: VBoxContainer = $Panel/MarginContainer/VBoxContainer/Content/LeftPane/SlotScroll/SlotContainer
@onready var selected_label: Label = $Panel/MarginContainer/VBoxContainer/Content/RightPane/SelectedLabel
@onready var inventory_container: VBoxContainer = $Panel/MarginContainer/VBoxContainer/Content/RightPane/InventoryScroll/InventoryContainer

func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS
	if close_button:
		close_button.pressed.connect(_on_close_pressed)
	_build_actor_tabs()
	_refresh()

func setup(_data: Dictionary) -> void:
	pass

func _build_actor_tabs() -> void:
	for child in actor_tabs.get_children():
		child.queue_free()
	for actor in ACTORS:
		var button := Button.new()
		button.text = str(actor["name"])
		button.toggle_mode = true
		button.button_pressed = str(actor["id"]) == _selected_actor_id
		button.pressed.connect(_on_actor_pressed.bind(str(actor["id"])))
		actor_tabs.add_child(button)

func _refresh() -> void:
	_refresh_actor_tabs()
	_refresh_stats()
	_refresh_slots()
	_refresh_inventory()

func _refresh_actor_tabs() -> void:
	for i in range(actor_tabs.get_child_count()):
		var button := actor_tabs.get_child(i) as Button
		if button:
			button.button_pressed = str(ACTORS[i]["id"]) == _selected_actor_id

func _refresh_stats() -> void:
	var stats := EquipmentManager.get_actor_stats(_selected_actor_id)
	stats_label.text = "生命 %.0f\n移速 %.0f\n攻击 %.1f\n防御 %.1f\n暴击 %.1f%%\n爆伤 %.1f%%" % [
		float(stats.get("max_hp", 0.0)),
		float(stats.get("move_speed", 0.0)),
		float(stats.get("attack", 0.0)),
		float(stats.get("defense", 0.0)),
		float(stats.get("crit_rate", 0.0)) * 100.0,
		float(stats.get("crit_damage", 0.0)) * 100.0,
	]

func _refresh_slots() -> void:
	for child in slot_container.get_children():
		child.queue_free()
	var loadout := EquipmentManager.get_actor_loadout(_selected_actor_id)
	for slot in EquipmentManager.get_all_slots():
		var row := HBoxContainer.new()
		row.custom_minimum_size = Vector2(0, 32)
		var slot_button := Button.new()
		slot_button.size_flags_horizontal = Control.SIZE_EXPAND_FILL
		var equipment_id := str(loadout.get(slot, ""))
		var equipment := EquipmentManager.get_equipment(equipment_id)
		var equipment_name := "空" if equipment.is_empty() else str(equipment.get("name", "未知装备"))
		slot_button.text = "%s：%s" % [EquipmentManager.get_slot_display_name(slot), equipment_name]
		slot_button.pressed.connect(_on_slot_pressed.bind(slot))
		row.add_child(slot_button)
		var unequip_button := Button.new()
		unequip_button.text = "卸下"
		unequip_button.disabled = equipment_id.is_empty()
		unequip_button.pressed.connect(_on_unequip_pressed.bind(slot))
		row.add_child(unequip_button)
		slot_container.add_child(row)

func _refresh_inventory() -> void:
	for child in inventory_container.get_children():
		child.queue_free()
	if _selected_equipment_id.is_empty():
		selected_label.text = "已选：无"
	else:
		var selected := EquipmentManager.get_equipment(_selected_equipment_id)
		selected_label.text = "已选：%s" % str(selected.get("name", "未知装备"))
	var inventory := EquipmentManager.get_inventory()
	if inventory.is_empty():
		var empty_label := Label.new()
		empty_label.text = "暂无装备。完成外出探索后会获得装备。"
		empty_label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
		inventory_container.add_child(empty_label)
		return
	for equipment in inventory:
		var button := Button.new()
		var instance_id := str(equipment.get("instance_id", ""))
		button.text = "%s  [%s]\n%s" % [
			str(equipment.get("name", "未知装备")),
			EquipmentManager.get_slot_display_name(str(equipment.get("slot", ""))),
			_format_bonuses(equipment.get("bonuses", {})),
		]
		button.text_alignment = HORIZONTAL_ALIGNMENT_LEFT
		button.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
		button.toggle_mode = true
		button.button_pressed = instance_id == _selected_equipment_id
		button.pressed.connect(_on_equipment_pressed.bind(instance_id))
		inventory_container.add_child(button)

func _on_actor_pressed(actor_id: String) -> void:
	_selected_actor_id = actor_id
	_refresh()

func _on_equipment_pressed(instance_id: String) -> void:
	_selected_equipment_id = instance_id
	_refresh()

func _on_slot_pressed(slot: String) -> void:
	if _selected_equipment_id.is_empty():
		return
	var equipment := EquipmentManager.get_equipment(_selected_equipment_id)
	if equipment.is_empty():
		return
	if not EquipmentManager.get_valid_slots_for_equipment(equipment).has(slot):
		selected_label.text = "已选装备不能穿到这个槽位"
		return
	EquipmentManager.equip(_selected_actor_id, _selected_equipment_id, slot)
	_refresh()

func _on_unequip_pressed(slot: String) -> void:
	EquipmentManager.unequip(_selected_actor_id, slot)
	_refresh()

func _on_close_pressed() -> void:
	UIManager.close_panel("equipment_panel")

func _format_bonuses(bonuses) -> String:
	if not (bonuses is Dictionary):
		return ""
	var parts: Array[String] = []
	for key in bonuses.keys():
		var stat_name := _get_stat_display_name(str(key))
		var value := float(bonuses[key])
		if str(key) == "crit_rate" or str(key) == "crit_damage":
			parts.append("%s +%.1f%%" % [stat_name, value * 100.0])
		else:
			parts.append("%s +%.1f" % [stat_name, value])
	return "  ".join(parts)

func _get_stat_display_name(key: String) -> String:
	match key:
		"max_hp":
			return "生命"
		"move_speed":
			return "移速"
		"attack":
			return "攻击"
		"defense":
			return "防御"
		"crit_rate":
			return "暴击"
		"crit_damage":
			return "爆伤"
		_:
			return key
