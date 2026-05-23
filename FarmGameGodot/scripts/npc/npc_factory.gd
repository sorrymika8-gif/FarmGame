## NPC 工厂
## 使用配置数据创建 NPC 实体
class_name NPCFactory
extends RefCounted

## 从配置字典创建 NPC 实体
static func create(config: Dictionary, brain: Brain) -> NPCEntity:
	var entity = NPCEntity.new(
		str(config.get("class_id", "")),
		config.get("name", ""),
		brain.executor_registry if brain else null
	)
	
	# 设置属性
	entity.gender = config.get("gender", "未知")
	entity.role = config.get("role", "npc")
	entity.shop_type = int(config.get("shop_type", 0))
	
	# 设置初始位置
	var init_pos = config.get("init_pos", [])
	if init_pos is Array and init_pos.size() >= 2:
		entity.position = MapManager.grid_to_world(Vector2i(int(init_pos[0]), int(init_pos[1])))
	
	# 设置交互距离
	var interaction_dis = config.get("interaction_dis", 0)
	entity.interaction_distance = float(interaction_dis) * MapManager.tile_size if interaction_dis > 0 else 2.0 * MapManager.tile_size
	
	# 设置提示词文件路径
	entity.prompt_file_path = config.get("prompt", "")
	entity.current_location_id = str(config.get("location_id", ""))

	var initial_memories = config.get("initial_memories", [])
	if initial_memories is Array:
		for memory in initial_memories:
			entity.record_memory(str(memory))
	
	return entity
