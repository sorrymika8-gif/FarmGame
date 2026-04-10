class_name EquipHandler
extends RefCounted

## 装备处理器
## 配置表 use = "equip"
## 支持的 use_arg: slot (string)

static func execute(player_data: Dictionary, item: Dictionary, args: Dictionary) -> bool:
	var slot: String = args.get("slot", "tool")
	print("[EquipHandler] 装备道具: %s, 槽位: %s" % [item.get("name", ""), slot])
	# TODO: 实现具体的装备逻辑
	# 装备类道具通常不从背包消失，返回 false
	return false
