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
	
	_load_settings()
	
	# 创建客户端
	_client = LLMClient.new()
	_client.setup(_default_provider, _default_base_url, _default_api_key, _default_model)
	
	_is_initialized = true
	print("[LLMService] 初始化完成, provider=%s, model=%s" % [_default_provider, _default_model])

func get_client():
	return _client

## 发送聊天请求
func chat(messages: Array, model_override: String = "", temperature: float = 0.7, max_tokens: int = 2048) -> Dictionary:
	if _client == null:
		push_error("[LLMService] 客户端未初始化")
		return {}
	return await _client.chat(messages, model_override, temperature, max_tokens)

## 发送简单文本请求
func ask(prompt: String, system_prompt: String = "", temperature: float = 0.7, max_tokens: int = 2048) -> String:
	if _client == null:
		push_error("[LLMService] 客户端未初始化")
		return ""
	
	var messages: Array = []
	if not system_prompt.is_empty():
		messages.append({"role": "system", "content": system_prompt})
	messages.append({"role": "user", "content": prompt})

	var result = await _client.chat(messages, "", temperature, max_tokens)
	if result.has("error"):
		push_warning("[LLMService] 请求失败: %s" % str(result.get("error", "未知错误")))
		return ""
	return result.get("content", "")

func _load_settings() -> void:
	var settings = ConfigManager.get_all("llm_settings")
	if settings.is_empty():
		push_warning("[LLMService] 未找到 llm_settings 配置")
		return

	var selected_setting: Dictionary = {}
	for setting in settings:
		if bool(setting.get("enabled", false)):
			selected_setting = setting
			break

	if selected_setting.is_empty():
		selected_setting = settings[0]

	_default_provider = str(selected_setting.get("provider_type", selected_setting.get("provider", ""))).strip_edges()
	_default_model = str(selected_setting.get("default_model", selected_setting.get("model", ""))).strip_edges()
	_default_base_url = str(selected_setting.get("base_url", "")).strip_edges()
	_apply_provider_defaults()
	_default_api_key = _resolve_api_key(str(selected_setting.get("api_key", "")), _default_provider)

func _apply_provider_defaults() -> void:
	match _default_provider.to_lower():
		"deepseek":
			if _default_base_url.is_empty():
				_default_base_url = "https://api.deepseek.com"
			if _default_model.is_empty():
				_default_model = "deepseek-chat"
		"openai":
			if _default_base_url.is_empty():
				_default_base_url = "https://api.openai.com/v1"
			if _default_model.is_empty():
				_default_model = "gpt-4o-mini"
		"anthropic":
			if _default_base_url.is_empty():
				_default_base_url = "https://api.anthropic.com/v1"
			if _default_model.is_empty():
				_default_model = "claude-3-5-sonnet-20240620"

func _resolve_api_key(config_value: String, provider: String) -> String:
	var trimmed_value = config_value.strip_edges()
	if not trimmed_value.is_empty():
		return trimmed_value

	var provider_name = provider.to_lower()
	var env_names: Array[String] = ["LLM_API_KEY"]
	match provider_name:
		"deepseek":
			env_names.push_front("DEEPSEEK_API_KEY")
		"openai":
			env_names.push_front("OPENAI_API_KEY")
		"anthropic":
			env_names.push_front("ANTHROPIC_API_KEY")

	for env_name in env_names:
		var env_value = OS.get_environment(env_name).strip_edges()
		if not env_value.is_empty():
			return env_value

	push_warning("[LLMService] API Key 为空，请在 configs/llm_settings.json 或环境变量中配置")
	return ""
