class_name AtomEnums
extends RefCounted

## 技能形状类型
enum ShapeType {
	POINT,    ## 点状（单体投射物）
	CIRCLE,   ## 圆形范围
	FAN,      ## 扇形范围
	LINE      ## 直线穿透
}

## 触发条件类型
enum TriggerType {
	IMMEDIATE,     ## 立即触发
	ON_HIT,        ## 被击中时触发
	HP_THRESHOLD,  ## 血量阈值触发
	INTERVAL,      ## 间隔触发
	ON_KILL        ## 击杀时触发
}

## 目标类型
enum TargetType {
	SELF,          ## 自身
	SINGLE_ENEMY,  ## 单体敌人
	AREA,          ## 区域内敌人
	ALL_ENEMIES,   ## 全体敌人
	NEAREST        ## 最近敌人
}

## 实体类型
enum EntityType {
	PLAYER,  ## 玩家
	ENEMY    ## 敌人
}

## 状态效果类型
enum StatusEffectType {
	DAMAGE_OVER_TIME,  ## 持续伤害/治疗
	SLOW,              ## 减速
	SILENCE,           ## 沉默（禁止释放技能）
	VULNERABLE,        ## 易伤（增加受到的伤害）
	DAMAGE_REDUCTION,  ## 减伤（减少受到的伤害）
	STEALTH,           ## 隐身
	MOVE_SPEED_MOD,    ## 移速变化
	ATTACK_MOD,        ## 攻击力变化
	DEFENSE_MOD        ## 防御变化
}

## 材料类型（技能合成用）
enum MaterialType {
	NORMAL,     ## 普通材料
	OFFENSIVE,  ## 攻击型材料
	DEFENSIVE,  ## 防御型材料
	SPEED,      ## 速度型材料
	CONTROL,    ## 控制型材料
	RARE        ## 稀有材料
}
