## 玩家管理器
## 负责玩家实体的创建、访问和生命周期管理
extends Node

const PLAYER_SCENE_PATH = "res://resources/prefabs/player/player.tscn"
const DEFAULT_GOLD = 100

var _is_initialized: bool = false
var _player: Node2D = null # PlayerController
var _player_root: Node2D = null
var _gold: int = DEFAULT_GOLD

signal gold_changed(amount: int)

## 玩家控制器实例
var player: Node2D:
	get:
		return _player

## 玩家金币数量
var gold: int:
	get:
		return _gold

func initialize() -> void:
	if _is_initialized:
		return
	
	_player_root = Node2D.new()
	_player_root.name = "PlayerRoot"
	add_child(_player_root)
	_gold = DEFAULT_GOLD
	
	_is_initialized = true
	print("[PlayerManager] 初始化完成")

## 创建玩家实例
func create_player() -> bool:
	if not _is_initialized:
		push_error("[PlayerManager] 未初始化")
		return false
	
	if _player != null:
		push_warning("[PlayerManager] 玩家已存在")
		return true
	
	# 加载玩家场景
	var player_scene = load(PLAYER_SCENE_PATH) as PackedScene
	if player_scene == null:
		push_error("[PlayerManager] 加载玩家预制体失败: %s" % PLAYER_SCENE_PATH)
		return false
	
	# 实例化玩家
	_player = player_scene.instantiate() as Node2D
	if _player == null:
		push_error("[PlayerManager] 实例化玩家失败")
		return false
	
	_player.name = "Player"
	_player_root.add_child(_player)
	
	# 如果玩家有 initialize 方法则调用
	if _player.has_method("initialize"):
		_player.initialize()
	
	print("[PlayerManager] 玩家已创建")
	return true

## 销毁玩家实例
func destroy_player() -> void:
	if _player != null:
		_player.queue_free()
		_player = null
		print("[PlayerManager] 玩家已销毁")

## 设置玩家位置
func set_player_position(position: Vector2) -> void:
	if _player == null:
		push_error("[PlayerManager] 玩家不存在")
		return
	_player.position = position

## 获取玩家背包
func get_player_inventory():
	if _player and _player.has_method("get_inventory"):
		return _player.get_inventory()
	return null

## 增加金币
func add_gold(amount: int) -> void:
	if not _is_initialized:
		return
	if amount <= 0:
		push_warning("[PlayerManager] 增加金币数量必须为正数")
		return
	_gold += amount
	gold_changed.emit(_gold)
	print("[PlayerManager] 金币增加: +%d, 总计: %d" % [amount, _gold])

## 花费金币
func spend_gold(amount: int) -> bool:
	if not _is_initialized:
		return false
	if amount <= 0:
		push_warning("[PlayerManager] 花费金币数量必须为正数")
		return false
	if _gold < amount:
		push_warning("[PlayerManager] 金币不足. 拥有: %d, 需要: %d" % [_gold, amount])
		return false
	_gold -= amount
	gold_changed.emit(_gold)
	print("[PlayerManager] 金币花费: -%d, 总计: %d" % [amount, _gold])
	return true

## 检查是否有足够金币
func has_enough_gold(amount: int) -> bool:
	return _gold >= amount

## 设置金币数量（存档加载用）
func set_gold(amount: int) -> void:
	if not _is_initialized:
		return
	var old_gold = _gold
	_gold = maxi(amount, 0)
	if old_gold != _gold:
		gold_changed.emit(_gold)
		print("[PlayerManager] 金币设置: %d -> %d" % [old_gold, _gold])

func _notification(what: int) -> void:
	if what == NOTIFICATION_PREDELETE:
		destroy_player()
