## 大脑核心
## 协调 提示词构建 -> LLM 调用 -> 指令解析 的流程
## 对应 Unity 的 Brain
class_name Brain
extends RefCounted

var _prompt_builder: UnifiedPromptBuilder
var _command_parser: UnifiedCommandParser
var _executor_registry: CommandExecutorRegistry
var _command_queue: CommandQueue

## 获取指令执行器注册表
var executor_registry: CommandExecutorRegistry:
	get: return _executor_registry

## 指令队列中的指令数量
var pending_command_count: int:
	get: return _command_queue.count

func _init() -> void:
	_prompt_builder = UnifiedPromptBuilder.new()
	_command_parser = UnifiedCommandParser.new()
	_executor_registry = CommandExecutorRegistry.new()
	_command_queue = CommandQueue.new(_executor_registry)

## 注册所有内置执行器
func register_all_executors() -> void:
	CommandExecutors.register_all(_executor_registry)

## 执行决策（构建提示词 -> LLM -> 解析指令）
func decide_async(context: DecisionContext) -> DecisionResult:
	var start_time = Time.get_ticks_msec()
	var result = DecisionResult.new()
	
	# 1. 构建提示词
	var prompt = _prompt_builder.build(context)
	if prompt.is_empty():
		result.success = false
		result.error_message = "提示词构建失败"
		return result
	
	# 2. 调用 LLM
	if LLMService.client == null:
		result.success = false
		result.error_message = "LLMService Client 未初始化"
		return result
	
	var llm_response = await LLMService.client.send_async(prompt)
	
	if not llm_response.get("success", false):
		result.success = false
		result.error_message = llm_response.get("error", "LLM调用失败")
		return result
	
	var content = llm_response.get("content", "")
	result.raw_output = content
	
	# 3. 解析指令
	var commands = _command_parser.parse(content)
	result.commands = commands
	result.success = true
	result.processing_time = (Time.get_ticks_msec() - start_time) / 1000.0
	
	return result

## 执行决策并将指令加入队列
func decide_and_enqueue_async(context: DecisionContext) -> DecisionResult:
	var result = await decide_async(context)
	
	if result.success and result.commands.size() > 0:
		_command_queue.enqueue_range(result.commands, context)
	
	return result

## 处理指令队列中的所有指令
func process_command_queue() -> void:
	_command_queue.process_all()

## 处理指令队列中的下一个指令
func process_next_command() -> bool:
	return _command_queue.process_next()

## 直接执行指令列表（不经过队列）
func execute_commands(commands: Array, context: DecisionContext) -> void:
	for cmd in commands:
		_executor_registry.execute(cmd, context)

## 清理
func dispose() -> void:
	_command_queue.clear()

