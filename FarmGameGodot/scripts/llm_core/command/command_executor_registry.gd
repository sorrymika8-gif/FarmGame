## 指令执行器注册表
## 管理所有指令类型对应的执行器
class_name CommandExecutorRegistry
extends RefCounted

## 指令类型 -> 执行器回调 callable(command: Dictionary, context: DecisionContext)
var _executors: Dictionary = {}

## 注册一个执行器
func register(command_type: String, executor: Callable) -> void:
	_executors[command_type] = executor

## 获取指定指令类型的执行器
func get_executor(command_type: String) -> Callable:
	if _executors.has(command_type):
		return _executors[command_type]
	return Callable()

## 是否有指定类型的执行器
func has_executor(command_type: String) -> bool:
	return _executors.has(command_type)

## 执行一条指令
func execute(command: Dictionary, context: DecisionContext) -> void:
	var cmd_type = command.get("type", "")
	if cmd_type.is_empty():
		push_warning("[CommandExecutorRegistry] 指令缺少type字段")
		return
	
	var executor = get_executor(cmd_type)
	if not executor.is_valid():
		push_warning("[CommandExecutorRegistry] 未找到指令执行器: %s" % cmd_type)
		return
	
	executor.call(command, context)
