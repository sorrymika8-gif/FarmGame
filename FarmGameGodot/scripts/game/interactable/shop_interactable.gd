class_name ShopInteractable
extends Area2D

## 商店可交互物体控制器

@export var shop_type: int = 1
@export var interaction_distance: float = 2.0 * 64.0
@export var shop_name: String = "商店"
@export var interaction_hint: String = "点击购物"
@export var too_far_hint: String = "太远了，请靠近一点"

func _ready() -> void:
	input_event.connect(_on_input_event)

func _on_input_event(_viewport: Node, event: InputEvent, _shape_idx: int) -> void:
	if event is InputEventMouseButton:
		var mb := event as InputEventMouseButton
		if mb.button_index == MOUSE_BUTTON_LEFT and mb.pressed:
			get_viewport().set_input_as_handled()
			try_interact()

## 尝试交互
func try_interact() -> void:
	if not _is_player_in_range():
		print("[ShopInteractable] %s" % too_far_hint)
		return
	interact()

## 执行交互（打开商店）
func interact() -> void:
	UIManager.open_shop_panel(shop_type, PlayerManager.get_player_inventory())
	print("[ShopInteractable] 打开 %s" % shop_name)

## 检查玩家是否在交互范围内
func _is_player_in_range() -> bool:
	var player = PlayerManager.player
	if player == null:
		return false
	var dist := global_position.distance_to(player.global_position)
	return dist <= interaction_distance

## 获取到玩家的距离
func get_distance_to_player() -> float:
	var player = PlayerManager.player
	if player == null:
		return INF
	return global_position.distance_to(player.global_position)
