## 农场右键菜单面板
## 对应 Unity 的 FarmContextMenuPanel
extends Control

var _soil = null
var _inventory = null
var _screen_position: Vector2 = Vector2.ZERO

@onready var panel: Panel = $Panel
@onready var button_container: VBoxContainer = $Panel/VBoxContainer

func _ready() -> void:
	# 点击面板外关闭
	gui_input.connect(_on_gui_input)

func setup(data: Dictionary) -> void:
	_soil = data.get("soil", null)
	_inventory = data.get("inventory", null)
	_screen_position = data.get("screen_position", Vector2.ZERO)
	
	# 设置面板位置
	if panel:
		panel.position = _screen_position
	
	_build_menu()

func _build_menu() -> void:
	if button_container == null or _soil == null:
		return
	
	for child in button_container.get_children():
		child.queue_free()
	
	# 如果未耕地，添加耕地按钮
	if not _soil.is_tilled:
		var till_btn = Button.new()
		till_btn.text = "耕地"
		till_btn.pressed.connect(_on_till_pressed)
		button_container.add_child(till_btn)
	else:
		# 已耕地且无作物，显示可种植的种子
		if not _soil.has_plant and _inventory:
			var seeds = _get_seed_items()
			for seed_info in seeds:
				var seed_btn = Button.new()
				seed_btn.text = "种植 %s (x%d)" % [seed_info.get("name", "???"), seed_info.get("count", 0)]
				var seed_id = seed_info.get("config_id", 0)
				seed_btn.pressed.connect(_on_plant_pressed.bind(seed_id))
				button_container.add_child(seed_btn)
			
			if seeds.is_empty():
				var label = Label.new()
				label.text = "没有种子"
				button_container.add_child(label)
	
	# 关闭按钮
	var close_btn = Button.new()
	close_btn.text = "关闭"
	close_btn.pressed.connect(_on_close_pressed)
	button_container.add_child(close_btn)

func _get_seed_items() -> Array:
	var seeds: Array = []
	if _inventory == null:
		return seeds
	
	for item in _inventory.get_all_items():
		var config_info = item.config_info if item.has_method("get") else {}
		if config_info.get("item_type", -1) == FarmManager.ITEM_TYPE_SEED:
			seeds.append({
				"config_id": item.config_id,
				"name": config_info.get("name", "种子"),
				"count": item.count,
			})
	return seeds

func _on_till_pressed() -> void:
	FarmManager.till(_soil)
	UIManager.close_panel("farm_context_menu_panel")

func _on_plant_pressed(seed_config_id: int) -> void:
	FarmManager.plant(_soil, seed_config_id, _inventory)
	UIManager.close_panel("farm_context_menu_panel")

func _on_close_pressed() -> void:
	UIManager.close_panel("farm_context_menu_panel")

func _on_gui_input(event: InputEvent) -> void:
	if event is InputEventMouseButton and event.pressed:
		UIManager.close_panel("farm_context_menu_panel")
