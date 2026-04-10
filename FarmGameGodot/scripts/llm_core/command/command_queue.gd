## 命令队列
## 管理指令的FIFO执行
## 对应 Unity 的 CommandQueue
class_name CommandQueue
extends RefCounted

var _queue: Array = [] # Array of {command: Dictionary, context: DecisionContext}
var _executor_registry: CommandExecutorRegistry

## 队列长度
var count: int:
	get: return _queue.size()

func _init(registry: CommandExecutorRegistry = null) -> void:
	_executor_registry = registry

## 添加指令到队列
func enqueue(command: Dictionary, context: DecisionContext) -> void:
	_queue.append({"command": command, "context": context})

## 批量添加指令
func enqueue_range(commands: Array, context: DecisionContext) -> void:
	for cmd in commands:
		enqueue(cmd, context)

## 处理队列中的下一个指令
func process_next() -> bool:
	if _queue.is_empty():
		return false
	
	var entry = _queue.pop_front()
	var command = entry["command"]
	var context = entry["context"]
	
	if _executor_registry:
		_executor_registry.execute(command, context)
	else:
		push_warning("[CommandQueue] 没有设置执行器注册表")
	
	return true

## 处理队列中的所有指令
func process_all() -> void:
	while not _queue.is_empty():
		process_next()

## 清空队列
func clear() -> void:
	_queue.clear()

## 是否为空
func is_empty() -> bool:
	return _queue.is_empty()
