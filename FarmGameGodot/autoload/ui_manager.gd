## UI 管理器
## 管理所有 UI 面板的打开、关闭、显示、隐藏
extends CanvasLayer

var _is_initialized: bool = false
var _panels: Dictionary = {} # panel_name -> Control
var _panel_stack: Array = [] # 面板打开栈
var _non_popup_panels := {
	"main_ui_panel": true,
	"main_menu_panel": true,
	"exploration_play_panel": true,
}

func initialize() -> void:
	if _is_initialized:
		return
	layer = 100 # UI 在最上层
	process_mode = Node.PROCESS_MODE_ALWAYS
	_is_initialized = true
	print("[UIManager] 初始化完成")

func _unhandled_input(event: InputEvent) -> void:
	if not _is_initialized:
		return
	if event.is_action_pressed("ui_cancel") and close_top_popup_panel():
		get_viewport().set_input_as_handled()

## 打开 UI 面板
## panel_scene_path: 面板场景路径（res://ui/xxx.tscn）
## data: 传递给面板的数据字典
func open_panel(panel_scene_path: String, data: Dictionary = {}) -> Control:
	if not _is_initialized:
		push_error("[UIManager] 未初始化")
		return null
	
	var panel_name = panel_scene_path.get_file().get_basename()
	
	# 如果面板已打开，直接返回
	if _panels.has(panel_name) and is_instance_valid(_panels[panel_name]):
		var existing = _panels[panel_name]
		existing.visible = true
		if existing.has_method("setup"):
			existing.setup(data)
		return existing
	
	# 加载面板场景
	var packed_scene = load(panel_scene_path) as PackedScene
	if packed_scene == null:
		push_error("[UIManager] 加载面板失败: %s" % panel_scene_path)
		return null
	
	# 实例化面板
	var panel = packed_scene.instantiate() as Control
	if panel == null:
		push_error("[UIManager] 面板不是 Control 类型: %s" % panel_scene_path)
		return null
	panel.process_mode = Node.PROCESS_MODE_ALWAYS
	
	# 设置全屏
	panel.set_anchors_preset(Control.PRESET_FULL_RECT)
	panel.name = panel_name
	
	# 添加到 UI 层
	add_child(panel)
	_panels[panel_name] = panel
	_panel_stack.append(panel_name)
	
	# 传递数据给面板（如果面板有 setup 方法）
	if panel.has_method("setup"):
		panel.setup(data)
	
	print("[UIManager] 打开面板: %s" % panel_name)
	return panel

## 关闭指定面板
func close_panel(panel_name: String) -> void:
	if not _panels.has(panel_name):
		return
	
	var panel = _panels[panel_name]
	if is_instance_valid(panel):
		panel.queue_free()
	
	_panels.erase(panel_name)
	_panel_stack.erase(panel_name)
	if panel_name == "pause_menu_panel":
		get_tree().paused = false
	print("[UIManager] 关闭面板: %s" % panel_name)

## 显示面板
func show_panel(panel_name: String) -> void:
	if _panels.has(panel_name) and is_instance_valid(_panels[panel_name]):
		_panels[panel_name].visible = true

## 隐藏面板
func hide_panel(panel_name: String) -> void:
	if _panels.has(panel_name) and is_instance_valid(_panels[panel_name]):
		_panels[panel_name].visible = false

## 获取面板实例
func get_panel(panel_name: String) -> Control:
	if _panels.has(panel_name) and is_instance_valid(_panels[panel_name]):
		return _panels[panel_name]
	return null

## 检查面板是否已打开
func is_panel_open(panel_name: String) -> bool:
	return _panels.has(panel_name) and is_instance_valid(_panels[panel_name])

## 关闭所有面板
func close_all_panels() -> void:
	for panel_name in _panels.keys():
		var panel = _panels[panel_name]
		if is_instance_valid(panel):
			panel.queue_free()
	_panels.clear()
	_panel_stack.clear()

## 关闭栈顶面板（类似返回操作）
func close_top_panel() -> void:
	if _panel_stack.size() > 0:
		var top = _panel_stack.back()
		close_panel(top)

func close_top_popup_panel() -> bool:
	for i in range(_panel_stack.size() - 1, -1, -1):
		var panel_name: String = _panel_stack[i]
		if _non_popup_panels.has(panel_name):
			continue
		if not _panels.has(panel_name) or not is_instance_valid(_panels[panel_name]):
			_panel_stack.remove_at(i)
			_panels.erase(panel_name)
			continue
		close_panel(panel_name)
		return true
	return false

# --- 便捷方法 ---

## 打开主界面
func open_main_ui() -> Control:
	return open_panel("res://ui/main_ui_panel.tscn")

func open_main_menu() -> Control:
	return open_panel("res://ui/main_menu_panel.tscn")

func open_pause_menu() -> Control:
	return open_panel("res://ui/pause_menu_panel.tscn")

func open_equipment_panel() -> Control:
	return open_panel("res://ui/equipment_panel.tscn")

## 打开背包面板
func open_backpack_panel(inventory) -> Control:
	return open_panel("res://ui/backpack_panel.tscn", {"inventory": inventory})

## 打开商店面板
func open_shop_panel(shop_type: int, inventory = null) -> Control:
	return open_panel("res://ui/shop_panel.tscn", {
		"shop_type": shop_type,
		"player_inventory": inventory
	})

## 打开对话面板
func open_dialogue_panel(npc_entity) -> Control:
	return open_panel("res://ui/dialogue_ui_panel.tscn", {"npc_entity": npc_entity})

## 打开存档面板
func open_save_load_panel(is_save_mode: bool = true) -> Control:
	return open_panel("res://ui/save_load_panel.tscn", {"is_save_mode": is_save_mode})

func _notification(what: int) -> void:
	if what == NOTIFICATION_PREDELETE:
		close_all_panels()
