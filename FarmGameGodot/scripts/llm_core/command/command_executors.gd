## 指令执行器集合
## 包含所有内置指令执行器的实现
## 每个执行器是一个静态方法，签名为 (command: Dictionary, context: DecisionContext)
class_name CommandExecutors
extends RefCounted

# ===== Signals =====
# 用信号替代 C# 的静态事件

## 气泡说话 (speaker_node: Node, content: String, mood: String)
static var on_bubble_speak: Array[Callable] = []
## 对话框说话 (npc_entity: NPCEntity, content: String)
static var on_dialogue_speak: Array[Callable] = []
## 攻击事件 (attacker_node: Node, target_id: String)
static var on_attack: Array[Callable] = []
## 状态变更事件 (character_node: Node, key: String, value: String)
static var on_state_changed: Array[Callable] = []
## 表情变更事件 (npc_entity: NPCEntity, expression: String)
static var on_expression_changed: Array[Callable] = []
## 心情变更事件 (npc_entity: NPCEntity, emoji: String)
static var on_mood_changed: Array[Callable] = []

# ===== 注册所有执行器到 Registry =====

static func register_all(registry: CommandExecutorRegistry) -> void:
	registry.register(CommandTypes.Move, CommandExecutors._execute_move)
	registry.register(CommandTypes.Speak, CommandExecutors._execute_speak)
	registry.register(CommandTypes.Attack, CommandExecutors._execute_attack)
	registry.register(CommandTypes.SetState, CommandExecutors._execute_set_state)
	registry.register(CommandTypes.SetExpression, CommandExecutors._execute_set_expression)
	registry.register(CommandTypes.SetMood, CommandExecutors._execute_set_mood)
	registry.register(CommandTypes.MemoryOperation, CommandExecutors._execute_memory_operation)
	registry.register(CommandTypes.Till, CommandExecutors._execute_till)
	registry.register(CommandTypes.Plant, CommandExecutors._execute_plant)
	registry.register(CommandTypes.Harvest, CommandExecutors._execute_harvest)

# ===== 移动执行器 =====

static func _execute_move(command: Dictionary, context: DecisionContext) -> void:
	var target_x = command.get("x", 0.0)
	var target_y = command.get("y", 0.0)
	var target_pos = Vector2(target_x, target_y)
	
	# 从上下文获取 Movable
	var movable = context.extra.get("Movable")
	if movable and movable.has_method("move_to"):
		movable.move_to(target_pos)
	
	# 记录到短期记忆
	var npc = context.extra.get("NPCEntity")
	if npc and npc.has_method("record_memory"):
		npc.record_memory("我移动到了位置 (%s, %s)" % [target_x, target_y])
	
	print("[MoveExecutor] 开始移动到 (%s, %s)" % [target_x, target_y])

# ===== 说话执行器 =====

static func _execute_speak(command: Dictionary, context: DecisionContext) -> void:
	var content = command.get("content", "")
	if content.is_empty():
		push_warning("[SpeakExecutor] 说话内容为空")
		return
	
	var npc = context.extra.get("NPCEntity")
	var speaker = context.extra.get("Node")
	
	# 根据TriggerEvent判断说话模式
	var trigger = context.trigger_event
	var is_dialogue = trigger.contains("Chat") or trigger.contains("Dialogue") or \
		trigger.contains("Talk") or trigger == "ReceiveChat" or trigger == "PlayerInteract"
	
	if is_dialogue and npc != null:
		# 正式对话模式
		for cb in on_dialogue_speak:
			if cb.is_valid():
				cb.call(npc, content)
	else:
		# 气泡模式
		var mood = ""
		if npc and npc.get("current_mood"):
			mood = npc.current_mood
		for cb in on_bubble_speak:
			if cb.is_valid():
				cb.call(speaker, content, mood)
		# 清除心情
		if npc and npc.has_method("clear_mood"):
			npc.clear_mood()
	
	# 记录到短期记忆
	if npc and npc.has_method("record_memory"):
		npc.record_memory("我说：%s" % content)
	
	print("[SpeakExecutor] %s" % content)

# ===== 攻击执行器 =====

static func _execute_attack(command: Dictionary, context: DecisionContext) -> void:
	var target_id = command.get("targetId", "")
	if target_id.is_empty():
		push_warning("[AttackExecutor] 攻击目标ID为空")
		return
	
	var attacker = context.extra.get("Node")
	for cb in on_attack:
		if cb.is_valid():
			cb.call(attacker, target_id)
	
	print("[AttackExecutor] 攻击目标: %s" % target_id)

# ===== 设置状态执行器 =====

static func _execute_set_state(command: Dictionary, context: DecisionContext) -> void:
	var key = command.get("key", "")
	var value = command.get("value", "")
	if key.is_empty():
		push_warning("[SetStateExecutor] 状态Key为空")
		return
	
	# 更新上下文中的状态
	if context.current_state != null:
		context.current_state[key] = value
	
	var character = context.extra.get("Node")
	for cb in on_state_changed:
		if cb.is_valid():
			cb.call(character, key, value)
	
	print("[SetStateExecutor] 状态变更: %s = %s" % [key, value])

# ===== 设置表情执行器 =====

static func _execute_set_expression(command: Dictionary, context: DecisionContext) -> void:
	var expression = command.get("expression", "")
	if expression.is_empty():
		push_warning("[SetExpressionExecutor] 表情ID为空")
		return
	
	var npc = context.extra.get("NPCEntity")
	if npc == null:
		push_warning("[SetExpressionExecutor] 无法从上下文获取NPCEntity")
		return
	
	if npc.has_method("set_expression"):
		npc.set_expression(expression)
	
	for cb in on_expression_changed:
		if cb.is_valid():
			cb.call(npc, expression)
	
	print("[SetExpressionExecutor] 表情变更: %s" % expression)

# ===== 设置心情执行器 =====

static func _execute_set_mood(command: Dictionary, context: DecisionContext) -> void:
	var emoji = command.get("emoji", "")
	if emoji.is_empty():
		push_warning("[SetMoodExecutor] Emoji为空")
		return
	
	var npc = context.extra.get("NPCEntity")
	if npc == null:
		push_warning("[SetMoodExecutor] 无法从上下文获取NPCEntity")
		return
	
	if npc.has_method("set_mood"):
		npc.set_mood(emoji)
	
	for cb in on_mood_changed:
		if cb.is_valid():
			cb.call(npc, emoji)
	
	print("[SetMoodExecutor] 心情变更: %s" % emoji)

# ===== 记忆操作执行器 =====

static func _execute_memory_operation(command: Dictionary, context: DecisionContext) -> void:
	var operation = command.get("operation", "")
	var partition_name = command.get("partition", "default")
	var content = command.get("content", "")
	
	if content.is_empty():
		push_warning("[MemoryOperationExecutor] 记忆内容为空")
		return
	
	if context.memory_store == null:
		push_error("[MemoryOperationExecutor] 上下文中未设置MemoryStore")
		return
	
	match operation:
		"Add":
			var partition = context.memory_store.get_or_create_partition(partition_name)
			partition.append(content)
			print("[MemoryOperationExecutor] 添加记忆到分区 '%s': %s" % [partition_name, content])
		"Remove":
			var partition = context.memory_store.get_partition(partition_name)
			if partition:
				var mem = MemoryItem.new(content)
				partition.remove(mem)
				print("[MemoryOperationExecutor] 从分区 '%s' 删除记忆: %s" % [partition_name, content])
		_:
			push_warning("[MemoryOperationExecutor] 未知的操作类型: %s" % operation)

# ===== 耕地执行器 =====

static func _execute_till(command: Dictionary, context: DecisionContext) -> void:
	var x = int(command.get("x", 0))
	var y = int(command.get("y", 0))
	var target_pos = Vector2i(x, y)
	
	var npc = context.extra.get("NPCEntity")
	var soil = FarmManager.get_soil(target_pos)
	
	if FarmManager.till(soil):
		if npc and npc.has_method("record_memory"):
			npc.record_memory("我在 (%s, %s) 开垦了土地。" % [x, y])
	else:
		if npc and npc.has_method("record_memory"):
			npc.record_memory("我在 (%s, %s) 开垦失败。" % [x, y])

# ===== 种植执行器 =====

static func _execute_plant(command: Dictionary, context: DecisionContext) -> void:
	var x = int(command.get("x", 0))
	var y = int(command.get("y", 0))
	var item_id = int(command.get("itemId", 0))
	var target_pos = Vector2i(x, y)
	
	var npc = context.extra.get("NPCEntity")
	var soil = FarmManager.get_soil(target_pos)
	var inventory = npc.inventory if npc else null
	
	if FarmManager.plant_seed(soil, item_id, inventory):
		if npc and npc.has_method("record_memory"):
			npc.record_memory("我在 (%s, %s) 种下了 %s。" % [x, y, item_id])
	else:
		if npc and npc.has_method("record_memory"):
			npc.record_memory("我在 (%s, %s) 种植失败。" % [x, y])

# ===== 收获执行器 =====

static func _execute_harvest(command: Dictionary, context: DecisionContext) -> void:
	var x = int(command.get("x", 0))
	var y = int(command.get("y", 0))
	var target_pos = Vector2i(x, y)
	
	var npc = context.extra.get("NPCEntity")
	var soil = FarmManager.get_soil(target_pos)
	var inventory = npc.inventory if npc else null
	
	if FarmManager.harvest(soil, inventory):
		if npc and npc.has_method("record_memory"):
			npc.record_memory("我在 (%s, %s) 收获了作物。" % [x, y])
	else:
		if npc and npc.has_method("record_memory"):
			npc.record_memory("我在 (%s, %s) 收获失败。" % [x, y])
