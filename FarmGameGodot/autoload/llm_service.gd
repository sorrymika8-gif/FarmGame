## LLM 服务
## 提供 LLM API 调用的统一接口
extends Node

var _is_initialized: bool = false
var _client = null # LLMClient 实例
var _default_model: String = ""
var _default_api_key: String = ""
var _default_base_url: String = ""
var _default_provider: String = ""

## 获取 LLM 客户端
var client:
	get:
		return _client

func initialize() -> void:
	if _is_initialized:
		return
	
	# 从配置中读取 LLM 设置
	var settings = ConfigManager.get_all("llm_settings")
	if settings.size() > 0:
		for setting in settings:
			var key = setting.get("key", "")
			var value = setting.get("value", "")
			match key:
				"model":
					_default_model = value
				"api_key":
					_default_api_key = value
				"base_url":
					_default_base_url = value
				"provider":
					_default_provider = value
	
	# 创建客户端
	_client = LLMClient.new()
	_client.setup(_default_provider, _default_base_url, _default_api_key, _default_model)
	
	_is_initialized = true
	print("[LLMService] 初始化完成, provider=%s, model=%s" % [_default_provider, _default_model])

## 发送聊天请求
func chat(messages: Array, model_override: String = "") -> Dictionary:
	if _client == null:
		push_error("[LLMService] 客户端未初始化")
		return {}
	return await _client.chat(messages, model_override)

## 发送简单文本请求
func ask(prompt: String, system_prompt: String = "") -> String:
	if _client == null:
		push_error("[LLMService] 客户端未初始化")
		return ""
	
	var messages: Array = []
	if not system_prompt.is_empty():
		messages.append({"role": "system", "content": system_prompt})
	messages.append({"role": "user", "content": prompt})
	
	var result = await _client.chat(messages)
	return result.get("content", "")
