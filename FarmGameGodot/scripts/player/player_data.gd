## 玩家数据类
## 存储玩家的基础属性数据
## 对应 Unity 的 PlayerData
class_name PlayerData
extends RefCounted

var is_new_player: bool = true
var position: Vector2 = Vector2.ZERO
var facing_direction: Vector2 = Vector2.DOWN
var move_speed: float = 100.0 # Godot 中使用像素/秒

func to_dict() -> Dictionary:
	return {
		"is_new_player": is_new_player,
		"position_x": position.x,
		"position_y": position.y,
		"facing_x": facing_direction.x,
		"facing_y": facing_direction.y,
		"move_speed": move_speed,
	}

static func from_dict(data: Dictionary) -> PlayerData:
	var pd = PlayerData.new()
	pd.is_new_player = data.get("is_new_player", true)
	pd.position = Vector2(data.get("position_x", 0), data.get("position_y", 0))
	pd.facing_direction = Vector2(data.get("facing_x", 0), data.get("facing_y", 1))
	pd.move_speed = data.get("move_speed", 100.0)
	return pd
