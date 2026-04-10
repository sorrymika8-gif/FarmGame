class_name AtomConstants
extends RefCounted

## 原子数值常量 - 数值平衡的唯一入口

# 弹道行为限制
const MAX_BOUNCE: int = 5
const MAX_SPLIT: int = 5
const MAX_PIERCE: int = 10
const MAX_TRACKING: float = 360.0
const MAX_ATTRACT: float = 10.0

# 范围限制
const MAX_AOE: float = 8.0
const MAX_PROJECTILE_WIDTH: float = 3.0

# 数值效果限制
const MAX_DIRECT_HP: float = 500.0
const MAX_DOT_HP: float = 50.0
const MAX_MOVE_SPEED_MOD: float = 100.0
const MAX_ATTACK_MOD: float = 200.0
const MAX_DEFENSE_MOD: float = 100.0

# 状态限制
const MAX_SLOW_PERCENT: float = 90.0
const MAX_SILENCE_DURATION: float = 10.0
const MAX_DAMAGE_MULTIPLIER: float = 3.0
const MAX_STEALTH_DURATION: float = 15.0

# 时间限制
const MAX_DELAY: float = 5.0
const MAX_DURATION: float = 30.0
const MIN_COOLDOWN: float = 0.1
const MAX_COOLDOWN: float = 60.0

# 速度限制
const MIN_PROJECTILE_SPEED: float = 1.0
const MAX_PROJECTILE_SPEED: float = 30.0
const DEFAULT_PROJECTILE_SPEED: float = 10.0

# 生成调度阀门
const SPAWN_QUEUE_CAPACITY: int = 256
const MAX_SPAWN_PER_FRAME: int = 10
const ADAPTIVE_SPAWN_FLOOR: int = 5
const ADAPTIVE_SPAWN_CEILING: int = 15
const FPS_ADAPTIVE_THRESHOLD: float = 40.0
const ENTITY_POOL_CAPACITY: int = 200
