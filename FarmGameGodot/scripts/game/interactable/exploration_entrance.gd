class_name ExplorationEntrance
extends Area2D

@export var interaction_distance: float = 2.0 * 64.0
@export var entrance_name: String = "外出探索"
@export var too_far_hint: String = "太远了，请靠近一点"

func _ready() -> void:
	input_event.connect(_on_input_event)

func _on_input_event(_viewport: Node, event: InputEvent, _shape_idx: int) -> void:
	if event is InputEventMouseButton:
		var mb := event as InputEventMouseButton
		if mb.button_index == MOUSE_BUTTON_LEFT and mb.pressed:
			get_viewport().set_input_as_handled()
			try_interact()

func try_interact() -> void:
	if not _is_player_in_range():
		print("[ExplorationEntrance] %s" % too_far_hint)
		return
	UIManager.open_panel("res://ui/exploration_confirm_panel.tscn")
	print("[ExplorationEntrance] 打开 %s 入口" % entrance_name)

func _is_player_in_range() -> bool:
	var player = PlayerManager.player
	if player == null:
		return false
	return global_position.distance_to(player.global_position) <= interaction_distance
