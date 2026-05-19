## 存档/读档面板
## 对应 Unity 的 SaveLoadPanel
extends Control

var _is_save_mode: bool = true
var _on_load_selected: Callable = Callable()

@onready var title_label: Label = $Panel/MarginContainer/VBoxContainer/TopBar/TitleLabel
@onready var close_button: Button = $Panel/MarginContainer/VBoxContainer/TopBar/CloseButton
@onready var slot_container: VBoxContainer = $Panel/MarginContainer/VBoxContainer/ScrollContainer/SlotContainer

func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS
	if close_button:
		close_button.pressed.connect(_on_close_pressed)

func setup(data: Dictionary) -> void:
	_is_save_mode = data.get("is_save_mode", true)
	_on_load_selected = data.get("on_load_selected", Callable())
	
	if title_label:
		title_label.text = "保存游戏" if _is_save_mode else "加载游戏"
	
	_refresh_slots()

func _refresh_slots() -> void:
	if slot_container == null:
		return
	
	for child in slot_container.get_children():
		child.queue_free()
	
	var slots = SaveSystem.get_save_slots_info()
	for slot_info in slots:
		var hbox = HBoxContainer.new()
		
		var label = Label.new()
		var slot_idx = slot_info.get("slot_index", 0)
		if slot_info.get("has_save", false):
			label.text = "槽位 %d - %s" % [slot_idx + 1, slot_info.get("save_time", "")]
		else:
			label.text = "槽位 %d - 空" % (slot_idx + 1)
		label.size_flags_horizontal = Control.SIZE_EXPAND_FILL
		hbox.add_child(label)
		
		var action_button = Button.new()
		action_button.text = "保存" if _is_save_mode else "加载"
		if not _is_save_mode and not slot_info.get("has_save", false):
			action_button.disabled = true
		action_button.pressed.connect(_on_slot_pressed.bind(slot_idx))
		hbox.add_child(action_button)
		
		slot_container.add_child(hbox)

func _on_slot_pressed(slot_index: int) -> void:
	if _is_save_mode:
		SaveSystem.save_game(slot_index)
	elif _on_load_selected.is_valid():
		_on_load_selected.call(slot_index)
		UIManager.close_panel("save_load_panel")
		return
	else:
		SaveSystem.load_game(slot_index)
	
	_refresh_slots()

func _on_close_pressed() -> void:
	UIManager.close_panel("save_load_panel")
