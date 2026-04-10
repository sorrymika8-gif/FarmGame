## 玩家控制器
## 挂载在玩家节点上，作为玩家实体的核心组件
## 对应 Unity 的 PlayerController
class_name PlayerController
extends CharacterBody2D

var _data: Dictionary = {}
var _is_initialized: bool = false
var _movable: Node = null

## 玩家数据
var data: Dictionary:
	get:
		return _data

## 移动组件
var movable: Node:
	get:
		return _movable

func initialize() -> void:
	if _is_initialized:
		return
	
	# 获取 Movable 子节点
	_movable = get_node_or_null("Movable")
	if _movable == null:
		# 动态创建 Movable
		var MovableScript = load("res://scripts/movement/movable.gd")
		_movable = Node.new()
		_movable.set_script(MovableScript)
		_movable.name = "Movable"
		add_child(_movable)
		_movable.target_node = self
	
	# 初始化玩家数据
	var PlayerDataScript = load("res://scripts/player/player_data.gd")
	var player_data_obj = PlayerDataScript.new()
	_data = player_data_obj.to_dict()
	
	# 初始化背包
	var InventoryScript = load("res://scripts/item/inventory_component.gd")
	_data["inventory"] = InventoryScript.new()
	
	# 同步移动速度
	if _movable:
		_movable.move_speed = _data.get("move_speed", 100.0)
	
	# 设置输入处理器
	var input_handler = get_node_or_null("PlayerInputHandler")
	if input_handler == null:
		var InputHandlerScript = load("res://scripts/player/player_input_handler.gd")
		input_handler = Node.new()
		input_handler.set_script(InputHandlerScript)
		input_handler.name = "PlayerInputHandler"
		add_child(input_handler)
	
	if input_handler.has_method("setup"):
		input_handler.setup(self)
	
	_is_initialized = true
	print("[PlayerController] 初始化完成")

## 获取背包
func get_inventory():
	return _data.get("inventory", null)

## 获取玩家数据
func get_data() -> Dictionary:
	return _data

## 设置为非新玩家
func set_new_player(value: bool) -> void:
	_data["is_new_player"] = value
