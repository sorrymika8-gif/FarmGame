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

const FARM_INTERACTION_DISTANCE = 32.0 # Godot 像素距离

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
				# 检查距离
				var soil_world_pos = _farm_view.grid_to_world(soil.grid_pos) if _farm_view.has_method("grid_to_world") else Vector2(soil.grid_pos) * 16
				var distance = _player.position.distance_to(soil_world_pos)
				
				if distance <= FARM_INTERACTION_DISTANCE:
					if soil.has_plant:
						_open_crop_detail_bubble(soil, soil_world_pos)
					else:
						_open_farm_context_menu(soil, event.position)
					return
		
		# 不是农田或距离太远，执行移动
		if _movable and _movable.has_method("move_to"):
			_movable.move_to(world_pos)

func _get_mouse_world_position() -> Vector2:
	var viewport = get_viewport()
	if viewport:
		var canvas_transform = viewport.get_canvas_transform()
		return canvas_transform.affine_inverse() * viewport.get_mouse_position()
	return Vector2.ZERO

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
