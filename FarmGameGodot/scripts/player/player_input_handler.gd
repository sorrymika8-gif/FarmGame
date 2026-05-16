## 玩家输入处理器
## 负责处理玩家的输入并转换为移动指令和交互
## 对应 Unity 的 PlayerInputHandler
class_name PlayerInputHandler
extends Node

var _main_camera: Camera2D = null
var _player: Node2D = null
var _movable: Node = null
var _farm_view: Node = null
var _inventory = null
var _is_initialized: bool = false
var _pending_soil = null
var _pending_screen_pos: Vector2 = Vector2.ZERO
var _keyboard_move_direction: Vector2 = Vector2.ZERO

const MIN_FARM_INTERACTION_DISTANCE = 48.0
const FARM_INTERACTION_DISTANCE_FACTOR = 1.5

## 是否启用输入
var input_enabled: bool = true

func setup(player: Node2D) -> void:
	_player = player
	if player.has_method("get_inventory"):
		_inventory = player.get_inventory()
	
	# 获取 movable
	_movable = player.get_node_or_null("Movable")
	if _movable == null and player.get("movable"):
		_movable = player.movable
	
	_is_initialized = true
	print("[PlayerInputHandler] 初始化完成")

## 设置农场视图引用
func set_farm_view(farm_view: Node) -> void:
	_farm_view = farm_view

func _unhandled_input(event: InputEvent) -> void:
	if not _is_initialized or not input_enabled:
		return
	
	_handle_click_input(event)

func _process(_delta: float) -> void:
	if not _is_initialized or not input_enabled:
		return
	_handle_keyboard_movement(_delta)

func _handle_click_input(event: InputEvent) -> void:
	if event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT:
		# 获取世界坐标
		var world_pos = _get_mouse_world_position()
		
		# 尝试查找农场视图
		if _farm_view == null:
			_farm_view = get_tree().get_first_node_in_group("farm_view")
		
		# 检查是否点击在农田上
		if _farm_view and _farm_view.has_method("get_soil_at_world_pos"):
			var soil = _farm_view.get_soil_at_world_pos(world_pos)
			if soil:
				var soil_world_pos = _get_soil_world_position(soil)
				if _is_soil_in_interaction_range(soil_world_pos):
					_open_soil_interaction(soil, soil_world_pos, event.position)
					return
				print("[PlayerInputHandler] 距离太远，无法交互土地: %s" % str(soil.grid_pos))
				return
		
		# 鼠标只用于交互，不再点击地面移动
		_clear_pending_farm_interaction()

func _handle_keyboard_movement(delta: float) -> void:
	if _player == null:
		return
	var direction := Vector2.ZERO
	if Input.is_key_pressed(KEY_A) or Input.is_key_pressed(KEY_LEFT):
		direction.x -= 1.0
	if Input.is_key_pressed(KEY_D) or Input.is_key_pressed(KEY_RIGHT):
		direction.x += 1.0
	if Input.is_key_pressed(KEY_W) or Input.is_key_pressed(KEY_UP):
		direction.y -= 1.0
	if Input.is_key_pressed(KEY_S) or Input.is_key_pressed(KEY_DOWN):
		direction.y += 1.0
	if direction != Vector2.ZERO:
		direction = direction.normalized()
		if _movable and _movable.has_method("stop_movement"):
			_movable.stop_movement()
		var speed := 100.0
		if _movable:
			var movable_speed = _movable.get("move_speed")
			if movable_speed != null:
				speed = float(movable_speed)
		_player.position += direction * speed * delta
		_keyboard_move_direction = direction
	elif _keyboard_move_direction != Vector2.ZERO:
		_keyboard_move_direction = Vector2.ZERO

func _get_mouse_world_position() -> Vector2:
	var viewport = get_viewport()
	if viewport:
		var canvas_transform = viewport.get_canvas_transform()
		return canvas_transform.affine_inverse() * viewport.get_mouse_position()
	return Vector2.ZERO

func _get_screen_position(world_pos: Vector2) -> Vector2:
	var viewport = get_viewport()
	if viewport:
		return viewport.get_canvas_transform() * world_pos
	return _pending_screen_pos

func _get_soil_world_position(soil) -> Vector2:
	if _farm_view and _farm_view.has_method("grid_to_world"):
		return _farm_view.grid_to_world(soil.grid_pos)
	return MapManager.grid_to_world(soil.grid_pos)

func _get_farm_interaction_distance() -> float:
	return maxf(MIN_FARM_INTERACTION_DISTANCE, MapManager.tile_size * FARM_INTERACTION_DISTANCE_FACTOR)

func _is_soil_in_interaction_range(soil_world_pos: Vector2) -> bool:
	if _player == null:
		return false
	return _player.global_position.distance_to(soil_world_pos) <= _get_farm_interaction_distance()

func _queue_farm_interaction(soil, soil_world_pos: Vector2, screen_pos: Vector2) -> void:
	_pending_soil = soil
	_pending_screen_pos = screen_pos
	if _farm_view and _farm_view.has_method("set_highlight"):
		_farm_view.set_highlight(soil.grid_pos)
	if _movable and _movable.has_method("move_to"):
		_movable.move_to(soil_world_pos)

func _try_complete_pending_farm_interaction() -> void:
	if _pending_soil == null:
		return
	var soil_world_pos = _get_soil_world_position(_pending_soil)
	if not _is_soil_in_interaction_range(soil_world_pos):
		return
	var screen_pos = _get_screen_position(soil_world_pos)
	var soil = _pending_soil
	_clear_pending_farm_interaction()
	_open_soil_interaction(soil, soil_world_pos, screen_pos)

func _clear_pending_farm_interaction() -> void:
	_pending_soil = null
	_pending_screen_pos = Vector2.ZERO
	if _farm_view and _farm_view.has_method("clear_highlight"):
		_farm_view.clear_highlight()

func _open_soil_interaction(soil, soil_world_pos: Vector2, screen_pos: Vector2) -> void:
	if soil.has_plant:
		_open_crop_detail_bubble(soil, soil_world_pos)
	else:
		_open_farm_context_menu(soil, screen_pos)

func _open_crop_detail_bubble(soil, world_pos: Vector2) -> void:
	print("[PlayerInputHandler] 打开作物详情气泡, 土地: %s" % str(soil.grid_pos))
	UIManager.open_panel("res://ui/crop_detail_bubble_panel.tscn", {
		"plant": soil.plant,
		"soil": soil,
		"world_position": world_pos,
		"inventory": _inventory,
	})

func _open_farm_context_menu(soil, screen_pos: Vector2) -> void:
	print("[PlayerInputHandler] 打开农场菜单, 土地: %s" % str(soil.grid_pos))
	UIManager.open_panel("res://ui/farm_context_menu_panel.tscn", {
		"soil": soil,
		"inventory": _inventory,
		"screen_position": screen_pos,
	})
