## 统一决策提示词构建器
## 用于所有类型的决策场景
## 采用模块化设计：通用模块 + NPC专属人设
class_name UnifiedPromptBuilder
extends RefCounted

const NPC_PROMPTS_DIR = "res://prompts/npcs/"
const COMMON_PROMPTS_DIR = "res://prompts/common/"
const MAX_SHORT_TERM = 20
const MAX_LONG_TERM = 10
const MAX_PERMANENT = 10

## 通用模块缓存
static var _common_module_cache: Dictionary = {}

## 初始化通用模块缓存
static func initialize_cache() -> void:
	_common_module_cache.clear()
	var module_files = ["BaseIdentity.md", "StatePerception.md", "MemorySystem.md", "DecisionRules.md", "OutputFormat.md"]
	for file_name in module_files:
		var path = COMMON_PROMPTS_DIR + file_name
		if FileAccess.file_exists(path):
			var file = FileAccess.open(path, FileAccess.READ)
			if file:
				_common_module_cache[file_name] = file.get_as_text()
				file.close()
				print("[UnifiedPromptBuilder] 已缓存通用模块: %s" % file_name)
		else:
			push_warning("[UnifiedPromptBuilder] 通用模块文件不存在: %s" % path)

## 加载通用模块内容
func _load_common_module(file_name: String) -> String:
	if _common_module_cache.has(file_name):
		return _common_module_cache[file_name]
	
	var path = COMMON_PROMPTS_DIR + file_name
	if FileAccess.file_exists(path):
		var file = FileAccess.open(path, FileAccess.READ)
		if file:
			var content = file.get_as_text()
			file.close()
			return content
	push_warning("[UnifiedPromptBuilder] 通用模块文件不存在: %s" % path)
	return ""

## 加载NPC专属人设文件
func _load_npc_prompt(prompt_file_name: String) -> String:
	if prompt_file_name.is_empty():
		push_error("[UnifiedPromptBuilder] NPC未配置专属提示词文件！")
		return ""
	
	var path = NPC_PROMPTS_DIR + prompt_file_name
	if not FileAccess.file_exists(path):
		push_error("[UnifiedPromptBuilder] 找不到NPC专属提示词文件: %s" % path)
		return ""
	
	var file = FileAccess.open(path, FileAccess.READ)
	if file:
		var content = file.get_as_text()
		file.close()
		return content
	return ""

## 构建提示词
func build(context: DecisionContext) -> String:
	var prompt_file_name = context.extra.get("PromptFilePath", "")
	var sb = ""
	
	# 1. 基础身份说明
	sb += _load_common_module("BaseIdentity.md") + "\n\n"
	
	# 2. NPC专属人设
	var npc_prompt = _load_npc_prompt(prompt_file_name)
	if npc_prompt.is_empty():
		return ""
	sb += npc_prompt + "\n\n"
	
	# 3. 状态和感知
	sb += _load_common_module("StatePerception.md") + "\n\n"
	
	# 4. 记忆系统
	sb += _load_common_module("MemorySystem.md") + "\n\n"
	
	# 5. 可用行为和表情
	sb += "## 可用行为\n"
	var available_actions = context.extra.get("AvailableActions")
	if available_actions is Array:
		sb += ActionHintLoader.get_action_hints(available_actions)
	else:
		sb += ActionHintLoader.get_all_action_hints()
	sb += "\n\n## 可用表情\n"
	sb += ExpressionHintLoader.get_all_expression_hints()
	sb += "\n\n"
	
	# 6. 通用决策规则
	sb += _load_common_module("DecisionRules.md") + "\n\n"
	
	# 7. 输出格式
	sb += _load_common_module("OutputFormat.md")
	
	# 替换所有占位符
	return _replace_placeholders(sb, context)

## 替换提示词中的所有占位符
func _replace_placeholders(template: String, context: DecisionContext) -> String:
	var character_profile = _build_dict_string(context.character_profile, "无特定设定")
	var current_state = _build_dict_string(context.current_state, "无状态信息")
	var perception_str = _build_dict_string(context.perception, "无特别感知")
	var short_term = _build_memory_string(context.memory_store, "short_term", MAX_SHORT_TERM, "无最近经历")
	var long_term = _build_memory_string(context.memory_store, "long_term", MAX_LONG_TERM, "无重要记忆")
	var permanent = _build_memory_string(context.memory_store, "permanent", MAX_PERMANENT, "无刻骨铭心的记忆")
	var trigger_event = context.trigger_event if not context.trigger_event.is_empty() else "无特定触发事件"
	
	return template \
		.replace("{{CHARACTER_PROFILE}}", character_profile) \
		.replace("{{CURRENT_STATE}}", current_state) \
		.replace("{{PERCEPTION}}", perception_str) \
		.replace("{{SHORT_TERM_MEMORIES}}", short_term) \
		.replace("{{LONG_TERM_MEMORIES}}", long_term) \
		.replace("{{PERMANENT_MEMORIES}}", permanent) \
		.replace("{{TRIGGER_EVENT}}", trigger_event)

## 构建字典的显示字符串
func _build_dict_string(dict: Dictionary, empty_message: String) -> String:
	if dict.is_empty():
		return "- %s" % empty_message
	var result = ""
	for key in dict:
		result += "- %s: %s\n" % [key, str(dict[key])]
	return result.strip_edges()

## 从指定分区构建记忆字符串
func _build_memory_string(store: MemoryStore, partition_name: String, max_count: int, empty_message: String) -> String:
	if store == null:
		return "- %s" % empty_message
	
	var partition = store.get_partition(partition_name)
	if partition == null or partition.count == 0:
		return "- %s" % empty_message
	
	var memories = partition.get_all()
	var result = ""
	var start_idx = max(0, memories.size() - max_count)
	for i in range(start_idx, memories.size()):
		result += "- %s\n" % memories[i].content
	return result.strip_edges()
