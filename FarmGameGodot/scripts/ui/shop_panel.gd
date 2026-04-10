## 商店面板
## 对应 Unity 的 ShopPanel
extends Control

var _shop_type: int = 0
var _player_inventory = null
var _shop_items: Array = []

@onready var item_container: VBoxContainer = $Panel/MarginContainer/VBoxContainer/ScrollContainer/ItemContainer
@onready var close_button: Button = $Panel/MarginContainer/VBoxContainer/TopBar/CloseButton
@onready var title_label: Label = $Panel/MarginContainer/VBoxContainer/TopBar/TitleLabel
@onready var gold_label: Label = $Panel/MarginContainer/VBoxContainer/BottomBar/GoldLabel

func _ready() -> void:
	if close_button:
		close_button.pressed.connect(_on_close_pressed)
	PlayerManager.gold_changed.connect(_update_gold)

func setup(data: Dictionary) -> void:
	_shop_type = data.get("shop_type", 0)
	_player_inventory = data.get("player_inventory", null)
	
	if title_label:
		match _shop_type:
			1: title_label.text = "种子店"
			2: title_label.text = "工具店"
			3: title_label.text = "杂货店"
			_: title_label.text = "商店"
	
	_update_gold(PlayerManager.gold)
	_load_shop_items()

func _load_shop_items() -> void:
	_shop_items = ShopManager.get_shop_items(_shop_type)
	_refresh_display()

func _refresh_display() -> void:
	if item_container == null:
		return
	
	for child in item_container.get_children():
		child.queue_free()
	
	for shop_item in _shop_items:
		var hbox = HBoxContainer.new()
		
		var name_label = Label.new()
		name_label.text = shop_item.get("item_name", "???")
		name_label.size_flags_horizontal = Control.SIZE_EXPAND_FILL
		hbox.add_child(name_label)
		
		var price_label = Label.new()
		price_label.text = "%d金" % shop_item.get("buy_price", 0)
		hbox.add_child(price_label)
		
		var buy_button = Button.new()
		buy_button.text = "购买"
		var item_id = shop_item.get("item_id", 0)
		buy_button.pressed.connect(_on_buy_pressed.bind(item_id))
		hbox.add_child(buy_button)
		
		item_container.add_child(hbox)

func _on_buy_pressed(item_id: int) -> void:
	if ShopManager.buy_item(item_id, 1, _shop_type):
		_refresh_display()

func _update_gold(amount: int) -> void:
	if gold_label:
		gold_label.text = "金币: %d" % amount

func _on_close_pressed() -> void:
	UIManager.close_panel("shop_panel")
