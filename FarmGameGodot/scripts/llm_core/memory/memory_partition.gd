## 记忆分区
## 一个存放记忆的区域，记忆按产生顺序排列
class_name MemoryPartition
extends RefCounted

var _name: String = ""
var _memories: Array[MemoryItem] = []

## 分区名称
var partition_name: String:
	get: return _name

## 记忆数量
var count: int:
	get: return _memories.size()

## 分区是否为空
var is_empty: bool:
	get: return _memories.is_empty()

func _init(p_name: String = "") -> void:
	_name = p_name

## 追加一条记忆到分区末尾
func append(content: String) -> MemoryItem:
	var memory = MemoryItem.new(content)
	_memories.append(memory)
	return memory

## 添加一条记忆（MemoryItem 对象）
func add(memory: MemoryItem) -> void:
	_memories.append(memory)

## 获取所有记忆
func get_all() -> Array[MemoryItem]:
	return _memories

## 移除指定的记忆
func remove(memory: MemoryItem) -> bool:
	var idx = _memories.find(memory)
	if idx >= 0:
		_memories.remove_at(idx)
		return true
	return false

## 移除指定位置的记忆
func remove_at(index: int) -> void:
	if index >= 0 and index < _memories.size():
		_memories.remove_at(index)

## 清空分区
func clear() -> void:
	_memories.clear()

## 获取指定位置的记忆
func get_at(index: int) -> MemoryItem:
	if index >= 0 and index < _memories.size():
		return _memories[index]
	return null

## 更新指定位置的记忆
func update_at(index: int, new_content: String) -> void:
	if index >= 0 and index < _memories.size():
		_memories[index] = MemoryItem.new(new_content)
