## NPC 实体
## 包含 NPC 的身份、状态、记忆和指令队列
## 对应 Unity 的 NPCEntity
class_name NPCEntity
extends RefCounted

# ===== 身份信息 =====
var id: String = ""
var npc_name: String = ""
var gender: String = "未知"

# ===== 初始设定 =====
var personality: String = ""
var background: String = ""
var appearance: String = ""
var prompt_file_path: String = "" ## NPC专属提示词文件名
var role: String = "npc"
var shop_type: int = 0

# ===== 动态状态 =====
var position: Vector2 = Vector2.ZERO
var interaction_distance: float = 2.0
var health: float = 100.0
var hunger: float = 0.0
var fatigue: float = 0.0
var emotion: String = "neutral"
var current_activity: String = "idle"

## 当前表情ID（用于立绘显示）
var current_expression: String = "default"
## 当前心情emoji（用于气泡对话显示）
var current_mood: String = ""

# ===== 信号 =====
signal expression_changed(old_expression: String, new_expression: String)
signal mood_changed(old_mood: String, new_mood: String)

# ===== 核心组件 =====
var memory_store: MemoryStore
var command_queue: CommandQueue
var inventory: InventoryComponent

func _init(p_id: String = "", p_name: String = "", executor_registry: CommandExecutorRegistry = null) -> void:
	id = p_id
	npc_name = p_name
	
	memory_store = MemoryStore.new()
	_initialize_memory_partitions()
	
	inventory = InventoryComponent.new()
	
	if executor_registry:
		command_queue = CommandQueue.new(executor_registry)
	elif NPCManager.shared_brain:
		command_queue = CommandQueue.new(NPCManager.shared_brain.executor_registry)

func _initialize_memory_partitions() -> void:
	memory_store.create_partition("short_term")
	memory_store.create_partition("long_term")
	memory_store.create_partition("permanent")

## 初始化记忆
func initialize_memories(initial_memories: Array[String]) -> void:
	var permanent = memory_store.get_partition("permanent")
	if permanent:
		for mem in initial_memories:
			permanent.append(mem)

## 设置表情
func set_expression(p_expression: String) -> void:
	if p_expression.is_empty() or current_expression == p_expression:
		return
	var old = current_expression
	current_expression = p_expression
	expression_changed.emit(old, p_expression)

## 设置心情
func set_mood(emoji: String) -> void:
	var old = current_mood
	current_mood = emoji if emoji else ""
	if old != current_mood:
		mood_changed.emit(old, current_mood)

## 清除心情
func clear_mood() -> void:
	set_mood("")

## 记录行为到短期记忆
func record_memory(content: String) -> void:
	var short_term = memory_store.get_partition("short_term")
	if short_term:
		short_term.append(content)

## 接收聊天信息并触发思考
func receive_chat_async(content: String):
	print("[玩家] %s" % content)
	
	# 1. 将玩家消息存入短期记忆
	record_memory("玩家对我说：%s" % content)
	
	# 2. 构建决策上下文
	var context = DecisionContext.new()
	context.decision_type = "Chat"
	context.memory_store = memory_store
	context.trigger_event = "ReceiveChat"
	context.character_profile = build_character_profile()
	context.current_state = build_current_state()
	context.extra["NPCEntity"] = self
	context.extra["PromptFilePath"] = prompt_file_path
	
	# 3. 触发大脑思考
	var brain = NPCManager.shared_brain
	if brain == null:
		push_error("[NPCEntity] SharedBrain is null!")
		var empty_result = DecisionResult.new()
		empty_result.success = false
		empty_result.error_message = "SharedBrain is null"
		return empty_result
	
	print("[NPCEntity] %s 开始调用 LLM 进行决策..." % npc_name)
	var result = await brain.decide_async(context)
	
	if not result.success:
		push_error("[NPCEntity] LLM 决策失败: %s" % result.error_message)
		return result
	
	print("[NPCEntity] LLM 原始输出: %s" % result.raw_output)
	
	if result.commands.is_empty():
		push_warning("[NPCEntity] LLM 返回成功但没有解析到任何指令")
		return result
	
	print("[NPCEntity] 解析到 %d 条指令" % result.commands.size())
	brain.execute_commands(result.commands, context)
	return result

## 构建角色设定字典
func build_character_profile() -> Dictionary:
	return {
		"ID": id,
		"Name": npc_name,
		"Gender": gender,
		"Personality": personality,
		"Background": background,
		"Appearance": appearance,
		"Role": role,
	}

## 构建当前状态字典
func build_current_state() -> Dictionary:
	return {
		"Position": str(position),
		"Health": health,
		"Hunger": hunger,
		"Fatigue": fatigue,
		"Emotion": emotion,
		"CurrentActivity": current_activity,
	}

## 序列化为字典
func to_dict() -> Dictionary:
	return {
		"id": id,
		"name": npc_name,
		"gender": gender,
		"personality": personality,
		"background": background,
		"position_x": position.x,
		"position_y": position.y,
		"health": health,
		"emotion": emotion,
		"role": role,
		"shop_type": shop_type,
	}
