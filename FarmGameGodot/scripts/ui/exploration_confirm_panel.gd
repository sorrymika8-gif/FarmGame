extends Control

@onready var start_button: Button = $Panel/MarginContainer/VBoxContainer/ButtonBar/StartButton
@onready var cancel_button: Button = $Panel/MarginContainer/VBoxContainer/ButtonBar/CancelButton
@onready var equipment_button: Button = $Panel/MarginContainer/VBoxContainer/ButtonBar/EquipmentButton
@onready var melee_check: CheckBox = $Panel/MarginContainer/VBoxContainer/NpcOptions/MeleeCheck
@onready var ranged_check: CheckBox = $Panel/MarginContainer/VBoxContainer/NpcOptions/RangedCheck
@onready var barrage_check: CheckBox = $Panel/MarginContainer/VBoxContainer/NpcOptions/BarrageCheck

func _ready() -> void:
	if start_button:
		start_button.pressed.connect(_on_start_pressed)
	if cancel_button:
		cancel_button.pressed.connect(_on_cancel_pressed)
	if equipment_button:
		equipment_button.pressed.connect(_on_equipment_pressed)

func setup(_data: Dictionary) -> void:
	pass

func _on_start_pressed() -> void:
	var selected_npcs: Array[String] = []
	if melee_check and melee_check.button_pressed:
		selected_npcs.append("melee")
	if ranged_check and ranged_check.button_pressed:
		selected_npcs.append("ranged")
	if barrage_check and barrage_check.button_pressed:
		selected_npcs.append("barrage")
	UIManager.close_panel("exploration_confirm_panel")
	UIManager.open_panel("res://ui/exploration_play_panel.tscn", {"selected_npcs": selected_npcs})

func _on_cancel_pressed() -> void:
	UIManager.close_panel("exploration_confirm_panel")

func _on_equipment_pressed() -> void:
	UIManager.open_equipment_panel()
