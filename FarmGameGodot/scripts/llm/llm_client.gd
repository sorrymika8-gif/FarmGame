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
func chat(messages: Array, model_override: String = "") -> Dictionary:
	var model = model_override if not model_override.is_empty() else _model
	
	var headers = _build_headers()
	var body = _build_request_body(messages, model)
	var url = _get_chat_url()
	
	# 使用 HTTPClient 发送请求
	var result = await _send_request(url, headers, body)
	return _parse_response(result)

# --- 私有方法 ---

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

func _build_request_body(messages: Array, model: String) -> Dictionary:
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
				"max_tokens": 2048,
			}
			if not system_msg.is_empty():
				body["system"] = system_msg
			return body
		_:
			# OpenAI 兼容格式（包括 DeepSeek）
			return {
				"model": model,
				"messages": messages,
				"max_tokens": 2048,
			}

func _get_chat_url() -> String:
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
	
	return json.data if json.data is Dictionary else {"error": "Invalid response"}

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
				return {
					"content": message.get("content", ""),
					"role": message.get("role", "assistant")
				}
			return {"content": "", "role": "assistant"}
