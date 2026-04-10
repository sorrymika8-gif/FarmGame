## 记忆存储
## 一个个体的完整记忆，包含若干分区
## 对应 Unity 的 MemoryStore
class_name MemoryStore
extends RefCounted

var _partitions: Dictionary = {} # name -> MemoryPartition

## 分区数量
var partition_count: int:
	get: return _partitions.size()

## 所有分区的记忆总数
var total_memory_count: int:
	get:
		var count = 0
		for p in _partitions.values():
			count += p.count
		return count

## 创建一个分区（如已存在则返回已有的）
func create_partition(p_name: String) -> MemoryPartition:
	if _partitions.has(p_name):
		return _partitions[p_name]
	var partition = MemoryPartition.new(p_name)
	_partitions[p_name] = partition
	return partition

## 获取一个分区
func get_partition(p_name: String) -> MemoryPartition:
	if _partitions.has(p_name):
		return _partitions[p_name]
	return null

## 获取或创建一个分区
func get_or_create_partition(p_name: String) -> MemoryPartition:
	var p = get_partition(p_name)
	if p == null:
		p = create_partition(p_name)
	return p

## 移除一个分区
func remove_partition(p_name: String) -> bool:
	return _partitions.erase(p_name)

## 获取所有分区名称
func get_partition_names() -> Array[String]:
	var names: Array[String] = []
	for key in _partitions.keys():
		names.append(key)
	return names

## 分区是否存在
func has_partition(p_name: String) -> bool:
	return _partitions.has(p_name)

## 清空所有分区
func clear_all_partitions() -> void:
	_partitions.clear()

## 清空所有分区内的记忆（保留分区结构）
func clear_all_memories() -> void:
	for p in _partitions.values():
		p.clear()

## 获取所有分区的所有记忆
func get_all_memories() -> Array[MemoryItem]:
	var all: Array[MemoryItem] = []
	for p in _partitions.values():
		all.append_array(p.get_all())
	return all

## 搜索记忆（简单关键词匹配）
func search_memories(keyword: String) -> Array:
	var results: Array = []
	for mem in get_all_memories():
		if mem.content.find(keyword) >= 0:
			results.append(mem)
	return results

## 序列化为字典（用于存档）
func to_dict() -> Dictionary:
	var data: Dictionary = {}
	for p_name in _partitions:
		var partition: MemoryPartition = _partitions[p_name]
		var items: Array = []
		for mem in partition.get_all():
			items.append(mem.content)
		data[p_name] = items
	return data

## 从字典恢复（用于加载存档）
func from_dict(data: Dictionary) -> void:
	_partitions.clear()
	for p_name in data:
		var partition := create_partition(p_name)
		var items = data[p_name]
		if items is Array:
			for content in items:
				partition.append(str(content))
