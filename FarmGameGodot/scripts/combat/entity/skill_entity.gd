class_name SkillEntity
extends Area2D

## 技能实体 - 投射物，每帧按数据驱动行为

var data: SkillAtomData
var owner_entity: CharEntity
var pierce_remaining: int = 0
var bounce_remaining: int = 0
var is_returning: bool = false
var start_position: Vector2 = Vector2.ZERO
var _traveled_distance: float = 0.0
var _lifetime: float = 0.0
var _hit_targets: Array[CharEntity] = []

const MAX_LIFETIME: float = 10.0
const MAX_DISTANCE: float = 50.0 * 64.0  # 像素单位

## 当前移动方向
var move_direction: Vector2:
	get: return Vector2.RIGHT.rotated(rotation)

func _ready() -> void:
	area_entered.connect(_on_area_entered)
	body_entered.connect(_on_body_entered)

func _process(delta: float) -> void:
	if data == null:
		return
	_lifetime += delta
	if _lifetime > MAX_LIFETIME:
		return_to_pool()
		return
	# 1. 追踪行为
	if data.tracking > 0 and not is_returning:
		TrackingHandler.execute(self)
	# 2. 弹道移动
	var movement := move_direction * data.projectile_speed * 64.0 * delta
	global_position += movement
	_traveled_distance += movement.length()
	# 3. 吸引/排斥
	if not is_zero_approx(data.attract):
		AttractHandler.execute(self)
	# 4. 返回检测
	if data.returning:
		ReturnHandler.execute(self)
	# 5. 距离检测
	if _traveled_distance > MAX_DISTANCE:
		return_to_pool()

## 碰撞处理 - 与CharEntity（body）碰撞
func _on_body_entered(body: Node2D) -> void:
	if body is CharEntity:
		_handle_hit(body as CharEntity)

## 碰撞处理 - 与Area2D碰撞
func _on_area_entered(_area: Area2D) -> void:
	pass

func _handle_hit(target_entity: CharEntity) -> void:
	if target_entity == null:
		return
	# 不攻击友方
	if owner_entity != null and target_entity.entity_type == owner_entity.entity_type:
		return
	# 检查是否已命中过
	if target_entity in _hit_targets:
		return
	_hit_targets.append(target_entity)
	# 应用效果
	EffectApplier.apply_effects(data, target_entity, owner_entity)
	# AOE 效果
	if data.aoe_radius > 0:
		AOEHandler.execute(self, global_position)
	# 弹射
	if bounce_remaining > 0:
		if BounceHandler.execute(self, target_entity):
			bounce_remaining -= 1
			return
	# 分裂
	if data.split > 0:
		SplitHandler.execute(self)
	# 穿透检查
	pierce_remaining -= 1
	if pierce_remaining < 0:
		return_to_pool()

## 初始化技能实体
func init_skill(p_data: SkillAtomData, p_owner: CharEntity = null) -> void:
	data = p_data
	owner_entity = p_owner
	pierce_remaining = p_data.pierce
	bounce_remaining = p_data.bounce
	is_returning = false
	start_position = global_position
	_traveled_distance = 0.0
	_lifetime = 0.0
	_hit_targets.clear()
	if data.projectile_speed <= 0:
		data.projectile_speed = AtomConstants.DEFAULT_PROJECTILE_SPEED
	visible = true

## 设置返回状态
func set_returning(p_returning: bool) -> void:
	is_returning = p_returning
	if p_returning:
		_hit_targets.clear()

## 设置移动方向
func set_direction(direction: Vector2) -> void:
	if direction.length_squared() > 0.001:
		rotation = direction.angle()

## 旋转方向
func rotate_direction(angle_delta: float) -> void:
	rotation += deg_to_rad(angle_delta)

## 获取所有者位置
func get_owner_position() -> Vector2:
	if owner_entity != null and owner_entity.is_alive():
		return owner_entity.global_position
	return start_position

## 消耗一次弹射计数
func consume_bounce() -> void:
	bounce_remaining -= 1

## 返回对象池
func return_to_pool() -> void:
	visible = false
	set_process(false)
	# 通过 CombatEntityPool 回收
	if CombatEntityPool.instance != null:
		CombatEntityPool.instance.return_skill_entity(self)

## 重置状态
func reset_state() -> void:
	data = null
	owner_entity = null
	pierce_remaining = 0
	bounce_remaining = 0
	is_returning = false
	_traveled_distance = 0.0
	_lifetime = 0.0
	_hit_targets.clear()
	rotation = 0.0
	set_process(true)
