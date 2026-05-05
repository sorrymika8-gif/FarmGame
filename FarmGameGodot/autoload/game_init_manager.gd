## 游戏初始化管理器
## 负责玩家首次进入游戏时的初始化逻辑
extends Node

const INITIAL_MAP = "init_map"
const INITIAL_SPAWN_POSITION = Vector2(0, 0)
const INITIAL_SEED_CONFIG_ID = 1001
const INITIAL_SEED_COUNT = 3

var _is_initialized: bool = false

func initialize() -> void:
	if _is_initialized:
		return
	_is_initialized = true
	print("[GameInitManager] 初始化完成")

## 开始新游戏
func start_new_game() -> void:
	if not _is_initialized:
		push_error("[GameInitManager] 未初始化")
		return
	
	# 创建玩家
	if not PlayerManager.create_player():
		push_error("[GameInitManager] 创建玩家失败")
		return
	
	# 检查是否新玩家
	var is_new_player = true
	if PlayerManager.player and PlayerManager.player.has_method("get_data"):
		var data = PlayerManager.player.get_data()
		if data:
			is_new_player = data.get("is_new_player", true)
	
	var map_to_load = INITIAL_MAP
	var spawn_position = INITIAL_SPAWN_POSITION
	
	print("[GameInitManager] 开始游戏, 新玩家: %s" % str(is_new_player))
	
	# 使用 GameManager 进入场景
	GameManager.enter_scene(map_to_load, spawn_position)
	
	# 打开主界面
	UIManager.open_main_ui()
	print("[GameInitManager] 主界面已打开")
	
	# 初始化天气系统
	WeatherManager.initialize()
	print("[GameInitManager] 天气系统已初始化")
	
	# 新玩家发放初始物品
	if is_new_player:
		var inventory = PlayerManager.get_player_inventory()
		if inventory and inventory.has_method("add_item"):
			inventory.add_item(INITIAL_SEED_CONFIG_ID, INITIAL_SEED_COUNT)
			print("[GameInitManager] 发放初始物品: %d 颗小麦种子" % INITIAL_SEED_COUNT)
		
		if PlayerManager.player and PlayerManager.player.has_method("set_new_player"):
			PlayerManager.player.set_new_player(false)
	
	print("[GameInitManager] 游戏初始化完成")
