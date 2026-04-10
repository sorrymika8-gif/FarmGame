class_name SpriteSortByY
extends Node

## 基于 Y 坐标的精灵排序组件
## Y 坐标越小（屏幕越靠下）的物体渲染在前面

enum UpdateMode { EVERY_FRAME, ONCE_ON_START, MANUAL }

@export var base_sorting_order: int = 100
@export var precision: int = 100
@export var update_mode: int = UpdateMode.EVERY_FRAME
@export var y_offset: float = 0.0

var _sprite: CanvasItem
var _last_y: float = 0.0

func _ready() -> void:
	_sprite = get_parent() as CanvasItem
	if _sprite == null:
		push_warning("[SpriteSortByY] 父节点不是 CanvasItem")
		return
	_last_y = _sprite.global_position.y
	update_sorting_order()

func _process(_delta: float) -> void:
	if update_mode != UpdateMode.EVERY_FRAME or _sprite == null:
		return
	var current_y: float = _sprite.global_position.y
	if not is_equal_approx(current_y, _last_y):
		_last_y = current_y
		update_sorting_order()

## 手动更新排序顺序
func update_sorting_order() -> void:
	if _sprite == null:
		return
	var effective_y: float = _sprite.global_position.y + y_offset
	var calculated_order: int = base_sorting_order - roundi(effective_y / 64.0 * precision)
	_sprite.z_index = calculated_order
