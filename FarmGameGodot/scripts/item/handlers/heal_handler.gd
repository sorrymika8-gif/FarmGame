class_name HealHandler
extends RefCounted

## 治疗/恢复效果处理器
## 配置表 use = "heal"
## 支持的 use_arg: hp (int), hunger (int)

static func execute(player_data: Dictionary, item: Dictionary, args: Dictionary) -> bool:
	var hp: int = _get_int_arg(args, "hp", 0)
	var hunger: int = _get_int_arg(args, "hunger", 0)
	print("[HealHandler] 使用道具: %s, 恢复体力: %d, 恢复饥饿: %d" % [item.get("name", ""), hp, hunger])
	# TODO: 实现具体的恢复逻辑
	return true

static func _get_int_arg(args: Dictionary, key: String, default_value: int) -> int:
	if not args.has(key):
		return default_value
	return int(args[key])
