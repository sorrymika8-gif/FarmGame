using UnityEngine;
using FarmGame.Combat.Data;

namespace FarmGame.Combat.Entity
{
    /// <summary>
    /// 效果应用器 - 将技能原子数据应用到目标角色
    /// 静态工具类，封装效果应用逻辑
    /// </summary>
    public static class EffectApplier
    {
        #region 公共方法

        /// <summary>
        /// 将技能效果应用到目标
        /// </summary>
        /// <param name="data">技能原子数据</param>
        /// <param name="target">目标角色</param>
        /// <param name="source">技能来源（可选，用于追踪）</param>
        public static void Apply(SkillAtomData data, CharEntity target, CharEntity source = null)
        {
            if (data == null || target == null || !target.IsAlive) return;

            // 1. 应用直接生命值变化
            ApplyDirectHP(data, target);

            // 2. 应用持续生命值变化（DoT/HoT）
            ApplyDotHP(data, target);

            // 3. 应用减速效果
            ApplySlow(data, target);

            // 4. 应用沉默效果
            ApplySilence(data, target);

            // 5. 应用易伤/减伤效果
            ApplyDamageMultiplier(data, target);

            // 6. 应用隐身效果
            ApplyStealth(data, target);

            // 7. 应用属性变化
            ApplyStatModifiers(data, target);
        }

        /// <summary>
        /// 仅应用即时伤害/治疗（不含状态效果）
        /// </summary>
        /// <param name="data">技能原子数据</param>
        /// <param name="target">目标角色</param>
        /// <returns>实际造成的伤害（正值）或治疗（负值）</returns>
        public static float ApplyInstantOnly(SkillAtomData data, CharEntity target)
        {
            if (data == null || target == null || !target.IsAlive) return 0f;

            return ApplyDirectHP(data, target);
        }

        #endregion

        #region 私有方法 - 各效果应用

        /// <summary>
        /// 应用直接生命值变化
        /// </summary>
        private static float ApplyDirectHP(SkillAtomData data, CharEntity target)
        {
            if (Mathf.Approximately(data.directHP, 0f)) return 0f;

            if (data.directHP < 0)
            {
                // 伤害
                return target.TakeDamage(-data.directHP);
            }
            else
            {
                // 治疗
                return -target.Heal(data.directHP);
            }
        }

        /// <summary>
        /// 应用持续生命值变化
        /// </summary>
        private static void ApplyDotHP(SkillAtomData data, CharEntity target)
        {
            if (Mathf.Approximately(data.dotHP, 0f) || data.duration <= 0f) return;

            var effect = new StatusEffect(
                StatusEffectType.DamageOverTime,
                data.dotHP,  // 每 Tick 的生命值变化
                data.duration,
                1f  // 默认 1 秒 Tick 一次
            );

            target.ApplyStatus(effect, stackable: true);
        }

        /// <summary>
        /// 应用减速效果
        /// </summary>
        private static void ApplySlow(SkillAtomData data, CharEntity target)
        {
            if (data.slowPercent <= 0f || data.duration <= 0f) return;

            var effect = new StatusEffect(
                StatusEffectType.Slow,
                data.slowPercent,
                data.duration
            );

            target.ApplyStatus(effect);
        }

        /// <summary>
        /// 应用沉默效果
        /// </summary>
        private static void ApplySilence(SkillAtomData data, CharEntity target)
        {
            if (data.silenceDuration <= 0f) return;

            var effect = new StatusEffect(
                StatusEffectType.Silence,
                1f,  // 沉默是布尔效果，数值无意义
                data.silenceDuration
            );

            target.ApplyStatus(effect);
        }

        /// <summary>
        /// 应用易伤/减伤效果
        /// </summary>
        private static void ApplyDamageMultiplier(SkillAtomData data, CharEntity target)
        {
            if (Mathf.Approximately(data.damageMultiplier, 1f) || data.duration <= 0f) return;

            StatusEffectType effectType;
            float value;

            if (data.damageMultiplier > 1f)
            {
                // 易伤
                effectType = StatusEffectType.Vulnerable;
                value = data.damageMultiplier - 1f;
            }
            else
            {
                // 减伤
                effectType = StatusEffectType.DamageReduction;
                value = 1f - data.damageMultiplier;
            }

            var effect = new StatusEffect(effectType, value, data.duration);
            target.ApplyStatus(effect);
        }

        /// <summary>
        /// 应用隐身效果
        /// </summary>
        private static void ApplyStealth(SkillAtomData data, CharEntity target)
        {
            if (data.stealthDuration <= 0f) return;

            var effect = new StatusEffect(
                StatusEffectType.Stealth,
                1f,  // 隐身是布尔效果
                data.stealthDuration
            );

            target.ApplyStatus(effect);
        }

        /// <summary>
        /// 应用属性变化效果
        /// </summary>
        private static void ApplyStatModifiers(SkillAtomData data, CharEntity target)
        {
            if (data.duration <= 0f) return;

            // 移速变化
            if (!Mathf.Approximately(data.moveSpeedMod, 0f))
            {
                var effect = new StatusEffect(
                    StatusEffectType.MoveSpeedMod,
                    data.moveSpeedMod,
                    data.duration
                );
                target.ApplyStatus(effect, stackable: true);
            }

            // 攻击力变化
            if (!Mathf.Approximately(data.attackMod, 0f))
            {
                var effect = new StatusEffect(
                    StatusEffectType.AttackMod,
                    data.attackMod,
                    data.duration
                );
                target.ApplyStatus(effect, stackable: true);
            }

            // 防御变化
            if (!Mathf.Approximately(data.defenseMod, 0f))
            {
                var effect = new StatusEffect(
                    StatusEffectType.DefenseMod,
                    data.defenseMod,
                    data.duration
                );
                target.ApplyStatus(effect, stackable: true);
            }
        }

        #endregion
    }
}
