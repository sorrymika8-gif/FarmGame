using System;
using System.Collections.Generic;
using UnityEngine;

namespace FarmGame.Combat.Data
{
    /// <summary>
    /// 实体属性容器
    /// 管理角色的基础属性和状态效果
    /// </summary>
    [Serializable]
    public class EntityStats
    {
        #region 基础属性

        /// <summary>最大生命值</summary>
        public float MaxHP = 100f;

        /// <summary>当前生命值</summary>
        public float CurrentHP = 100f;

        /// <summary>基础移动速度</summary>
        public float BaseMoveSpeed = 5f;

        /// <summary>基础攻击力</summary>
        public float BaseAttack = 10f;

        /// <summary>基础防御</summary>
        public float BaseDefense = 5f;

        #endregion

        #region 计算后属性（受状态效果影响）

        /// <summary>当前移动速度</summary>
        public float MoveSpeed => CalculateMoveSpeed();

        /// <summary>当前攻击力</summary>
        public float Attack => CalculateAttack();

        /// <summary>当前防御</summary>
        public float Defense => CalculateDefense();

        /// <summary>是否被沉默</summary>
        public bool IsSilenced => HasEffect(StatusEffectType.Silence);

        /// <summary>是否隐身</summary>
        public bool IsStealthed => HasEffect(StatusEffectType.Stealth);

        /// <summary>受到伤害倍率</summary>
        public float DamageMultiplier => CalculateDamageMultiplier();

        /// <summary>生命值百分比</summary>
        public float HPPercent => MaxHP > 0 ? CurrentHP / MaxHP : 0f;

        /// <summary>是否存活</summary>
        public bool IsAlive => CurrentHP > 0f;

        #endregion

        #region 状态效果

        /// <summary>当前激活的状态效果列表</summary>
        private List<StatusEffect> mActiveEffects = new List<StatusEffect>();

        /// <summary>获取所有激活的状态效果（只读）</summary>
        public IReadOnlyList<StatusEffect> ActiveEffects => mActiveEffects;

        #endregion

        #region 事件

        /// <summary>状态效果触发 Tick 时的回调（用于 DoT 伤害等）</summary>
        public Action<StatusEffect> OnEffectTick;

        /// <summary>状态效果过期时的回调</summary>
        public Action<StatusEffect> OnEffectExpired;

        /// <summary>生命值变化时的回调（参数：变化量，正为治疗负为伤害）</summary>
        public Action<float> OnHPChanged;

        /// <summary>死亡时的回调</summary>
        public Action OnDeath;

        #endregion

        #region 公共方法

        /// <summary>
        /// 每帧更新状态效果
        /// </summary>
        /// <param name="deltaTime">时间增量</param>
        public void Tick(float deltaTime)
        {
            for (int i = mActiveEffects.Count - 1; i >= 0; i--)
            {
                var effect = mActiveEffects[i];

                // 更新效果并检查是否触发 Tick
                if (effect.Tick(deltaTime))
                {
                    OnEffectTick?.Invoke(effect);

                    // 处理 DoT 效果
                    if (effect.EffectType == StatusEffectType.DamageOverTime)
                    {
                        ApplyHPChange(effect.Value);
                    }
                }

                // 移除过期效果
                if (effect.IsExpired)
                {
                    OnEffectExpired?.Invoke(effect);
                    mActiveEffects.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 添加状态效果
        /// </summary>
        /// <param name="effect">要添加的效果</param>
        /// <param name="stackable">是否可叠加（同类型效果）</param>
        public void ApplyEffect(StatusEffect effect, bool stackable = false)
        {
            if (effect == null) return;

            // 查找同类型效果
            var existingEffect = mActiveEffects.Find(e => e.EffectType == effect.EffectType);

            if (existingEffect != null)
            {
                if (stackable)
                {
                    existingEffect.Stack(effect.Value);
                    existingEffect.Refresh(effect.RemainingDuration);
                }
                else
                {
                    // 刷新持续时间，使用较高的数值
                    existingEffect.Refresh(Mathf.Max(existingEffect.RemainingDuration, effect.RemainingDuration));
                    existingEffect.Value = Mathf.Max(existingEffect.Value, effect.Value);
                }
            }
            else
            {
                mActiveEffects.Add(effect);
            }
        }

        /// <summary>
        /// 移除指定类型的状态效果
        /// </summary>
        /// <param name="effectType">效果类型</param>
        /// <returns>是否成功移除</returns>
        public bool RemoveEffect(StatusEffectType effectType)
        {
            return mActiveEffects.RemoveAll(e => e.EffectType == effectType) > 0;
        }

        /// <summary>
        /// 移除所有状态效果
        /// </summary>
        public void ClearAllEffects()
        {
            mActiveEffects.Clear();
        }

        /// <summary>
        /// 检查是否有指定类型的状态效果
        /// </summary>
        /// <param name="effectType">效果类型</param>
        /// <returns>是否存在</returns>
        public bool HasEffect(StatusEffectType effectType)
        {
            return mActiveEffects.Exists(e => e.EffectType == effectType);
        }

        /// <summary>
        /// 获取指定类型状态效果的总数值
        /// </summary>
        /// <param name="effectType">效果类型</param>
        /// <returns>效果数值总和</returns>
        public float GetEffectValue(StatusEffectType effectType)
        {
            float total = 0f;
            foreach (var effect in mActiveEffects)
            {
                if (effect.EffectType == effectType)
                {
                    total += effect.Value;
                }
            }
            return total;
        }

        /// <summary>
        /// 应用生命值变化
        /// </summary>
        /// <param name="amount">变化量（正为治疗，负为伤害）</param>
        public void ApplyHPChange(float amount)
        {
            if (!IsAlive && amount < 0) return;

            float previousHP = CurrentHP;
            CurrentHP = Mathf.Clamp(CurrentHP + amount, 0f, MaxHP);

            float actualChange = CurrentHP - previousHP;
            if (Mathf.Abs(actualChange) > 0.001f)
            {
                OnHPChanged?.Invoke(actualChange);
            }

            if (previousHP > 0f && CurrentHP <= 0f)
            {
                OnDeath?.Invoke();
            }
        }

        /// <summary>
        /// 重置属性到初始状态
        /// </summary>
        public void Reset()
        {
            CurrentHP = MaxHP;
            ClearAllEffects();
        }

        #endregion

        #region 私有方法

        private float CalculateMoveSpeed()
        {
            float speed = BaseMoveSpeed;

            // 应用移速加成
            float speedMod = GetEffectValue(StatusEffectType.MoveSpeedMod);
            speed *= (1f + speedMod / 100f);

            // 应用减速效果
            float slowPercent = GetEffectValue(StatusEffectType.Slow);
            speed *= (1f - slowPercent / 100f);

            return Mathf.Max(0f, speed);
        }

        private float CalculateAttack()
        {
            float attack = BaseAttack;

            // 应用攻击力加成
            float attackMod = GetEffectValue(StatusEffectType.AttackMod);
            attack *= (1f + attackMod / 100f);

            return Mathf.Max(0f, attack);
        }

        private float CalculateDefense()
        {
            float defense = BaseDefense;

            // 应用防御加成
            float defenseMod = GetEffectValue(StatusEffectType.DefenseMod);
            defense *= (1f + defenseMod / 100f);

            return Mathf.Max(0f, defense);
        }

        private float CalculateDamageMultiplier()
        {
            float multiplier = 1f;

            // 易伤效果
            float vulnerable = GetEffectValue(StatusEffectType.Vulnerable);
            multiplier += vulnerable;

            // 减伤效果
            float reduction = GetEffectValue(StatusEffectType.DamageReduction);
            multiplier -= reduction;

            return Mathf.Max(0f, multiplier);
        }

        #endregion
    }
}
