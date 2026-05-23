## NPC 控制器 (View/Controller层)
## 负责处理 NPC 的可视化表现、移动和交互
## 对应 Unity 的 NPCController
class_name NPCController
extends CharacterBody2D

@export var bubble_offset: Vector2 = Vector2(0, -40)

var _movable: Movable = null
var _entity: NPCEntity = null
var _bubble_label: Label = null
var _bubble_timer: Timer = null
var _is_initialized: bool = false

var npc_id: String:
	get: return _entity.id if _entity else ""

func _ready() -> void:
	add_to_group("npc")
	
	# 创建碰撞形状
	if not has_node("CollisionShape2D"):
		var col = CollisionShape2D.new()
		var shape = RectangleShape2D.new()
		shape.size = Vector2(16, 16)
		col.shape = shape
		add_child(col)
	
	_initialize_movable()
	
	# 初始化气泡
	_initialize_bubble()
	_register_command_callbacks()
	
	# 设置输入检测区域
	var area = Area2D.new()
	area.name = "InteractArea"
	var area_col = CollisionShape2D.new()
	var area_shape = CircleShape2D.new()
	area_shape.radius = 8
	area_col.shape = area_shape
	area.add_child(area_col)
	area.input_pickable = true
	area.input_event.connect(_on_input_event)
	add_child(area)

func _process(_delta: float) -> void:
	if _entity:
		_entity.position = global_position

## 绑定数据实体
func bind(entity: NPCEntity) -> void:
	_entity = entity
	if _entity == null:
		return
	
	global_position = _entity.position
	_is_initialized = true
	
	# 注册到 Manager
	NPCManager.register_controller(self)

## 获取移动组件
func get_movable() -> Movable:
	return _movable

## 交互接口
func interact() -> void:
	if _entity == null:
		return
	var is_at_shop = _entity.current_location_id.is_empty() or _entity.current_location_id == "seed_shop"
	if _entity.role == "shop_owner" and _entity.shop_type > 0 and is_at_shop:
		UIManager.open_shop_panel(_entity.shop_type, PlayerManager.get_player_inventory())
		return
	UIManager.open_dialogue_panel(_entity)

## 输入事件处理（点击交互）
func _on_input_event(_viewport: Node, event: InputEvent, _shape_idx: int) -> void:
	if event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT:
		get_viewport().set_input_as_handled()
		if not _is_player_in_range():
			print("[NPCController] 玩家不在交互距离内")
			return
		interact()

## 检查玩家是否在交互距离内
func _is_player_in_range() -> bool:
	if _entity == null:
		return false
	
	var player = PlayerManager.player
	if player == null:
		return true
	
	var distance = global_position.distance_to(player.global_position)
	return distance <= _entity.interaction_distance

## 初始化气泡
func _initialize_bubble() -> void:
	_bubble_label = Label.new()
	_bubble_label.name = "BubbleLabel"
	_bubble_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	_bubble_label.position = bubble_offset
	_bubble_label.visible = false
	_bubble_label.add_theme_font_size_override("font_size", 12)
	add_child(_bubble_label)
	
	_bubble_timer = Timer.new()
	_bubble_timer.one_shot = true
	_bubble_timer.timeout.connect(_hide_bubble)
	add_child(_bubble_timer)

func _initialize_movable() -> void:
	var existing_movable = get_node_or_null("Movable")
	if existing_movable is Movable:
		_movable = existing_movable
	else:
		var MovableScript = load("res://scripts/movement/movable.gd")
		var movable_node = Node.new()
		movable_node.set_script(MovableScript)
		movable_node.name = "Movable"
		add_child(movable_node)
		_movable = movable_node as Movable
	_movable.target_node = self

func _register_command_callbacks() -> void:
	var callback = Callable(self, "_on_bubble_speak")
	if not CommandExecutors.on_bubble_speak.has(callback):
		CommandExecutors.on_bubble_speak.append(callback)

## 显示气泡
func show_bubble(content: String, duration: float = 3.0) -> void:
	if _bubble_label == null:
		return
	_bubble_label.text = content
	_bubble_label.visible = true
	_bubble_timer.start(duration)

## 显示带心情的气泡
func show_bubble_with_mood(content: String, mood: String = "", duration: float = 3.0) -> void:
	var display = content
	if not mood.is_empty():
		display = "%s %s" % [mood, content]
	show_bubble(display, duration)

## 隐藏气泡
func _hide_bubble() -> void:
	if _bubble_label:
		_bubble_label.visible = false

## 移动到指定位置
func move_to(p_position: Vector2) -> void:
	if _movable:
		_movable.move_to(p_position)
	else:
		global_position = p_position

## 说话（会显示气泡）
func speak(content: String) -> void:
	print("[NPC %s] 说: %s" % [_entity.npc_name if _entity else "Unknown", content])
	show_bubble(content)

func _on_bubble_speak(speaker_node: Node, content: String, mood: String) -> void:
	if speaker_node != self:
		return
	show_bubble_with_mood(content, mood)

func _exit_tree() -> void:
	CommandExecutors.on_bubble_speak.erase(Callable(self, "_on_bubble_speak"))
	if _entity and NPCManager:
		NPCManager.unregister_controller(_entity.id, self)
