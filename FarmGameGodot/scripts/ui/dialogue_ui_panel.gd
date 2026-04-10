## 对话面板
## 对应 Unity 的 DialogueUIPanel
extends Control

var _npc_entity = null
var _chat_history: Array = []

@onready var npc_name_label: Label = $Panel/MarginContainer/VBoxContainer/TopBar/NPCNameLabel
@onready var chat_container: VBoxContainer = $Panel/MarginContainer/VBoxContainer/ScrollContainer/ChatContainer
@onready var input_field: LineEdit = $Panel/MarginContainer/VBoxContainer/InputBar/InputField
@onready var send_button: Button = $Panel/MarginContainer/VBoxContainer/InputBar/SendButton
@onready var close_button: Button = $Panel/MarginContainer/VBoxContainer/TopBar/CloseButton

func _ready() -> void:
	if send_button:
		send_button.pressed.connect(_on_send_pressed)
	if close_button:
		close_button.pressed.connect(_on_close_pressed)
	if input_field:
		input_field.text_submitted.connect(_on_text_submitted)

func setup(data: Dictionary) -> void:
	_npc_entity = data.get("npc_entity", null)
	if _npc_entity and npc_name_label:
		if _npc_entity is Node and _npc_entity.has_method("get_npc_name"):
			npc_name_label.text = _npc_entity.get_npc_name()
		elif _npc_entity is Dictionary:
			npc_name_label.text = _npc_entity.get("name", "NPC")

func _on_send_pressed() -> void:
	if input_field and not input_field.text.strip_edges().is_empty():
		_send_message(input_field.text.strip_edges())
		input_field.text = ""

func _on_text_submitted(text: String) -> void:
	if not text.strip_edges().is_empty():
		_send_message(text.strip_edges())
		input_field.text = ""

func _send_message(message: String) -> void:
	# 显示玩家消息
	_add_chat_bubble("你", message, true)
	
	# 获取 NPC 回复
	if _npc_entity and _npc_entity is Node and _npc_entity.has_method("get_brain"):
		var brain = _npc_entity.get_brain()
		if brain:
			var response = await brain.chat(message)
			var npc_name = _npc_entity.get_npc_name() if _npc_entity.has_method("get_npc_name") else "NPC"
			_add_chat_bubble(npc_name, response, false)
		else:
			_add_chat_bubble("NPC", "（对方没有回应）", false)
	else:
		_add_chat_bubble("NPC", "（对方没有回应）", false)

func _add_chat_bubble(speaker: String, text: String, is_player: bool) -> void:
	if chat_container == null:
		return
	
	var label = RichTextLabel.new()
	label.bbcode_enabled = true
	label.fit_content = true
	label.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	
	if is_player:
		label.text = "[right][color=cyan]%s: %s[/color][/right]" % [speaker, text]
	else:
		label.text = "[color=yellow]%s: %s[/color]" % [speaker, text]
	
	chat_container.add_child(label)
	
	_chat_history.append({"speaker": speaker, "text": text, "is_player": is_player})

func _on_close_pressed() -> void:
	UIManager.close_panel("dialogue_ui_panel")
