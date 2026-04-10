class_name MaterialData
extends RefCounted

## 材料数据 - 用于技能合成

var material_name: String = ""
var description: String = ""
var attributes: String = ""
var quality: int = 1
var material_type: int = AtomEnums.MaterialType.NORMAL

func _init(p_name: String = "", p_description: String = "", p_attributes: String = "") -> void:
	material_name = p_name
	description = p_description
	attributes = p_attributes
