## LLM 类型定义
## 对应 Unity 的 LLMTypes
class_name LLMTypes
extends RefCounted

## 聊天消息
class ChatMessage:
	var role: String = ""
	var content: String = ""
	
	func _init(p_role: String = "", p_content: String = "") -> void:
		role = p_role
		content = p_content
	
	func to_dict() -> Dictionary:
		return {"role": role, "content": content}
	
	static func system(content: String) -> ChatMessage:
		return ChatMessage.new("system", content)
	
	static func user(content: String) -> ChatMessage:
		return ChatMessage.new("user", content)
	
	static func assistant(content: String) -> ChatMessage:
		return ChatMessage.new("assistant", content)

## LLM 响应
class LLMResponse:
	var content: String = ""
	var role: String = "assistant"
	var is_success: bool = false
	var error_message: String = ""
	
	func _init(p_content: String = "", p_success: bool = true) -> void:
		content = p_content
		is_success = p_success

## Provider 类型枚举
enum ProviderType {
	OPENAI,
	DEEPSEEK,
	ANTHROPIC,
}
