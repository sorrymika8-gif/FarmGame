using System;
using UnityEngine;

namespace FarmGame.Combat.Data
{
    /// <summary>
    /// 技能原子数据 - 核心数据结构
    /// 字段直接对应正交原子池，从 LLM 返回的 JSON 一行反序列化
    /// </summary>
    [Serializable]
    public class SkillAtomData
    {
        #region 弹道行为

        /// <summary>弹射次数（0 = 不弹射）</summary>
        public int bounce;

        /// <summary>追踪角度（0 = 不追踪，360 = 全向追踪）</summary>
        public float tracking;

        /// <summary>穿透目标数（0 = 命中即消失）</summary>
        public int pierce;

        /// <summary>分裂数量（0 = 不分裂）</summary>
        public int split;

        /// <summary>是否返回（回旋镖效果）</summary>
        public bool returning;

        /// <summary>吸引/排斥强度（正 = 吸引，负 = 排斥，0 = 无效果）</summary>
        public float attract;

        #endregion

        #region 范围规则

        /// <summary>AOE 半径（0 = 单体）</summary>
        public float aoeRadius;

        /// <summary>形状类型</summary>
        public ShapeType shape;

        /// <summary>弹道宽度（用于直线型技能）</summary>
        public float projectileWidth;

        #endregion

        #region 数值效果

        /// <summary>直接生命变化（正 = 治疗，负 = 伤害）</summary>
        public float directHP;

        /// <summary>每秒持续生命变化（正 = 持续治疗，负 = 持续伤害）</summary>
        public float dotHP;

        /// <summary>移速变化百分比</summary>
        public float moveSpeedMod;

        /// <summary>攻击力变化百分比</summary>
        public float attackMod;

        /// <summary>防御变化百分比</summary>
        public float defenseMod;

        #endregion

        #region 状态效果

        /// <summary>减速百分比（0-100）</summary>
        public float slowPercent;

        /// <summary>沉默持续秒数</summary>
        public float silenceDuration;

        /// <summary>易伤/减伤倍率（>1 = 易伤，<1 = 减伤）</summary>
        public float damageMultiplier;

        /// <summary>隐身持续秒数</summary>
        public float stealthDuration;

        #endregion

        #region 触发条件

        /// <summary>触发类型</summary>
        public TriggerType trigger;

        /// <summary>作用目标类型</summary>
        public TargetType target;

        #endregion

        #region 时间参数

        /// <summary>延迟释放秒数</summary>
        public float delay;

        /// <summary>效果持续时长</summary>
        public float duration;

        /// <summary>冷却时间</summary>
        public float cooldown;

        #endregion

        #region 元信息

        /// <summary>技能名（纯展示用）</summary>
        public string displayName;

        /// <summary>投射物飞行速度</summary>
        public float projectileSpeed;

        #endregion

        #region 方法

        /// <summary>
        /// 深拷贝技能数据（用于分裂等场景）
        /// </summary>
        /// <returns>拷贝后的新实例</returns>
        public SkillAtomData Clone()
        {
            return new SkillAtomData
            {
                // 弹道行为
                bounce = this.bounce,
                tracking = this.tracking,
                pierce = this.pierce,
                split = this.split,
                returning = this.returning,
                attract = this.attract,

                // 范围规则
                aoeRadius = this.aoeRadius,
                shape = this.shape,
                projectileWidth = this.projectileWidth,

                // 数值效果
                directHP = this.directHP,
                dotHP = this.dotHP,
                moveSpeedMod = this.moveSpeedMod,
                attackMod = this.attackMod,
                defenseMod = this.defenseMod,

                // 状态效果
                slowPercent = this.slowPercent,
                silenceDuration = this.silenceDuration,
                damageMultiplier = this.damageMultiplier,
                stealthDuration = this.stealthDuration,

                // 触发条件
                trigger = this.trigger,
                target = this.target,

                // 时间参数
                delay = this.delay,
                duration = this.duration,
                cooldown = this.cooldown,

                // 元信息
                displayName = this.displayName,
                projectileSpeed = this.projectileSpeed
            };
        }

        /// <summary>
        /// 应用数值限制（安全阀）
        /// </summary>
        public void Clamp()
        {
            // 弹道行为
            bounce = Mathf.Clamp(bounce, 0, AtomConstants.MaxBounce);
            tracking = Mathf.Clamp(tracking, 0f, AtomConstants.MaxTracking);
            pierce = Mathf.Clamp(pierce, 0, AtomConstants.MaxPierce);
            split = Mathf.Clamp(split, 0, AtomConstants.MaxSplit);
            attract = Mathf.Clamp(attract, -AtomConstants.MaxAttract, AtomConstants.MaxAttract);

            // 范围规则
            aoeRadius = Mathf.Clamp(aoeRadius, 0f, AtomConstants.MaxAOE);
            projectileWidth = Mathf.Clamp(projectileWidth, 0f, AtomConstants.MaxProjectileWidth);

            // 数值效果
            directHP = Mathf.Clamp(directHP, -AtomConstants.MaxDirectHP, AtomConstants.MaxDirectHP);
            dotHP = Mathf.Clamp(dotHP, -AtomConstants.MaxDotHP, AtomConstants.MaxDotHP);
            moveSpeedMod = Mathf.Clamp(moveSpeedMod, -AtomConstants.MaxMoveSpeedMod, AtomConstants.MaxMoveSpeedMod);
            attackMod = Mathf.Clamp(attackMod, -AtomConstants.MaxAttackMod, AtomConstants.MaxAttackMod);
            defenseMod = Mathf.Clamp(defenseMod, -AtomConstants.MaxDefenseMod, AtomConstants.MaxDefenseMod);

            // 状态效果
            slowPercent = Mathf.Clamp(slowPercent, 0f, AtomConstants.MaxSlowPercent);
            silenceDuration = Mathf.Clamp(silenceDuration, 0f, AtomConstants.MaxSilenceDuration);
            damageMultiplier = Mathf.Clamp(damageMultiplier, 0f, AtomConstants.MaxDamageMultiplier);
            stealthDuration = Mathf.Clamp(stealthDuration, 0f, AtomConstants.MaxStealthDuration);

            // 时间参数
            delay = Mathf.Clamp(delay, 0f, AtomConstants.MaxDelay);
            duration = Mathf.Clamp(duration, 0f, AtomConstants.MaxDuration);
            cooldown = Mathf.Clamp(cooldown, AtomConstants.MinCooldown, AtomConstants.MaxCooldown);

            // 速度
            if (projectileSpeed <= 0f)
            {
                projectileSpeed = AtomConstants.DefaultProjectileSpeed;
            }
            projectileSpeed = Mathf.Clamp(projectileSpeed, AtomConstants.MinProjectileSpeed, AtomConstants.MaxProjectileSpeed);
        }

        #endregion
    }
}
