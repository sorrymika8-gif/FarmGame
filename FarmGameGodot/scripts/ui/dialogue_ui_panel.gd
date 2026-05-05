## 对话面板
## 对应 Unity 的 DialogueUIPanel
extends Control

var _npc_entity = null
var _chat_history: Array = []
var _waiting_for_response: bool = false
var _received_response: bool = false

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
	var callback = Callable(self, "_on_dialogue_speak")
	if not CommandExecutors.on_dialogue_speak.has(callback):
		CommandExecutors.on_dialogue_speak.append(callback)

func _exit_tree() -> void:
	CommandExecutors.on_dialogue_speak.erase(Callable(self, "_on_dialogue_speak"))

func setup(data: Dictionary) -> void:
	_npc_entity = data.get("npc_entity", null)
	if npc_name_label:
		npc_name_label.text = _get_npc_name()

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

	if _npc_entity == null or not _npc_entity.has_method("receive_chat_async"):
		_add_chat_bubble(_get_npc_name(), "（对方没有回应）", false)
		return

	_set_input_enabled(false)
	_waiting_for_response = true
	_received_response = false
	var result = await _npc_entity.receive_chat_async(message)
	_waiting_for_response = false
	_set_input_enabled(true)

	if result != null and not result.success:
		_add_chat_bubble(_get_npc_name(), "（LLM调用失败：%s）" % result.error_message, false)
		return
	
	if not _received_response:
		_add_chat_bubble(_get_npc_name(), "（对方没有回应）", false)

func _on_dialogue_speak(npc_entity, content: String) -> void:
	if npc_entity != _npc_entity:
		return
	_received_response = true
	_add_chat_bubble(_get_npc_name(), content, false)

func _set_input_enabled(enabled: bool) -> void:
	if send_button:
		send_button.disabled = not enabled
	if input_field:
		input_field.editable = enabled

func _get_npc_name() -> String:
	if _npc_entity == null:
		return "NPC"
	if _npc_entity is Dictionary:
		return str(_npc_entity.get("name", "NPC"))
	if _npc_entity.has_method("get_npc_name"):
		return str(_npc_entity.get_npc_name())
	var entity_name = _npc_entity.get("npc_name")
	if entity_name != null and not str(entity_name).is_empty():
		return str(entity_name)
	return "NPC"

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
