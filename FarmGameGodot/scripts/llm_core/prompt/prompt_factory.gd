## 提示工厂
## 负责构建各种类型的 LLM 提示
## 对应 Unity 的 PromptFactory
class_name PromptFactory
extends RefCounted

## 加载提示模板文件
static func load_template(template_name: String) -> String:
	var path = "res://prompts/" + template_name + ".txt"
	if FileAccess.file_exists(path):
		var file = FileAccess.open(path, FileAccess.READ)
		if file:
			var content = file.get_as_text()
			file.close()
			return content
	push_warning("[PromptFactory] 模板不存在: %s" % template_name)
	return ""

## 替换模板中的占位符
static func fill_template(template: String, variables: Dictionary) -> String:
	var result = template
	for key in variables:
		result = result.replace("{%s}" % key, str(variables[key]))
	return result

## 构建聊天提示
static func build_chat_prompt(npc_name: String, personality: String, context: Dictionary) -> String:
	var prompt = "你是 %s。%s\n" % [npc_name, personality]
	
	if context.has("location"):
		prompt += "当前位置: %s\n" % context["location"]
	if context.has("time"):
		prompt += "当前时间: %s\n" % context["time"]
	if context.has("weather"):
		prompt += "当前天气: %s\n" % context["weather"]
	
	return prompt

## 构建行为决策提示
static func build_behavior_prompt(npc_name: String, available_actions: Array, context: Dictionary) -> String:
	var prompt = "你是 %s，请根据当前情境选择合适的行为。\n" % npc_name
	prompt += "可用动作: %s\n" % ", ".join(available_actions)
	prompt += "当前情境:\n"
	for key in context:
		prompt += "- %s: %s\n" % [key, str(context[key])]
	prompt += "\n请以 JSON 格式回复你的决策。"
	return prompt

## 构建记忆整理提示
static func build_memory_organize_prompt(memories: Array) -> String:
	var prompt = "以下是一些记忆内容，请帮我整理和总结：\n\n"
	for i in range(memories.size()):
		var mem = memories[i]
		prompt += "%d. [%s] %s\n" % [i + 1, mem.get("type", ""), mem.get("content", "")]
	prompt += "\n请总结这些记忆的关键要点。"
	return prompt
