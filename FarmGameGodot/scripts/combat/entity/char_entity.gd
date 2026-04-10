class_name CharEntity
extends CharacterBody2D

## 角色实体 - 战斗中的角色单位（玩家和敌人通用）

signal hp_changed(amount: float)
signal died()
signal effect_ticked(effect: StatusEffect)

@export var entity_type: int = AtomEnums.EntityType.ENEMY
@export var initial_max_hp: float = 100.0
@export var initial_move_speed: float = 5.0
@export var initial_attack: float = 10.0
@export var initial_defense: float = 5.0

var stats: EntityStats
var _is_initialized: bool = false

func _ready() -> void:
	if not _is_initialized:
		initialize()

func _physics_process(delta: float) -> void:
	if stats != null and stats.is_alive():
		stats.tick(delta)

## 初始化角色实体
func initialize(p_entity_type: int = -1, p_stats: EntityStats = null) -> void:
	if p_entity_type >= 0:
		entity_type = p_entity_type
	if p_stats != null:
		stats = p_stats
	else:
		stats = EntityStats.new()
		stats.max_hp = initial_max_hp
		stats.current_hp = initial_max_hp
		stats.base_move_speed = initial_move_speed
		stats.base_attack = initial_attack
		stats.base_defense = initial_defense
	# 绑定回调
	stats.on_hp_changed.append(_on_hp_changed)
	stats.on_death.append(_on_death)
	stats.on_effect_tick.append(_on_effect_tick)
	_is_initialized = true

## 受到伤害
func take_damage(base_damage: float, ignore_defense: bool = false) -> float:
	if not is_alive() or base_damage <= 0:
		return 0.0
	var damage := base_damage
	if not ignore_defense and stats.get_defense() > 0:
		damage = base_damage * (100.0 / (100.0 + stats.get_defense()))
	damage *= stats.get_damage_multiplier()
	damage = maxf(0.0, damage)
	stats.apply_hp_change(-damage)
	return damage

## 治疗
func heal(amount: float) -> float:
	if not is_alive() or amount <= 0:
		return 0.0
	var previous_hp := stats.current_hp
	stats.apply_hp_change(amount)
	return stats.current_hp - previous_hp

## 应用状态效果
func apply_status(effect: StatusEffect, stackable: bool = false) -> void:
	if not is_alive() or effect == null:
		return
	stats.apply_effect(effect, stackable)

## 移除状态效果
func remove_status(effect_type: int) -> void:
	if stats != null:
		stats.remove_effect(effect_type)

## 设置移动方向
func set_move_direction(direction: Vector2) -> void:
	if not is_alive():
		return
	var speed := stats.get_move_speed() * 64.0  # 转换为像素速度
	if stats.is_silenced():
		velocity = direction.normalized() * speed * 0.5
	else:
		velocity = direction.normalized() * speed
	move_and_slide()

## 停止移动
func stop_moving() -> void:
	velocity = Vector2.ZERO

## 是否存活
func is_alive() -> bool:
	return stats != null and stats.is_alive()

## 应用力
func apply_force(force: Vector2) -> void:
	if is_alive():
		global_position += force

## 重置状态（对象池重用）
func reset_entity() -> void:
	if stats != null:
		stats.reset()
	stop_moving()
	visible = true

## 回调
func _on_hp_changed(change: float) -> void:
	hp_changed.emit(change)

func _on_death() -> void:
	stop_moving()
	visible = false
	died.emit()

func _on_effect_tick(effect: StatusEffect) -> void:
	effect_ticked.emit(effect)
