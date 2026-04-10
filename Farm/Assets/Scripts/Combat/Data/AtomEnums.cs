namespace FarmGame.Combat.Data
{
    /// <summary>
    /// 技能形状类型
    /// </summary>
    public enum ShapeType
    {
        /// <summary>点状（单体投射物）</summary>
        Point,
        /// <summary>圆形范围</summary>
        Circle,
        /// <summary>扇形范围</summary>
        Fan,
        /// <summary>直线穿透</summary>
        Line
    }

    /// <summary>
    /// 触发条件类型
    /// </summary>
    public enum TriggerType
    {
        /// <summary>立即触发</summary>
        Immediate,
        /// <summary>被击中时触发</summary>
        OnHit,
        /// <summary>血量阈值触发</summary>
        HPThreshold,
        /// <summary>间隔触发</summary>
        Interval,
        /// <summary>击杀时触发</summary>
        OnKill
    }

    /// <summary>
    /// 目标类型
    /// </summary>
    public enum TargetType
    {
        /// <summary>自身</summary>
        Self,
        /// <summary>单体敌人</summary>
        SingleEnemy,
        /// <summary>区域内敌人</summary>
        Area,
        /// <summary>全体敌人</summary>
        AllEnemies,
        /// <summary>最近敌人</summary>
        Nearest
    }

    /// <summary>
    /// 实体类型
    /// </summary>
    public enum EntityType
    {
        /// <summary>玩家</summary>
        Player,
        /// <summary>敌人</summary>
        Enemy
    }

    /// <summary>
    /// 状态效果类型
    /// </summary>
    public enum StatusEffectType
    {
        /// <summary>持续伤害/治疗</summary>
        DamageOverTime,
        /// <summary>减速</summary>
        Slow,
        /// <summary>沉默（禁止释放技能）</summary>
        Silence,
        /// <summary>易伤（增加受到的伤害）</summary>
        Vulnerable,
        /// <summary>减伤（减少受到的伤害）</summary>
        DamageReduction,
        /// <summary>隐身</summary>
        Stealth,
        /// <summary>移速变化</summary>
        MoveSpeedMod,
        /// <summary>攻击力变化</summary>
        AttackMod,
        /// <summary>防御变化</summary>
        DefenseMod
    }
}
