extends Control

const DEFAULT_ARENA_SIZE := Vector2(1120.0, 620.0)
const PLAYER_SIZE := Vector2(24.0, 24.0)
const ALLY_SIZE := Vector2(20.0, 20.0)
const ENEMY_SIZE := Vector2(18.0, 18.0)
const PROJECTILE_SIZE := Vector2(8.0, 8.0)
const HEAVY_PROJECTILE_SIZE := Vector2(18.0, 18.0)
const PLAYER_SPEED := 190.0
const ENEMY_SPEED := 72.0
const PROJECTILE_SPEED := 360.0
const HEAVY_PROJECTILE_SPEED := 165.0
const FIRE_INTERVAL := 0.35
const RANGED_NPC_FIRE_INTERVAL := 0.55
const BARRAGE_NPC_FIRE_INTERVAL := 0.24
const MELEE_NPC_ATTACK_INTERVAL := 0.9
const SPAWN_INTERVAL := 0.45
const CONTACT_INTERVAL := 0.45
const CONTACT_DAMAGE := 10
const NPC_CONTACT_DAMAGE := 8
const SURVIVE_TIME := 10.0
const ENEMY_HP := 6
const GRID_STEP := 100.0

@onready var map_panel: Panel = $MapPanel
@onready var world_rect: ColorRect = $MapPanel/WorldRect
@onready var world_grid: ColorRect = $MapPanel/WorldGrid
@onready var player_rect: ColorRect = $MapPanel/Player
@onready var hp_label: Label = $Hud/HpLabel
@onready var timer_label: Label = $Hud/TimerLabel
@onready var ally_label: Label = $Hud/AllyLabel
@onready var result_label: Label = $ResultLabel

var _selected_npcs: Array[String] = ["melee", "ranged", "barrage"]
var _arena_size := DEFAULT_ARENA_SIZE
var _player_pos := Vector2.ZERO
var _player_facing := Vector2.DOWN
var _player_hp := 100
var _elapsed := 0.0
var _spawn_timer := 0.0
var _fire_timer := 0.0
var _is_running := true
var _allies: Array = []
var _enemies: Array = []
var _projectiles: Array = []
var _grid_lines: Array[ColorRect] = []
var _rng := RandomNumberGenerator.new()

func setup(data: Dictionary) -> void:
	var selected: Array = data.get("selected_npcs", _selected_npcs)
	_selected_npcs.clear()
	for npc_id in selected:
		_selected_npcs.append(str(npc_id))

func _ready() -> void:
	_rng.randomize()
	_set_world_input_enabled(false)
	await get_tree().process_frame
	_arena_size = map_panel.size
	if _arena_size.x <= 0.0 or _arena_size.y <= 0.0:
		_arena_size = DEFAULT_ARENA_SIZE
	_player_pos = _arena_size * 0.5
	_create_grid_lines()
	_create_selected_allies()
	_update_world_visuals()
	_update_player_node()
	_update_hud()
	result_label.visible = false

func _exit_tree() -> void:
	_set_world_input_enabled(true)

func _process(delta: float) -> void:
	if not _is_running:
		return
	_elapsed += delta
	_spawn_timer -= delta
	_fire_timer -= delta
	_handle_player_move(delta)
	_update_world_visuals()
	_update_player_node()
	_update_allies(delta)
	_update_enemies(delta)
	_update_projectiles(delta)
	_try_spawn_enemy()
	_try_fire_player_projectile()
	_update_hud()
	if _elapsed >= SURVIVE_TIME:
		_finish_run(true)

func _create_selected_allies() -> void:
	var slot_offsets := [Vector2(-42.0, 34.0), Vector2(42.0, 34.0), Vector2(0.0, 58.0)]
	var slot := 0
	for npc_id in _selected_npcs:
		var node := ColorRect.new()
		node.size = ALLY_SIZE
		node.color = _get_ally_color(npc_id)
		map_panel.add_child(node)
		var offset: Vector2 = slot_offsets[slot % slot_offsets.size()]
		var ally := {
			"id": npc_id,
			"name": _get_ally_name(npc_id),
			"node": node,
			"pos": _player_pos + offset,
			"offset": offset,
			"follow_target": _player_pos + offset,
			"follow_speed": _get_ally_follow_speed(npc_id),
			"reaction_delay": _get_ally_reaction_delay(npc_id),
			"reaction_timer": 0.0,
			"hp": 70,
			"max_hp": 70,
			"attack_cd": 0.0,
			"contact_cd": 0.0,
			"alive": true,
		}
		_allies.append(ally)
		var ally_pos: Vector2 = ally["pos"]
		_update_world_rect(node, ally_pos, ALLY_SIZE)
		slot += 1

func _create_grid_lines() -> void:
	for child in _grid_lines:
		if is_instance_valid(child):
			child.queue_free()
	_grid_lines.clear()
	var vertical_count := int(_arena_size.x / GRID_STEP) + 1
	var horizontal_count := int(_arena_size.y / GRID_STEP) + 1
	for i in range(vertical_count):
		var line := ColorRect.new()
		line.color = Color(0.23, 0.33, 0.23, 0.55)
		map_panel.add_child(line)
		map_panel.move_child(line, world_grid.get_index() + 1)
		_grid_lines.append(line)
	for i in range(horizontal_count):
		var line := ColorRect.new()
		line.color = Color(0.23, 0.33, 0.23, 0.55)
		map_panel.add_child(line)
		map_panel.move_child(line, world_grid.get_index() + 1)
		_grid_lines.append(line)

func _get_ally_color(npc_id: String) -> Color:
	if npc_id == "melee":
		return Color(0.95, 0.55, 0.2, 1.0)
	if npc_id == "ranged":
		return Color(0.35, 0.9, 0.45, 1.0)
	return Color(0.78, 0.48, 1.0, 1.0)

func _get_ally_name(npc_id: String) -> String:
	if npc_id == "melee":
		return "近战"
	if npc_id == "ranged":
		return "远程"
	return "重弹幕"

func _get_ally_follow_speed(npc_id: String) -> float:
	if npc_id == "melee":
		return 285.0
	if npc_id == "ranged":
		return 225.0
	return 175.0

func _get_ally_reaction_delay(npc_id: String) -> float:
	if npc_id == "melee":
		return 0.07
	if npc_id == "ranged":
		return 0.12
	return 0.18

func _handle_player_move(delta: float) -> void:
	var direction := Vector2.ZERO
	if Input.is_key_pressed(KEY_A) or Input.is_key_pressed(KEY_LEFT):
		direction.x -= 1.0
	if Input.is_key_pressed(KEY_D) or Input.is_key_pressed(KEY_RIGHT):
		direction.x += 1.0
	if Input.is_key_pressed(KEY_W) or Input.is_key_pressed(KEY_UP):
		direction.y -= 1.0
	if Input.is_key_pressed(KEY_S) or Input.is_key_pressed(KEY_DOWN):
		direction.y += 1.0
	if direction != Vector2.ZERO:
		_player_facing = direction.normalized()
		_player_pos += _player_facing * PLAYER_SPEED * delta
	_player_pos = _clamp_to_world(_player_pos, PLAYER_SIZE)

func _update_allies(delta: float) -> void:
	for ally_value in _allies:
		var ally: Dictionary = ally_value
		var node: ColorRect = ally["node"]
		if not is_instance_valid(node):
			continue
		if not bool(ally["alive"]):
			var dead_pos: Vector2 = ally["pos"]
			_update_world_rect(node, dead_pos, ALLY_SIZE)
			continue
		var offset: Vector2 = ally["offset"]
		ally["reaction_timer"] = maxf(float(ally["reaction_timer"]) - delta, 0.0)
		if float(ally["reaction_timer"]) <= 0.0:
			ally["follow_target"] = _player_pos + offset
			ally["reaction_timer"] = float(ally["reaction_delay"])
		var follow_pos: Vector2 = ally["follow_target"]
		var pos: Vector2 = ally["pos"]
		pos = pos.move_toward(follow_pos, float(ally["follow_speed"]) * delta)
		pos = _clamp_to_world(pos, ALLY_SIZE)
		ally["pos"] = pos
		ally["attack_cd"] = maxf(float(ally["attack_cd"]) - delta, 0.0)
		ally["contact_cd"] = maxf(float(ally["contact_cd"]) - delta, 0.0)
		_update_world_rect(node, pos, ALLY_SIZE)
		_update_ally_attack(ally)

func _update_ally_attack(ally: Dictionary) -> void:
	var npc_id := str(ally["id"])
	if npc_id == "melee":
		_try_melee_attack(ally)
	elif npc_id == "ranged":
		_try_ally_projectile(ally, RANGED_NPC_FIRE_INTERVAL, PROJECTILE_SPEED, PROJECTILE_SIZE, Color(0.45, 1.0, 0.55, 1.0), 1)
	elif npc_id == "barrage":
		_try_ally_projectile(ally, BARRAGE_NPC_FIRE_INTERVAL, HEAVY_PROJECTILE_SPEED, HEAVY_PROJECTILE_SIZE, Color(0.85, 0.55, 1.0, 1.0), 1)

func _try_melee_attack(ally: Dictionary) -> void:
	if float(ally["attack_cd"]) > 0.0:
		return
	var ally_pos: Vector2 = ally["pos"]
	var target := _find_nearest_enemy(ally_pos)
	if target.is_empty():
		return
	var target_pos: Vector2 = target["pos"]
	var forward := (target_pos - ally_pos).normalized()
	if forward == Vector2.ZERO or ally_pos.distance_to(target_pos) > 78.0:
		return
	ally["attack_cd"] = MELEE_NPC_ATTACK_INTERVAL
	_show_melee_swing(ally_pos, forward)
	for i in range(_enemies.size() - 1, -1, -1):
		var enemy: Dictionary = _enemies[i]
		var enemy_pos: Vector2 = enemy["pos"]
		var to_enemy := enemy_pos - ally_pos
		if to_enemy.length() > 86.0:
			continue
		if absf(forward.angle_to(to_enemy.normalized())) > deg_to_rad(52.0):
			continue
		var knockback := to_enemy.normalized() * 42.0
		enemy["pos"] = _clamp_to_world(enemy_pos + knockback, ENEMY_SIZE)
		_damage_enemy_at(i, 2)

func _show_melee_swing(center_pos: Vector2, forward: Vector2) -> void:
	var swing := Polygon2D.new()
	swing.color = Color(1.0, 0.76, 0.25, 0.55)
	var radius := 74.0
	var left := forward.rotated(deg_to_rad(-52.0)) * radius
	var right := forward.rotated(deg_to_rad(52.0)) * radius
	swing.polygon = PackedVector2Array([Vector2.ZERO, left, forward * radius, right])
	swing.position = _world_to_screen(center_pos)
	map_panel.add_child(swing)
	var timer := get_tree().create_timer(0.12)
	timer.timeout.connect(func() -> void:
		if is_instance_valid(swing):
			swing.queue_free()
	)

func _try_ally_projectile(ally: Dictionary, interval: float, speed: float, size: Vector2, color: Color, damage: int) -> void:
	if float(ally["attack_cd"]) > 0.0:
		return
	var pos: Vector2 = ally["pos"]
	var target := _find_nearest_enemy(pos)
	if target.is_empty():
		return
	ally["attack_cd"] = interval
	var target_pos: Vector2 = target["pos"]
	var direction := (target_pos - pos).normalized()
	if direction == Vector2.ZERO:
		direction = Vector2.RIGHT
	_spawn_projectile(pos, direction * speed, size, color, damage)

func _try_spawn_enemy() -> void:
	if _spawn_timer > 0.0:
		return
	_spawn_timer = SPAWN_INTERVAL
	var side := _rng.randi_range(0, 3)
	var screen_pos := Vector2.ZERO
	if side == 0:
		screen_pos = Vector2(_rng.randf_range(0.0, _arena_size.x), -ENEMY_SIZE.y)
	elif side == 1:
		screen_pos = Vector2(_arena_size.x + ENEMY_SIZE.x, _rng.randf_range(0.0, _arena_size.y))
	elif side == 2:
		screen_pos = Vector2(_rng.randf_range(0.0, _arena_size.x), _arena_size.y + ENEMY_SIZE.y)
	else:
		screen_pos = Vector2(-ENEMY_SIZE.x, _rng.randf_range(0.0, _arena_size.y))
	var pos := _clamp_to_world(_screen_to_world(screen_pos), ENEMY_SIZE)
	var node := ColorRect.new()
	node.color = Color(0.9, 0.22, 0.18, 1.0)
	node.size = ENEMY_SIZE
	map_panel.add_child(node)
	_enemies.append({"node": node, "pos": pos, "hp": ENEMY_HP, "contact_cd": 0.0})
	_update_world_rect(node, pos, ENEMY_SIZE)

func _update_enemies(delta: float) -> void:
	for i in range(_enemies.size() - 1, -1, -1):
		var enemy: Dictionary = _enemies[i]
		var node: ColorRect = enemy["node"]
		if not is_instance_valid(node):
			_enemies.remove_at(i)
			continue
		var pos: Vector2 = enemy["pos"]
		var target := _find_nearest_target(pos)
		var target_pos: Vector2 = target.get("pos", _player_pos)
		var direction := (target_pos - pos).normalized()
		pos += direction * ENEMY_SPEED * delta
		pos = _clamp_to_world(pos, ENEMY_SIZE)
		enemy["pos"] = pos
		enemy["contact_cd"] = maxf(float(enemy["contact_cd"]) - delta, 0.0)
		if pos.distance_to(target_pos) <= 22.0 and float(enemy["contact_cd"]) <= 0.0:
			enemy["contact_cd"] = CONTACT_INTERVAL
			_damage_target(target)
		_update_world_rect(node, pos, ENEMY_SIZE)

func _find_nearest_target(from_pos: Vector2) -> Dictionary:
	var nearest := {"type": "player", "pos": _player_pos}
	var nearest_distance := from_pos.distance_to(_player_pos)
	for ally_value in _allies:
		var ally: Dictionary = ally_value
		if not bool(ally["alive"]):
			continue
		var ally_pos: Vector2 = ally["pos"]
		var distance := from_pos.distance_to(ally_pos)
		if distance < nearest_distance:
			nearest = {"type": "ally", "ally": ally, "pos": ally_pos}
			nearest_distance = distance
	return nearest

func _damage_target(target: Dictionary) -> void:
	if str(target.get("type", "player")) == "ally":
		var ally: Dictionary = target["ally"]
		ally["hp"] = int(ally["hp"]) - NPC_CONTACT_DAMAGE
		if int(ally["hp"]) <= 0:
			ally["hp"] = 0
			ally["alive"] = false
			var node: ColorRect = ally["node"]
			if is_instance_valid(node):
				node.color = Color(0.18, 0.18, 0.18, 1.0)
		return
	_player_hp -= CONTACT_DAMAGE
	if _player_hp <= 0:
		_player_hp = 0
		_finish_run(false)

func _try_fire_player_projectile() -> void:
	if _fire_timer > 0.0 or _enemies.is_empty():
		return
	var target := _find_nearest_enemy(_player_pos)
	if target.is_empty():
		return
	_fire_timer = FIRE_INTERVAL
	var target_pos: Vector2 = target["pos"]
	var direction := (target_pos - _player_pos).normalized()
	if direction == Vector2.ZERO:
		direction = Vector2.RIGHT
	_spawn_projectile(_player_pos, direction * PROJECTILE_SPEED, PROJECTILE_SIZE, Color(1.0, 0.85, 0.24, 1.0), 1)

func _spawn_projectile(pos: Vector2, velocity: Vector2, size: Vector2, color: Color, damage: int) -> void:
	var node := ColorRect.new()
	node.color = color
	node.size = size
	map_panel.add_child(node)
	_projectiles.append({"node": node, "pos": pos, "velocity": velocity, "size": size, "damage": damage})
	_update_world_rect(node, pos, size)

func _update_projectiles(delta: float) -> void:
	for i in range(_projectiles.size() - 1, -1, -1):
		var projectile: Dictionary = _projectiles[i]
		var node: ColorRect = projectile["node"]
		if not is_instance_valid(node):
			_projectiles.remove_at(i)
			continue
		var projectile_pos: Vector2 = projectile["pos"]
		var velocity: Vector2 = projectile["velocity"]
		var pos: Vector2 = projectile_pos + velocity * delta
		projectile["pos"] = pos
		var screen_pos := _world_to_screen(pos)
		if screen_pos.x < -24.0 or screen_pos.x > _arena_size.x + 24.0 or screen_pos.y < -24.0 or screen_pos.y > _arena_size.y + 24.0:
			node.queue_free()
			_projectiles.remove_at(i)
			continue
		var projectile_size: Vector2 = projectile["size"]
		if _projectile_hit_enemy(pos, projectile_size.x * 0.5, int(projectile["damage"])):
			node.queue_free()
			_projectiles.remove_at(i)
			continue
		_update_world_rect(node, pos, projectile_size)

func _projectile_hit_enemy(projectile_pos: Vector2, radius: float, damage: int) -> bool:
	for i in range(_enemies.size() - 1, -1, -1):
		var enemy: Dictionary = _enemies[i]
		var enemy_pos: Vector2 = enemy["pos"]
		if projectile_pos.distance_to(enemy_pos) > 13.0 + radius:
			continue
		_damage_enemy_at(i, damage)
		return true
	return false

func _damage_enemy_at(index: int, damage: int) -> void:
	if index < 0 or index >= _enemies.size():
		return
	var enemy: Dictionary = _enemies[index]
	enemy["hp"] = int(enemy["hp"]) - damage
	if int(enemy["hp"]) <= 0:
		var node: ColorRect = enemy["node"]
		if is_instance_valid(node):
			node.queue_free()
		_enemies.remove_at(index)

func _find_nearest_enemy(from_pos: Vector2) -> Dictionary:
	var nearest: Dictionary = {}
	var nearest_distance := INF
	for enemy_value in _enemies:
		var enemy: Dictionary = enemy_value
		var pos: Vector2 = enemy["pos"]
		var distance := from_pos.distance_to(pos)
		if distance < nearest_distance:
			nearest = enemy
			nearest_distance = distance
	return nearest

func _finish_run(victory: bool) -> void:
	if not _is_running:
		return
	_is_running = false
	result_label.visible = true
	result_label.text = "探索胜利" if victory else "探索失败"
	await get_tree().create_timer(1.0).timeout
	UIManager.close_panel("exploration_play_panel")

func _update_player_node() -> void:
	_update_world_rect(player_rect, _player_pos, PLAYER_SIZE)

func _update_world_visuals() -> void:
	var top_left := _world_to_screen(Vector2.ZERO)
	world_rect.position = top_left
	world_rect.size = _arena_size
	world_grid.position = top_left
	world_grid.size = _arena_size
	var vertical_count := int(_arena_size.x / GRID_STEP) + 1
	for i in range(_grid_lines.size()):
		var line := _grid_lines[i]
		if not is_instance_valid(line):
			continue
		if i < vertical_count:
			line.position = _world_to_screen(Vector2(i * GRID_STEP, 0.0))
			line.size = Vector2(1.0, _arena_size.y)
		else:
			var row := i - vertical_count
			line.position = _world_to_screen(Vector2(0.0, row * GRID_STEP))
			line.size = Vector2(_arena_size.x, 1.0)

func _update_screen_rect(node: Control, center_pos: Vector2, rect_size: Vector2) -> void:
	node.position = center_pos - rect_size * 0.5
	node.size = rect_size

func _update_world_rect(node: Control, world_pos: Vector2, rect_size: Vector2) -> void:
	_update_screen_rect(node, _world_to_screen(world_pos), rect_size)

func _world_to_screen(world_pos: Vector2) -> Vector2:
	return world_pos

func _screen_to_world(screen_pos: Vector2) -> Vector2:
	return screen_pos

func _clamp_to_world(pos: Vector2, rect_size: Vector2) -> Vector2:
	return Vector2(
		clampf(pos.x, rect_size.x * 0.5, _arena_size.x - rect_size.x * 0.5),
		clampf(pos.y, rect_size.y * 0.5, _arena_size.y - rect_size.y * 0.5)
	)

func _update_hud() -> void:
	hp_label.text = "玩家生命: %d" % _player_hp
	timer_label.text = "坚持: %.1f / %.0f" % [minf(_elapsed, SURVIVE_TIME), SURVIVE_TIME]
	var parts: Array[String] = []
	for ally_value in _allies:
		var ally: Dictionary = ally_value
		parts.append("%s:%d" % [str(ally["name"]), int(ally["hp"])])
	var ally_text := ""
	for part in parts:
		if ally_text != "":
			ally_text += "  "
		ally_text += part
	ally_label.text = "NPC " + ("无" if ally_text == "" else ally_text)

func _set_world_input_enabled(enabled: bool) -> void:
	var player = PlayerManager.player
	if player == null:
		return
	var input_handler = player.get_node_or_null("PlayerInputHandler")
	if input_handler:
		input_handler.input_enabled = enabled
