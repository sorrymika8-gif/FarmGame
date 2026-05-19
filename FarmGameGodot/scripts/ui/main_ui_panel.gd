## 主界面面板
## 对应 Unity 的 MainUIPanel
extends Control

@onready var gold_label: Label = $MarginContainer/VBoxContainer/TopBar/GoldLabel
@onready var weather_label: Label = $MarginContainer/VBoxContainer/TopBar/WeatherLabel
@onready var backpack_button: Button = $MarginContainer/VBoxContainer/BottomBar/BackpackButton
@onready var save_button: Button = $MarginContainer/VBoxContainer/BottomBar/SaveButton
@onready var equipment_button: Button = $MarginContainer/VBoxContainer/BottomBar/EquipmentButton
@onready var pause_button: Button = $MarginContainer/VBoxContainer/BottomBar/PauseButton

func _ready() -> void:
	# 连接按钮信号
	if backpack_button:
		backpack_button.pressed.connect(_on_backpack_pressed)
	if save_button:
		save_button.pressed.connect(_on_save_pressed)
	if equipment_button:
		equipment_button.pressed.connect(_on_equipment_pressed)
	if pause_button:
		pause_button.pressed.connect(_on_pause_pressed)
	
	# 连接金币变化信号
	PlayerManager.gold_changed.connect(_on_gold_changed)
	
	# 连接天气变化信号
	if WeatherManager.has_signal("weather_changed"):
		WeatherManager.weather_changed.connect(_on_weather_changed)
	
	_update_gold_display()
	_update_weather_display()

func setup(data: Dictionary) -> void:
	pass

func _on_gold_changed(amount: int) -> void:
	_update_gold_display()

func _on_weather_changed(weather: int) -> void:
	_update_weather_display()

func _on_backpack_pressed() -> void:
	var inventory = PlayerManager.get_player_inventory()
	UIManager.open_backpack_panel(inventory)

func _on_save_pressed() -> void:
	UIManager.open_save_load_panel(true)

func _on_equipment_pressed() -> void:
	UIManager.open_equipment_panel()

func _on_pause_pressed() -> void:
	UIManager.open_pause_menu()
	get_tree().paused = true

func _update_gold_display() -> void:
	if gold_label:
		gold_label.text = "金币: %d" % PlayerManager.gold

func _update_weather_display() -> void:
	if weather_label and WeatherManager._is_initialized:
		weather_label.text = "天气: %s" % WeatherManager.get_weather_name()
