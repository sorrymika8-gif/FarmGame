## LLM 描述生成服务
## 用于为游戏对象（作物、NPC、建筑等）生成诗意描述
class_name LLMDescriptionService
extends RefCounted

static var instance: LLMDescriptionService

## 模板路径注册表 { type_name: template_path }
var mTemplateRegistry: Dictionary = {}

## 模板内容缓存 { type_name: template_content }
var mTemplateCache: Dictionary = {}

## 描述缓存 { cache_key: description }
var mDescriptionCache: Dictionary = {}

## LLM 系统提示词
const SYSTEM_PROMPT := "你是游戏中的诗意叙述者。请用简短、优美、富有想象力的语言描述游戏中的事物。直接输出描述文字，不要任何前缀、解释或标点符号外的额外内容。"

## LLM 参数
const TEMPERATURE := 0.7
const MAX_TOKENS := 100

func _init() -> void:
	instance = self
	initialize()

## 初始化并注册默认模板
func initialize() -> void:
	register_template("Crop", "res://prompts/descriptions/CropDescription.md")

## 注册模板
func register_template(type_name: String, template_path: String) -> void:
	mTemplateRegistry[type_name] = template_path

## 生成对象描述（异步）
func generate_description_async(target, use_cache: bool = true) -> String:
	# 构建上下文
	var context := DescriptionContext.build_from(target)
	
	# 检查缓存
	if use_cache and context.cache_key != "" and mDescriptionCache.has(context.cache_key):
		return mDescriptionCache[context.cache_key]
	
	# 加载模板
	var template := await _load_template_async(context.type)
	if template.is_empty():
		return _build_fallback_description(context)
	
	# 替换占位符
	var prompt := context.replace_template(template)
	
	# 调用 LLM
	if LLMService == null or LLMService.client == null:
		return _build_fallback_description(context)
	
	var result: String = await LLMService.ask(prompt, SYSTEM_PROMPT, TEMPERATURE, MAX_TOKENS)
	
	if result.is_empty():
		return _build_fallback_description(context)
	
	# 清理结果（去除多余空白和引号）
	result = result.strip_edges()
	if result.begins_with("\"") and result.ends_with("\""):
		result = result.substr(1, result.length() - 2)
	
	# 缓存结果
	if context.cache_key != "":
		mDescriptionCache[context.cache_key] = result
	
	return result

func _build_fallback_description(context: DescriptionContext) -> String:
	var display_name = context.display_name if not context.display_name.is_empty() else str(context.get_property("Name", "事物"))
	if context.type == "Crop":
		var stage_name = str(context.get_property("StageName", ""))
		if not stage_name.is_empty():
			return "%s正处于%s。" % [display_name, stage_name]
	return "（%s）" % display_name

## 加载模板（异步，但实际是同步文件读取）
func _load_template_async(type_name: String) -> String:
	# 检查模板缓存
	if mTemplateCache.has(type_name):
		return mTemplateCache[type_name]
	
	# 检查是否已注册
	if not mTemplateRegistry.has(type_name):
		push_warning("[LLMDescriptionService] 未注册模板类型: %s" % type_name)
		return ""
	
	var path: String = mTemplateRegistry[type_name]
	if not FileAccess.file_exists(path):
		push_warning("[LLMDescriptionService] 模板文件不存在: %s" % path)
		return ""
	
	var file := FileAccess.open(path, FileAccess.READ)
	if file == null:
		push_warning("[LLMDescriptionService] 无法打开模板: %s" % path)
		return ""
	
	var content := file.get_as_text()
	file.close()
	
	# 缓存模板
	mTemplateCache[type_name] = content
	return content

## 清除描述缓存
func clear_cache(cache_key: String = "") -> void:
	if cache_key.is_empty():
		mDescriptionCache.clear()
	else:
		mDescriptionCache.erase(cache_key)

## 预加载模板
func preload_template_async(type_name: String) -> void:
	await _load_template_async(type_name)
