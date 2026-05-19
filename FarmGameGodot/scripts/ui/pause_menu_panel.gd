extends Control

@onready var resume_button: Button = $CenterContainer/Panel/MarginContainer/VBoxContainer/ResumeButton
@onready var main_menu_button: Button = $CenterContainer/Panel/MarginContainer/VBoxContainer/MainMenuButton
@onready var save_button: Button = $CenterContainer/Panel/MarginContainer/VBoxContainer/SaveButton
@onready var quit_button: Button = $CenterContainer/Panel/MarginContainer/VBoxContainer/QuitButton

func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS
	if resume_button:
		resume_button.pressed.connect(_on_resume_pressed)
	if main_menu_button:
		main_menu_button.pressed.connect(_on_main_menu_pressed)
	if save_button:
		save_button.pressed.connect(_on_save_pressed)
	if quit_button:
		quit_button.pressed.connect(_on_quit_pressed)

func setup(_data: Dictionary) -> void:
	pass

func _on_resume_pressed() -> void:
	UIManager.close_panel("pause_menu_panel")
	get_tree().paused = false

func _on_main_menu_pressed() -> void:
	GameInitManager.return_to_main_menu()

func _on_save_pressed() -> void:
	UIManager.open_save_load_panel(true)

func _on_quit_pressed() -> void:
	get_tree().quit()
