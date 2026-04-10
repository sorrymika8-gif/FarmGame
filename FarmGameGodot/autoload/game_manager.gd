## 游戏管理器
## 负责场景切换、相机绑定等高层游戏逻辑
extends Node

const INITIAL_MAP_NAME = "init_map"

## 进入场景
func enter_scene(map_name: String, spawn_pos: Vector2) -> void:
	# 1. 加载地图
	if not MapManager.load_map(map_name):
		push_error("[GameManager] 加载地图失败: %s" % map_name)
		return
	
	# 2. 确保玩家存在
	if PlayerManager.player == null:
		if not PlayerManager.create_player():
			push_error("[GameManager] 创建玩家失败")
			return
	
	# 3. 设置玩家位置
	PlayerManager.set_player_position(spawn_pos)
	
	# 4. 绑定相机
	_setup_camera()
	
	# 5. 初始化农田视图（仅初始地图）
	_setup_farm_on_init_map(map_name)
	
	print("[GameManager] 进入场景: %s at %s" % [map_name, str(spawn_pos)])

# --- 私有方法 ---

func _setup_camera() -> void:
	# 获取或创建 Camera2D
	var camera = get_viewport().get_camera_2d()
	if camera == null and PlayerManager.player:
		# 检查玩家是否已有相机
		var existing_camera = PlayerManager.player.get_node_or_null("Camera2D")
		if existing_camera == null:
			camera = Camera2D.new()
			camera.name = "Camera2D"
			PlayerManager.player.add_child(camera)
			camera.make_current()
		else:
			existing_camera.make_current()
	elif camera and PlayerManager.player:
		# 如果有相机跟随脚本，设置目标
		if camera.has_method("set_target"):
			camera.set_target(PlayerManager.player)

func _setup_farm_on_init_map(map_name: String) -> void:
	if map_name != INITIAL_MAP_NAME:
		return
	
	var current_map = MapManager.current_map
	if current_map == null:
		push_warning("[GameManager] 当前地图为空，无法挂载农田")
		return
	
	# 查找农田视图节点
	var farm_view = _find_node_by_type(current_map, "FarmTilemapView")
	if farm_view == null:
		push_warning("[GameManager] init_map 中未找到 FarmTilemapView")
		return
	
	if FarmManager.current_map == null:
		push_warning("[GameManager] FarmManager.current_map 为空")
		return
	
	# 初始化农田视图
	if farm_view.has_method("initialize"):
		farm_view.initialize(FarmManager.current_map)
	
	# 设置玩家的农田视图引用
	if PlayerManager.player:
		var input_handler = PlayerManager.player.get_node_or_null("PlayerInputHandler")
		if input_handler and input_handler.has_method("set_farm_view"):
			input_handler.set_farm_view(farm_view)
	
	print("[GameManager] init_map 农田资源挂载完成")

func _find_node_by_type(root: Node, type_name: String) -> Node:
	# 查找子节点中具有指定脚本名称的节点
	for child in root.get_children():
		if child.get_script() and child.get_script().get_global_name() == type_name:
			return child
		if child.name == type_name:
			return child
		var found = _find_node_by_type(child, type_name)
		if found:
			return found
	return null
