## 作物详情气泡面板
## 对应 Unity 的 CropDetailBubblePanel
extends Control

var _plant = null
var _soil = null
var _world_position: Vector2 = Vector2.ZERO
var _inventory = null
var _description_request_id: int = 0

const PANEL_SIZE = Vector2(280, 190)
const PANEL_OFFSET = Vector2(18, -150)
const VIEWPORT_MARGIN = 12.0

@onready var panel: Panel = $Panel
@onready var name_label: Label = $Panel/VBoxContainer/NameLabel
@onready var stage_label: Label = $Panel/VBoxContainer/StageLabel
@onready var description_label: Label = $Panel/VBoxContainer/DescriptionLabel
@onready var maturity_bar: ProgressBar = $Panel/VBoxContainer/MaturityBar
@onready var harvest_button: Button = $Panel/VBoxContainer/HarvestButton
@onready var close_button: Button = $Panel/VBoxContainer/CloseButton

func _ready() -> void:
	if panel:
		panel.set_anchors_preset(Control.PRESET_TOP_LEFT)
		panel.custom_minimum_size = PANEL_SIZE
		panel.size = PANEL_SIZE
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
	_request_description()
	call_deferred("_place_panel")

func _update_display() -> void:
	if _plant == null:
		return
	
	if name_label:
		name_label.text = _plant.plant_name
	
	if stage_label:
		stage_label.text = "阶段: %s" % _plant.stage_name
	
	if maturity_bar:
		maturity_bar.value = clampf(_plant.maturity_percent, 0.0, 100.0)
	
	if harvest_button:
		harvest_button.visible = _plant.is_mature
		harvest_button.disabled = not _plant.is_mature

func _request_description() -> void:
	_description_request_id += 1
	var request_id = _description_request_id
	if description_label == null or _plant == null:
		return

	description_label.text = "正在生成描述..."
	if LLMDescriptionService.instance == null:
		LLMDescriptionService.new()

	var description = await LLMDescriptionService.instance.generate_description_async(_plant)
	if request_id != _description_request_id or description_label == null:
		return

	description_label.text = description if not description.is_empty() else _build_fallback_description()

func _build_fallback_description() -> String:
	if _plant == null:
		return "这株作物静静生长着。"
	return "%s正处于%s。" % [_plant.plant_name, _plant.stage_name]

func _place_panel() -> void:
	if panel == null:
		return

	var viewport = get_viewport()
	if viewport == null:
		return

	var screen_pos = viewport.get_canvas_transform() * _world_position
	var target_pos = screen_pos + PANEL_OFFSET
	var viewport_size = get_viewport_rect().size
	var max_pos = viewport_size - panel.size - Vector2(VIEWPORT_MARGIN, VIEWPORT_MARGIN)

	target_pos.x = clampf(target_pos.x, VIEWPORT_MARGIN, max_pos.x)
	target_pos.y = clampf(target_pos.y, VIEWPORT_MARGIN, max_pos.y)
	panel.position = target_pos

func _on_harvest_pressed() -> void:
	if _soil and _inventory:
		FarmManager.harvest(_soil, _inventory)
	UIManager.close_panel("crop_detail_bubble_panel")

func _on_close_pressed() -> void:
	UIManager.close_panel("crop_detail_bubble_panel")
