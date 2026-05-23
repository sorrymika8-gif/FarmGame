class_name MapTransition
extends Area2D

@export var target_map: String = ""
@export var target_spawn_grid: Vector2i = Vector2i.ZERO
@export var interaction_distance: float = 2.0 * 64.0
@export var transition_name: String = "前往"
@export var too_far_hint: String = "太远了，请靠近一点"

func _ready() -> void:
	input_event.connect(_on_input_event)

func _on_input_event(_viewport: Node, event: InputEvent, _shape_idx: int) -> void:
	if event is InputEventMouseButton:
		var mouse_button := event as InputEventMouseButton
		if mouse_button.button_index == MOUSE_BUTTON_LEFT and mouse_button.pressed:
			get_viewport().set_input_as_handled()
			try_transition()

func try_transition() -> void:
	if target_map.is_empty():
		push_warning("[MapTransition] 未配置目标地图")
		return
	if not _is_player_in_range():
		print("[MapTransition] %s" % too_far_hint)
		return

	var spawn_position = MapManager.grid_to_world(target_spawn_grid)
	GameManager.enter_scene(target_map, spawn_position)
	print("[MapTransition] %s -> %s" % [transition_name, target_map])

func _is_player_in_range() -> bool:
	var player = PlayerManager.player
	if player == null:
		return false
	return global_position.distance_to(player.global_position) <= interaction_distance