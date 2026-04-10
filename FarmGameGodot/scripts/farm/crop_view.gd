## 作物视图
## 负责显示单个作物的生长状态
## 对应 Unity 的 CropView
class_name CropView
extends Sprite2D

var _soil: SoilEntity = null
var _plant: PlantEntity = null
var _current_stage: int = -1

## 绑定土壤实体
func bind(soil: SoilEntity) -> void:
	_soil = soil
	if _soil and _soil.plant:
		_plant = _soil.plant
		_update_visual()

## 更新视觉显示
func _update_visual() -> void:
	if _plant == null:
		visible = false
		return
	
	visible = true
	var stage = _plant.current_stage
	
	if stage != _current_stage:
		_current_stage = stage
		# 根据种子ID和生长阶段加载对应的纹理
		var seed_id = _plant.seed_config_id
		var tex_path = "res://resources/sprites/plants/%d_stage_%d.png" % [seed_id, stage]
		if ResourceLoader.exists(tex_path):
			texture = load(tex_path)
		else:
			# 使用默认占位符纹理或颜色方块
			_draw_placeholder(stage)
	
	# 成熟时可以添加特效
	if _plant.is_mature:
		modulate = Color(1.0, 1.0, 0.8, 1.0) # 略微发光
	else:
		modulate = Color.WHITE

## 绘制占位符
func _draw_placeholder(stage: int) -> void:
	# 创建一个简单的颜色矩形作为占位
	var img = Image.create(16, 16, false, Image.FORMAT_RGBA8)
	var color = Color.GREEN
	match stage:
		0: color = Color(0.4, 0.8, 0.2) # 幼苗 - 浅绿
		1: color = Color(0.2, 0.7, 0.2) # 生长 - 绿色
		2: color = Color(0.1, 0.6, 0.1) # 成长 - 深绿
		_: color = Color(0.9, 0.8, 0.1) # 成熟 - 金黄
	img.fill(color)
	texture = ImageTexture.create_from_image(img)

func _process(_delta: float) -> void:
	if _plant:
		_update_visual()
