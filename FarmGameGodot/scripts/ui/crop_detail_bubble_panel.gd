## 作物详情气泡面板
## 对应 Unity 的 CropDetailBubblePanel
extends Control

var _plant = null
var _soil = null
var _world_position: Vector2 = Vector2.ZERO
var _inventory = null

@onready var panel: Panel = $Panel
@onready var name_label: Label = $Panel/VBoxContainer/NameLabel
@onready var stage_label: Label = $Panel/VBoxContainer/StageLabel
@onready var maturity_bar: ProgressBar = $Panel/VBoxContainer/MaturityBar
@onready var harvest_button: Button = $Panel/VBoxContainer/HarvestButton
@onready var close_button: Button = $Panel/VBoxContainer/CloseButton

func _ready() -> void:
	if harvest_button:
		harvest_button.pressed.connect(_on_harvest_pressed)
	if close_button:
		close_button.pressed.connect(_on_close_pressed)

func setup(data: Dictionary) -> void:
	_plant = data.get("plant", null)
	_soil = data.get("soil", null)
	_world_position = data.get("world_position", Vector2.ZERO)
	_inventory = data.get("inventory", null)
	
	_update_display()

func _update_display() -> void:
	if _plant == null:
		return
	
	if name_label:
		name_label.text = _plant.plant_name if _plant.has_method("get") else "作物"
	
	if stage_label:
		stage_label.text = "阶段: %s" % (_plant.stage_name if _plant.has_method("get") else "未知")
	
	if maturity_bar:
		maturity_bar.value = _plant.maturity_percent if _plant.has_method("get") else 0.0
	
	if harvest_button:
		harvest_button.visible = _plant.is_mature if _plant.has_method("get") else false
		harvest_button.disabled = not (_plant.is_mature if _plant.has_method("get") else false)

func _on_harvest_pressed() -> void:
	if _soil and _inventory:
		FarmManager.harvest(_soil, _inventory)
	UIManager.close_panel("crop_detail_bubble_panel")

func _on_close_pressed() -> void:
	UIManager.close_panel("crop_detail_bubble_panel")
