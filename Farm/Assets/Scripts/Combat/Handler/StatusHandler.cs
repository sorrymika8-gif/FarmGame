using FarmGame.Combat.Entity;
using FarmGame.Combat.Data;

namespace FarmGame.Combat.Handler
{
    /// <summary>
    /// 状态效果处理器 - 辅助创建和管理状态效果
    /// </summary>
    public static class StatusHandler
    {
        /// <summary>
        /// 从技能数据创建减速效果
        /// </summary>
        /// <param name="data">技能原子数据</param>
        /// <returns>状态效果，如果数据无效则返回 null</returns>
        public static StatusEffect CreateSlowEffect(SkillAtomData data)
        {
            if (data == null || data.slowPercent <= 0 || data.duration <= 0)
                return null;

            return new StatusEffect(
                StatusEffectType.Slow,
                data.slowPercent,
                data.duration
            );
        }

        /// <summary>
        /// 从技能数据创建沉默效果
        /// </summary>
        /// <param name="data">技能原子数据</param>
        /// <returns>状态效果</returns>
        public static StatusEffect CreateSilenceEffect(SkillAtomData data)
        {
            if (data == null || data.silenceDuration <= 0)
                return null;

            return new StatusEffect(
                StatusEffectType.Silence,
                1f,
                data.silenceDuration
            );
        }

        /// <summary>
        /// 从技能数据创建 DoT 效果
        /// </summary>
        /// <param name="data">技能原子数据</param>
        /// <param name="tickInterval">Tick 间隔</param>
        /// <returns>状态效果</returns>
        public static StatusEffect CreateDotEffect(SkillAtomData data, float tickInterval = 1f)
        {
            if (data == null || data.dotHP == 0 || data.duration <= 0)
                return null;

            return new StatusEffect(
                StatusEffectType.DamageOverTime,
                data.dotHP,
                data.duration,
                tickInterval
            );
        }

        /// <summary>
        /// 从技能数据创建隐身效果
        /// </summary>
        /// <param name="data">技能原子数据</param>
        /// <returns>状态效果</returns>
        public static StatusEffect CreateStealthEffect(SkillAtomData data)
        {
            if (data == null || data.stealthDuration <= 0)
                return null;

            return new StatusEffect(
                StatusEffectType.Stealth,
                1f,
                data.stealthDuration
            );
        }

        /// <summary>
        /// 创建移速加成效果
        /// </summary>
        /// <param name="percentChange">百分比变化</param>
        /// <param name="duration">持续时间</param>
        /// <returns>状态效果</returns>
        public static StatusEffect CreateMoveSpeedEffect(float percentChange, float duration)
        {
            if (duration <= 0 || percentChange == 0)
                return null;

            return new StatusEffect(
                StatusEffectType.MoveSpeedMod,
                percentChange,
                duration
            );
        }

        /// <summary>
        /// 创建攻击力加成效果
        /// </summary>
        /// <param name="percentChange">百分比变化</param>
        /// <param name="duration">持续时间</param>
        /// <returns>状态效果</returns>
        public static StatusEffect CreateAttackEffect(float percentChange, float duration)
        {
            if (duration <= 0 || percentChange == 0)
                return null;

            return new StatusEffect(
                StatusEffectType.AttackMod,
                percentChange,
                duration
            );
        }

        /// <summary>
        /// 创建防御加成效果
        /// </summary>
        /// <param name="percentChange">百分比变化</param>
        /// <param name="duration">持续时间</param>
        /// <returns>状态效果</returns>
        public static StatusEffect CreateDefenseEffect(float percentChange, float duration)
        {
            if (duration <= 0 || percentChange == 0)
                return null;

            return new StatusEffect(
                StatusEffectType.DefenseMod,
                percentChange,
                duration
            );
        }

        /// <summary>
        /// 创建易伤效果
        /// </summary>
        /// <param name="multiplier">伤害倍率增加量（如 0.5 = 增加 50% 受到的伤害）</param>
        /// <param name="duration">持续时间</param>
        /// <returns>状态效果</returns>
        public static StatusEffect CreateVulnerableEffect(float multiplier, float duration)
        {
            if (duration <= 0 || multiplier <= 0)
                return null;

            return new StatusEffect(
                StatusEffectType.Vulnerable,
                multiplier,
                duration
            );
        }

        /// <summary>
        /// 创建减伤效果
        /// </summary>
        /// <param name="reduction">伤害减少量（如 0.3 = 减少 30% 受到的伤害）</param>
        /// <param name="duration">持续时间</param>
        /// <returns>状态效果</returns>
        public static StatusEffect CreateDamageReductionEffect(float reduction, float duration)
        {
            if (duration <= 0 || reduction <= 0)
                return null;

            return new StatusEffect(
                StatusEffectType.DamageReduction,
                reduction,
                duration
            );
        }

        /// <summary>
        /// 批量应用状态效果到目标
        /// </summary>
        /// <param name="target">目标角色</param>
        /// <param name="effects">效果数组</param>
        /// <param name="stackable">是否可叠加</param>
        public static void ApplyEffects(CharEntity target, StatusEffect[] effects, bool stackable = false)
        {
            if (target == null || effects == null) return;

            foreach (var effect in effects)
            {
                if (effect != null)
                {
                    target.ApplyStatus(effect, stackable);
                }
            }
        }
    }
}
