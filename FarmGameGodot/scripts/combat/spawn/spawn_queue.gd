class_name SpawnQueue
extends RefCounted

## 生成队列 - 环形缓冲区实现，零 GC

static var instance: SpawnQueue:
	get:
		if instance == null:
			instance = SpawnQueue.new(AtomConstants.SPAWN_QUEUE_CAPACITY)
		return instance

var _buffer: Array[SpawnRequest] = []
var _head: int = 0
var _tail: int = 0
var _count: int = 0
var _capacity: int
var _delayed_list: Array[SpawnRequest] = []

var count: int:
	get: return _count

var delayed_count: int:
	get: return _delayed_list.size()

var total_count: int:
	get: return _count + _delayed_list.size()

var is_empty: bool:
	get: return _count == 0

func _init(capacity: int = 256) -> void:
	_capacity = capacity
	_buffer.resize(capacity)

## 入队请求
func enqueue(request: SpawnRequest) -> void:
	if request.time_remaining() > 0.01:
		_delayed_list.append(request)
		return
	_buffer[_tail] = request
	_tail = (_tail + 1) % _capacity
	if _count == _capacity:
		_head = (_head + 1) % _capacity
		push_warning("[SpawnQueue] 缓冲区已满，丢弃最旧的请求")
	else:
		_count += 1

## 查看队首请求（不移除）
func try_peek() -> SpawnRequest:
	if _count == 0:
		return null
	return _buffer[_head]

## 出队请求
func dequeue() -> SpawnRequest:
	if _count == 0:
		push_error("[SpawnQueue] 从空队列出队")
		return null
	var request := _buffer[_head]
	_head = (_head + 1) % _capacity
	_count -= 1
	return request

## 尝试出队
func try_dequeue() -> SpawnRequest:
	if _count == 0:
		return null
	return dequeue()

## 处理延迟队列
func process_delayed_requests() -> void:
	for i in range(_delayed_list.size() - 1, -1, -1):
		var request := _delayed_list[i]
		if request.is_ready():
			_delayed_list.remove_at(i)
			enqueue(request)

## 清空队列
func clear() -> void:
	_head = 0
	_tail = 0
	_count = 0
	_delayed_list.clear()

## 重置单例
static func reset_instance() -> void:
	if instance != null:
		instance.clear()
	instance = null
