## 启动管理器
## 对应 Unity 的 BootManager，负责按顺序初始化各管理器
## 这个脚本挂载在 Init 场景的根节点上
extends Node

var _is_initialized: bool = false

func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS
	_initialize_async()

func _unhandled_input(event: InputEvent) -> void:
	if not _is_initialized:
		return
	if event.is_action_pressed("ui_cancel") and GameInitManager.is_game_running():
		if UIManager.close_top_popup_panel():
			get_viewport().set_input_as_handled()
			return
		if UIManager.is_panel_open("pause_menu_panel"):
			UIManager.close_panel("pause_menu_panel")
			get_tree().paused = false
		else:
			UIManager.open_panel("res://ui/pause_menu_panel.tscn")
			get_tree().paused = true
		get_viewport().set_input_as_handled()

func _initialize_async() -> void:
	if _is_initialized:
		return
	
	print("[BootManager] 开始初始化...")
	
	# 1. 初始化资源管理器
	ResourceManager.initialize()
	print("[BootManager] ResourceManager 初始化完成")
	
	# 2. 初始化配置管理器并加载配置
	ConfigManager.initialize()
	await ConfigManager.load_all_configs("res://configs")
	print("[BootManager] ConfigManager 初始化完成")
	
	# 3. 初始化 LLM 服务
	LLMService.initialize()
	print("[BootManager] LLMService 初始化完成")
	LLMDescriptionService.new()
	print("[BootManager] LLMDescriptionService 初始化完成")
	
	# 4. 初始化 UI 管理器
	UIManager.initialize()
	print("[BootManager] UIManager 初始化完成")
	
	# 5. 初始化存档系统
	SaveSystem.initialize()
	print("[BootManager] SaveSystem 初始化完成")
	
	# 6. 初始化地图管理器
	MapManager.initialize()
	print("[BootManager] MapManager 初始化完成")
	
	# 7. 初始化玩家管理器
	PlayerManager.initialize()
	print("[BootManager] PlayerManager 初始化完成")
	
	# 8. 初始化移动管理器
	MovementManager.initialize()
	print("[BootManager] MovementManager 初始化完成")
	
	# 9. 初始化游戏初始化管理器
	GameInitManager.initialize()
	print("[BootManager] GameInitManager 初始化完成")
	
	# 10. 初始化 NPC 管理器
	NPCManager.initialize()
	print("[BootManager] NPCManager 初始化完成")
	
	# 11. 初始化物品管理器
	ItemManager.initialize()
	print("[BootManager] ItemManager 初始化完成")
	
	# 12. 初始化商店管理器
	ShopManager.initialize()
	print("[BootManager] ShopManager 初始化完成")
	
	# 13. 初始化农场管理器
	FarmManager.initialize()
	print("[BootManager] FarmManager 初始化完成")
	
	# 14. 初始化天气管理器
	WeatherManager.initialize()
	print("[BootManager] WeatherManager 初始化完成")
	
	# 15. 初始化战斗管理器
	CombatManager.initialize()
	print("[BootManager] CombatManager 初始化完成")

	# 16. 初始化装备管理器
	EquipmentManager.initialize()
	print("[BootManager] EquipmentManager 初始化完成")
	
	_is_initialized = true
	print("[BootManager] 所有管理器初始化完成!")
	
	UIManager.open_panel("res://ui/main_menu_panel.tscn")
