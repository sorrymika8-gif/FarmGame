extends Control

@onready var start_button: Button = $CenterContainer/Panel/MarginContainer/VBoxContainer/StartButton
@onready var load_button: Button = $CenterContainer/Panel/MarginContainer/VBoxContainer/LoadButton
@onready var quit_button: Button = $CenterContainer/Panel/MarginContainer/VBoxContainer/QuitButton

func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS
	if start_button:
		start_button.pressed.connect(_on_start_pressed)
	if load_button:
		load_button.pressed.connect(_on_load_pressed)
	if quit_button:
		quit_button.pressed.connect(_on_quit_pressed)

func setup(_data: Dictionary) -> void:
	pass

func _on_start_pressed() -> void:
	GameInitManager.start_new_game()

func _on_load_pressed() -> void:
	UIManager.open_panel("res://ui/save_load_panel.tscn", {
		"is_save_mode": false,
		"on_load_selected": Callable(self, "_on_load_slot_selected")
	})

func _on_load_slot_selected(slot_index: int) -> void:
	GameInitManager.start_game_from_save(slot_index)

func _on_quit_pressed() -> void:
	get_tree().quit()
