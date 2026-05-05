## LLM 客户端
## 负责与 LLM API 通信
## 对应 Unity 的 LLMClient
class_name LLMClient
extends RefCounted

var _provider_name: String = ""
var _base_url: String = ""
var _api_key: String = ""
var _model: String = ""
var _http_client: HTTPRequest = null

func setup(provider: String, base_url: String, api_key: String, model: String) -> void:
	_provider_name = provider
	_base_url = base_url.rstrip("/")
	_api_key = api_key
	_model = model
	print("[LLMClient] 配置完成: provider=%s, model=%s" % [provider, model])

## 发送聊天请求
func chat(messages: Array, model_override: String = "", temperature: float = 0.7, max_tokens: int = 2048) -> Dictionary:
	var config_error = _get_config_error()
	if not config_error.is_empty():
		push_error("[LLMClient] %s" % config_error)
		return {"error": config_error}

	var model = model_override if not model_override.is_empty() else _model

	var headers = _build_headers()
	var body = _build_request_body(messages, model, temperature, max_tokens)
	var url = _get_chat_url()

	# 使用 HTTPClient 发送请求
	var result = await _send_request(url, headers, body)
	return _parse_response(result)

func send_async(prompt: String, system_prompt: String = "") -> Dictionary:
	var messages: Array = []
	if not system_prompt.is_empty():
		messages.append({"role": "system", "content": system_prompt})
	messages.append({"role": "user", "content": prompt})

	var result = await chat(messages)
	if result.has("error"):
		return {"success": false, "error": str(result.get("error", "未知错误"))}

	return {
		"success": true,
		"content": result.get("content", ""),
		"role": result.get("role", "assistant"),
	}

func chat_async(messages_or_system, prompt_or_temperature = 0.7, temperature_or_max_tokens = 2048, max_tokens: int = 2048) -> String:
	var messages: Array = []
	var temperature = 0.7
	var token_limit = 2048

	if messages_or_system is Array:
		messages = messages_or_system
		if prompt_or_temperature is float or prompt_or_temperature is int:
			temperature = float(prompt_or_temperature)
		if temperature_or_max_tokens is int:
			token_limit = int(temperature_or_max_tokens)
	else:
		var system_prompt = str(messages_or_system)
		var user_prompt = str(prompt_or_temperature)
		if not system_prompt.is_empty():
			messages.append({"role": "system", "content": system_prompt})
		messages.append({"role": "user", "content": user_prompt})
		if temperature_or_max_tokens is float or temperature_or_max_tokens is int:
			temperature = float(temperature_or_max_tokens)
		token_limit = max_tokens

	var result = await chat(messages, "", temperature, token_limit)
	if result.has("error"):
		push_warning("[LLMClient] chat_async 失败: %s" % str(result.get("error", "未知错误")))
		return ""
	return result.get("content", "")

# --- 私有方法 ---

func _get_config_error() -> String:
	if _provider_name.strip_edges().is_empty():
		return "provider 未配置"
	if _base_url.strip_edges().is_empty():
		return "base_url 未配置"
	if _model.strip_edges().is_empty():
		return "model 未配置"
	if _api_key.strip_edges().is_empty():
		return "api_key 未配置"
	return ""

func _build_headers() -> PackedStringArray:
	var headers = PackedStringArray()
	headers.append("Content-Type: application/json")
	
	match _provider_name.to_lower():
		"anthropic":
			headers.append("x-api-key: %s" % _api_key)
			headers.append("anthropic-version: 2023-06-01")
		_:
			headers.append("Authorization: Bearer %s" % _api_key)
	
	return headers

func _build_request_body(messages: Array, model: String, temperature: float, max_tokens: int) -> Dictionary:
	match _provider_name.to_lower():
		"anthropic":
			# Anthropic 格式
			var system_msg = ""
			var chat_messages: Array = []
			for msg in messages:
				if msg.get("role", "") == "system":
					system_msg = msg.get("content", "")
				else:
					chat_messages.append(msg)
			
			var body: Dictionary = {
				"model": model,
				"messages": chat_messages,
				"max_tokens": max_tokens,
				"temperature": temperature,
			}
			if not system_msg.is_empty():
				body["system"] = system_msg
			return body
		_:
			# OpenAI 兼容格式（包括 DeepSeek）
			return {
				"model": model,
				"messages": messages,
				"max_tokens": max_tokens,
				"temperature": temperature,
			}

func _get_chat_url() -> String:
	if _base_url.ends_with("/chat/completions") or _base_url.ends_with("/messages"):
		return _base_url
	if _provider_name.to_lower() == "deepseek":
		return _base_url + "/chat/completions"
	if _base_url.ends_with("/v1"):
		match _provider_name.to_lower():
			"anthropic":
				return _base_url + "/messages"
			_:
				return _base_url + "/chat/completions"

	match _provider_name.to_lower():
		"anthropic":
			return _base_url + "/v1/messages"
		_:
			return _base_url + "/v1/chat/completions"

func _send_request(url: String, headers: PackedStringArray, body: Dictionary) -> Dictionary:
	# 创建 HTTPRequest 节点
	var scene_tree = Engine.get_main_loop() as SceneTree
	if scene_tree == null:
		push_error("[LLMClient] 无法获取 SceneTree")
		return {"error": "No SceneTree"}
	
	var http = HTTPRequest.new()
	scene_tree.root.add_child(http)
	
	var json_body = JSON.stringify(body)
	var error = http.request(url, headers, HTTPClient.METHOD_POST, json_body)
	
	if error != OK:
		http.queue_free()
		push_error("[LLMClient] HTTP 请求失败: %d" % error)
		return {"error": "Request failed"}
	
	# 等待响应
	var response = await http.request_completed
	http.queue_free()
	
	var result_code = response[0]
	var response_code = response[1]
	var response_headers = response[2]
	var response_body = response[3] as PackedByteArray
	
	if result_code != HTTPRequest.RESULT_SUCCESS:
		return {"error": "HTTP error: %d" % result_code}
	
	var json = JSON.new()
	if json.parse(response_body.get_string_from_utf8()) != OK:
		return {"error": "JSON parse error"}
	
	if not json.data is Dictionary:
		return {"error": "Invalid response"}

	if response_code < 200 or response_code >= 300:
		return {"error": "HTTP %d: %s" % [response_code, _extract_error_message(json.data)]}

	return json.data

func _extract_error_message(data: Dictionary) -> String:
	var error_data = data.get("error", "")
	if error_data is Dictionary:
		return str(error_data.get("message", error_data))
	return str(error_data)

func _parse_response(response: Dictionary) -> Dictionary:
	if response.has("error"):
		return response
	
	match _provider_name.to_lower():
		"anthropic":
			var content_blocks = response.get("content", [])
			var text = ""
			for block in content_blocks:
				if block.get("type", "") == "text":
					text += block.get("text", "")
			return {"content": text, "role": "assistant"}
		_:
			var choices = response.get("choices", [])
			if choices.size() > 0:
				var message = choices[0].get("message", {})
				var content = str(message.get("content", ""))
				if content.is_empty():
					content = str(message.get("reasoning_content", ""))
				return {
					"content": _remove_thinking_tags(content),
					"role": message.get("role", "assistant")
				}
			return {"content": "", "role": "assistant"}

func _remove_thinking_tags(content: String) -> String:
	var result = content
	while result.find("<think>") != -1 and result.find("</think>") != -1:
		var start = result.find("<think>")
		var end = result.find("</think>", start)
		if end == -1:
			break
		result = result.substr(0, start) + result.substr(end + "</think>".length())
	return result.strip_edges()
