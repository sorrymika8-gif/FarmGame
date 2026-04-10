class_name PlayerCombatController
extends Node2D

## 玩家战斗控制器 - 处理战斗中的玩家输入和技能释放

@export var max_skill_slots: int = 4
@export var aim_assist_range: float = 5.0 * 64.0
@export var aim_assist_angle: float = 30.0
@export var aim_line_length: float = 3.0 * 64.0

var char_entity: CharEntity
var _skill_slots: Array[SkillSlot] = []
var _current_skill_index: int = 0
var _aim_direction: Vector2 = Vector2.RIGHT
var _input_direction: Vector2 = Vector2.ZERO
var _last_skill_time: float = 0.0

func _ready() -> void:
	for i in range(max_skill_slots):
		_skill_slots.append(SkillSlot.new())
	_equip_default_skills()

func bind(entity: CharEntity) -> void:
	char_entity = entity

func _process(delta: float) -> void:
	if char_entity == null or not char_entity.is_alive():
		return
	_handle_move_input()
	_handle_aim_input()
	_handle_skill_input()
	_handle_skill_switch()
	_update_cooldowns(delta)

func _physics_process(_delta: float) -> void:
	_apply_movement()

func _handle_move_input() -> void:
	_input_direction = Input.get_vector("move_left", "move_right", "move_up", "move_down")

func _handle_aim_input() -> void:
	var mouse_pos := get_global_mouse_position()
	var to_mouse := mouse_pos - global_position
	if to_mouse.length_squared() > 0.01:
		_aim_direction = to_mouse.normalized()
	# 瞄准辅助
	if aim_assist_range > 0:
		_apply_aim_assist()

func _handle_skill_input() -> void:
	if Input.is_mouse_button_pressed(MOUSE_BUTTON_LEFT):
		_try_cast_skill(_current_skill_index)
	if Input.is_mouse_button_pressed(MOUSE_BUTTON_RIGHT):
		_try_cast_skill(1)
	if Input.is_action_just_pressed("skill_3"):
		_try_cast_skill(2)
	if Input.is_action_just_pressed("skill_4"):
		_try_cast_skill(3)

func _handle_skill_switch() -> void:
	for i in range(mini(4, max_skill_slots)):
		if Input.is_action_just_pressed("slot_%d" % (i + 1)):
			_current_skill_index = i

func _apply_movement() -> void:
	if _input_direction.length_squared() > 0.01:
		char_entity.set_move_direction(_input_direction)
	else:
		char_entity.stop_moving()

func _apply_aim_assist() -> void:
	var tree := get_tree()
	if tree == null:
		return
	var best_target: CharEntity = null
	var best_angle := aim_assist_angle
	for node in tree.get_nodes_in_group("combat_entities"):
		if node is CharEntity:
			var target_entity := node as CharEntity
			if target_entity.entity_type != AtomEnums.EntityType.ENEMY:
				continue
			if not target_entity.is_alive():
				continue
			var to_target := (target_entity.global_position - global_position).normalized()
			var angle := rad_to_deg(_aim_direction.angle_to(to_target))
			if absf(angle) < best_angle:
				best_angle = absf(angle)
				best_target = target_entity
	if best_target != null:
		var to_target := (best_target.global_position - global_position).normalized()
		_aim_direction = _aim_direction.lerp(to_target, 0.3).normalized()

func _try_cast_skill(slot_index: int) -> void:
	if slot_index < 0 or slot_index >= _skill_slots.size():
		return
	var slot := _skill_slots[slot_index]
	if slot.is_empty:
		return
	if slot.remaining_cooldown > 0:
		return
	var current_time := Time.get_ticks_msec() / 1000.0
	if current_time - _last_skill_time < CombatConfig.MIN_SKILL_INTERVAL:
		return
	if char_entity.stats.is_silenced():
		return
	_cast_skill(slot)

func _cast_skill(slot: SkillSlot) -> void:
	if slot.skill == null:
		return
	var skill_data := slot.skill.clone()
	var spawn_pos := global_position + _aim_direction * 32.0
	var angle := _aim_direction.angle()
	var request := SpawnRequest.new()
	request.data = skill_data
	request.position = spawn_pos
	request.rotation_angle = angle
	request.owner = char_entity
	SpawnQueue.instance.enqueue(request)
	slot.start_cooldown()
	_last_skill_time = Time.get_ticks_msec() / 1000.0

func _update_cooldowns(delta: float) -> void:
	for slot in _skill_slots:
		slot.update_cooldown(delta)

## 装备技能到指定槽位
func equip_skill(slot_index: int, skill: SkillAtomData) -> void:
	if slot_index < 0 or slot_index >= _skill_slots.size():
		return
	_skill_slots[slot_index].set_skill(skill)

## 卸载技能
func unequip_skill(slot_index: int) -> void:
	if slot_index < 0 or slot_index >= _skill_slots.size():
		return
	_skill_slots[slot_index].clear()

## 获取技能
func get_skill(slot_index: int) -> SkillAtomData:
	if slot_index < 0 or slot_index >= _skill_slots.size():
		return null
	return _skill_slots[slot_index].skill

func _equip_default_skills() -> void:
	var s1 := SkillAtomData.new()
	s1.display_name = "基础射击"
	s1.direct_hp = -15.0
	s1.projectile_speed = 12.0
	s1.cooldown = 0.3
	equip_skill(0, s1)

	var s2 := SkillAtomData.new()
	s2.display_name = "追踪弹"
	s2.direct_hp = -10.0
	s2.projectile_speed = 8.0
	s2.tracking = 60.0
	s2.cooldown = 1.0
	equip_skill(1, s2)

	var s3 := SkillAtomData.new()
	s3.display_name = "散射"
	s3.direct_hp = -8.0
	s3.projectile_speed = 10.0
	s3.split = 3
	s3.cooldown = 1.5
	equip_skill(2, s3)

	var s4 := SkillAtomData.new()
	s4.display_name = "爆炸弹"
	s4.direct_hp = -20.0
	s4.projectile_speed = 6.0
	s4.aoe_radius = 3.0
	s4.cooldown = 3.0
	s4.shape = AtomEnums.ShapeType.CIRCLE
	equip_skill(3, s4)
