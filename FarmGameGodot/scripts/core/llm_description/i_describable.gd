## 可描述对象的接口约定
## 实现此接口的类需要提供以下方法：
## - get_description_type() -> String  # 描述类型标识，用于匹配模板
## - get_display_name() -> String      # 获取显示名称
## - get_cache_key() -> String         # 获取缓存键（可包含状态）
## - get_describable_properties() -> Dictionary  # 返回属性字典用于模板替换
class_name IDescribable
extends RefCounted

## 描述类型标识，用于匹配模板
func get_description_type() -> String:
	return ""

## 获取显示名称
func get_display_name() -> String:
	return ""

## 获取缓存键（可包含状态，如 "crop_1_stage_2"）
func get_cache_key() -> String:
	return ""

## 返回属性字典用于模板替换
## 例如: {"Name": "小麦", "StageName": "发芽期", "MaturityPercent": 25}
func get_describable_properties() -> Dictionary:
	return {}
