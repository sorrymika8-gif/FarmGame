## 统一指令解析器
## 将 LLM 返回的 JSON 解析为指令字典列表
## 支持所有类型的指令
class_name UnifiedCommandParser
extends RefCounted

## 解析 LLM 输出
func parse(llm_output: String) -> Array:
	var commands: Array = []
	
	if llm_output.strip_edges().is_empty():
		push_warning("[UnifiedCommandParser] LLM输出为空")
		return commands
	
	var json_text = _clean_json_output(llm_output)
	var command_data_list = _parse_json_array(json_text)
	
	for data in command_data_list:
		if data is Dictionary and data.has("type"):
			commands.append(data)
		else:
			push_warning("[UnifiedCommandParser] 指令缺少type字段")
	
	return commands

## 清理JSON输出，移除markdown代码块等
func _clean_json_output(output: String) -> String:
	var result = output.strip_edges()
	
	# 移除markdown代码块标记
	if result.begins_with("```json"):
		result = result.substr(7)
	elif result.begins_with("```"):
		result = result.substr(3)
	
	if result.ends_with("```"):
		result = result.substr(0, result.length() - 3)
	
	return result.strip_edges()

## 解析JSON数组
func _parse_json_array(json_text: String) -> Array:
	var json = JSON.new()
	var error = json.parse(json_text)
	
	if error != OK:
		# 尝试找到 [] 包裹的部分
		var start = json_text.find("[")
		var end = json_text.rfind("]")
		if start >= 0 and end > start:
			var array_text = json_text.substr(start, end - start + 1)
			error = json.parse(array_text)
			if error != OK:
				push_error("[UnifiedCommandParser] JSON解析失败: %s" % json.get_error_message())
				return []
		else:
			push_error("[UnifiedCommandParser] 无效的JSON数组格式")
			return []
	
	var result = json.data
	if result is Array:
		return result
	
	push_warning("[UnifiedCommandParser] JSON解析结果不是数组")
	return []
