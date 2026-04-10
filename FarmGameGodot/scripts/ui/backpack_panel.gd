## 背包面板
## 对应 Unity 的 BackpackPanel
extends Control

var _inventory = null

@onready var item_container: VBoxContainer = $Panel/MarginContainer/VBoxContainer/ScrollContainer/ItemContainer
@onready var close_button: Button = $Panel/MarginContainer/VBoxContainer/TopBar/CloseButton
@onready var title_label: Label = $Panel/MarginContainer/VBoxContainer/TopBar/TitleLabel

func _ready() -> void:
	if close_button:
		close_button.pressed.connect(_on_close_pressed)

func setup(data: Dictionary) -> void:
	_inventory = data.get("inventory", null)
	_refresh_items()

func _refresh_items() -> void:
	if item_container == null or _inventory == null:
		return
	
	# 清空现有项
	for child in item_container.get_children():
		child.queue_free()
	
	# 添加物品
	var items = _inventory.get_all_items()
	for item in items:
		var hbox = HBoxContainer.new()
		
		var name_label = Label.new()
		var config_info = item.config_info if item.has_method("get") else {}
		name_label.text = config_info.get("name", "物品#%d" % item.config_id)
		hbox.add_child(name_label)
		
		var count_label = Label.new()
		count_label.text = " x%d" % item.count
		hbox.add_child(count_label)
		
		item_container.add_child(hbox)

func _on_close_pressed() -> void:
	UIManager.close_panel("backpack_panel")
