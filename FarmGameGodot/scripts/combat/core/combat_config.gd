class_name CombatConfig
extends RefCounted

## 战斗配置常量

# 场景配置
const COMBAT_SCENE_NAME := "Combat"
const COMBAT_SCENE_PATH := "res://scenes/combat.tscn"

# 预制体路径
const SKILL_ENTITY_SCENE := "res://resources/prefabs/combat/skill_entity.tscn"
const CHAR_ENTITY_SCENE := "res://resources/prefabs/combat/char_entity.tscn"
const PLAYER_COMBAT_SCENE := "res://resources/prefabs/combat/player_combat.tscn"

# 战斗区域
const BATTLE_AREA_WIDTH: float = 20.0
const BATTLE_AREA_HEIGHT: float = 15.0
const PLAYER_SPAWN_X: float = -7.0 * 64.0  # 像素坐标
const ENEMY_SPAWN_X: float = 7.0 * 64.0

# 战斗参数
const BATTLE_START_COUNTDOWN: float = 3.0
const BATTLE_END_DELAY: float = 2.0
const PLAYER_INVINCIBILITY_TIME: float = 0.5
const MIN_SKILL_INTERVAL: float = 0.1

# 敌人配置
const DEFAULT_ENEMY_HP: float = 100.0
const DEFAULT_ENEMY_ATTACK: float = 10.0
const DEFAULT_ENEMY_DEFENSE: float = 5.0
const DEFAULT_ENEMY_MOVE_SPEED: float = 3.0

# 玩家配置
const DEFAULT_PLAYER_HP: float = 200.0
const DEFAULT_PLAYER_ATTACK: float = 15.0
const DEFAULT_PLAYER_DEFENSE: float = 10.0
const DEFAULT_PLAYER_MOVE_SPEED: float = 5.0

# 碰撞层（在 Godot 项目设置中配置对应层）
const PLAYER_COLLISION_LAYER: int = 1
const ENEMY_COLLISION_LAYER: int = 2
const PLAYER_PROJECTILE_LAYER: int = 3
const ENEMY_PROJECTILE_LAYER: int = 4

# AI配置
const ENEMY_DETECTION_RANGE: float = 12.0 * 64.0
const ENEMY_ATTACK_RANGE: float = 8.0 * 64.0
const ENEMY_ATTACK_COOLDOWN: float = 2.0
const ENEMY_FLEE_HP_THRESHOLD: float = 0.2
const ENEMY_AI_UPDATE_INTERVAL: float = 0.1
