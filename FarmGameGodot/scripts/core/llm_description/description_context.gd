## 描述上下文，封装从可描述对象收集的属性数据
class_name DescriptionContext
extends RefCounted

## 对象类型标识
var type: String = ""

## 对象显示名称
var display_name: String = ""

## 缓存键
var cache_key: String = ""

## 属性字典
var properties: Dictionary = {}

## 链式添加属性
func add_property(key: String, value) -> DescriptionContext:
	properties[key] = value
	return self

## 获取属性值
func get_property(key: String, default_value = null):
	return properties.get(key, default_value)

## 核心方法：将 {{Key}} 替换为属性值
func replace_template(template: String) -> String:
	var result := template
	for key in properties:
		var value = properties[key]
		var replacement := ""
		
		if value is bool:
			replacement = "是" if value else "否"
		elif value is float:
			replacement = "%.2f" % value
		elif value == null:
			replacement = ""
		else:
			replacement = str(value)
		
		result = result.replace("{{%s}}" % key, replacement)
	
	return result

## 从 IDescribable 对象构建上下文
static func build_from(target) -> DescriptionContext:
	var ctx := DescriptionContext.new()
	
	if target.has_method("get_description_type"):
		ctx.type = target.get_description_type()
	if target.has_method("get_display_name"):
		ctx.display_name = target.get_display_name()
	if target.has_method("get_cache_key"):
		ctx.cache_key = target.get_cache_key()
	if target.has_method("get_describable_properties"):
		var props: Dictionary = target.get_describable_properties()
		for key in props:
			ctx.properties[key] = props[key]
	
	return ctx
