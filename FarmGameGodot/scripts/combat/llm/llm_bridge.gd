class_name LLMBridge
extends RefCounted

## LLM 桥接层 - 调用 LLM 生成技能数据

static var instance: LLMBridge:
	get:
		if instance == null:
			instance = LLMBridge.new()
		return instance

var _is_initialized: bool = false

## 初始化
func initialize() -> void:
	if _is_initialized:
		return
	_is_initialized = true
	print("[LLMBridge] 已初始化")

## 合成技能
func synthesize_async(materials: Array[MaterialData]) -> SkillAtomData:
	var client = LLMService.get_client()
	if client == null:
		push_warning("[LLMBridge] LLMService 客户端为空")
		return _create_fallback_skill(materials)
	# 构建请求
	var messages := [
		{"role": "system", "content": SkillPromptBuilder.build_system_prompt()},
		{"role": "user", "content": SkillPromptBuilder.build_user_prompt(materials)}
	]
	var response = await client.chat_async(messages, 0.8, 512)
	if response == null or response.is_empty():
		push_warning("[LLMBridge] LLM 合成失败")
		return _create_fallback_skill(materials)
	# 解析JSON
	var json := JSON.new()
	var err := json.parse(response)
	if err != OK:
		push_warning("[LLMBridge] JSON 解析失败: %s" % json.get_error_message())
		return _create_fallback_skill(materials)
	var data := SkillAtomData.from_dict(json.data)
	print("[LLMBridge] 合成技能: %s" % data.display_name)
	return data

## 创建回退技能
func _create_fallback_skill(materials: Array[MaterialData]) -> SkillAtomData:
	var skill := SkillAtomData.new()
	skill.display_name = "基础攻击"
	skill.direct_hp = -20.0
	skill.projectile_speed = AtomConstants.DEFAULT_PROJECTILE_SPEED
	skill.cooldown = 1.0
	if not materials.is_empty():
		skill.direct_hp = -20.0 - (materials.size() * 5.0)
		skill.display_name = "合成攻击 Lv.%d" % materials.size()
		for material in materials:
			match material.material_type:
				AtomEnums.MaterialType.OFFENSIVE:
					skill.direct_hp -= 10.0
				AtomEnums.MaterialType.DEFENSIVE:
					skill.direct_hp += 5.0
					skill.duration = 3.0
					skill.defense_mod = 10.0
				AtomEnums.MaterialType.SPEED:
					skill.projectile_speed += 2.0
					skill.tracking = 45.0
				AtomEnums.MaterialType.CONTROL:
					skill.slow_percent = 20.0
					skill.duration = 2.0
	skill.clamp_values()
	return skill

## 创建测试用技能
static func create_test_skill(skill_type: String = "basic") -> SkillAtomData:
	var skill := SkillAtomData.new()
	match skill_type.to_lower():
		"tracking":
			skill.display_name = "追踪弹"
			skill.direct_hp = -15.0
			skill.projectile_speed = 8.0
			skill.tracking = 180.0
		"split":
			skill.display_name = "分裂弹"
			skill.direct_hp = -10.0
			skill.projectile_speed = 10.0
			skill.split = 3
		"bounce":
			skill.display_name = "弹射弹"
			skill.direct_hp = -12.0
			skill.projectile_speed = 12.0
			skill.bounce = 3
		"aoe":
			skill.display_name = "范围爆发"
			skill.direct_hp = -25.0
			skill.projectile_speed = 6.0
			skill.aoe_radius = 3.0
			skill.shape = AtomEnums.ShapeType.CIRCLE
		"dot":
			skill.display_name = "毒弹"
			skill.direct_hp = -5.0
			skill.dot_hp = -8.0
			skill.duration = 5.0
			skill.projectile_speed = 10.0
		"combo":
			skill.display_name = "涌现风暴"
			skill.direct_hp = -8.0
			skill.projectile_speed = 10.0
			skill.tracking = 90.0
			skill.split = 2
			skill.bounce = 2
			skill.pierce = 1
		_:
			skill.display_name = "基础弹"
			skill.direct_hp = -20.0
			skill.projectile_speed = AtomConstants.DEFAULT_PROJECTILE_SPEED
	skill.clamp_values()
	return skill
