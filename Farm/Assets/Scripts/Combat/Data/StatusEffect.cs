using System;

namespace FarmGame.Combat.Data
{
    /// <summary>
    /// 状态效果数据
    /// </summary>
    [Serializable]
    public class StatusEffect
    {
        /// <summary>效果类型</summary>
        public StatusEffectType EffectType;

        /// <summary>效果数值（含义取决于 EffectType）</summary>
        public float Value;

        /// <summary>剩余持续时间（秒）</summary>
        public float RemainingDuration;

        /// <summary>Tick 间隔（用于 DoT 类效果）</summary>
        public float TickInterval;

        /// <summary>距下次 Tick 的时间</summary>
        public float TimeToNextTick;

        /// <summary>效果来源（用于追踪和去重）</summary>
        public string SourceId;

        /// <summary>
        /// 创建状态效果
        /// </summary>
        /// <param name="effectType">效果类型</param>
        /// <param name="value">效果数值</param>
        /// <param name="duration">持续时间</param>
        /// <param name="tickInterval">Tick 间隔（DoT 用）</param>
        /// <param name="sourceId">来源 ID</param>
        public StatusEffect(
            StatusEffectType effectType,
            float value,
            float duration,
            float tickInterval = 1f,
            string sourceId = null)
        {
            EffectType = effectType;
            Value = value;
            RemainingDuration = duration;
            TickInterval = tickInterval;
            TimeToNextTick = tickInterval;
            SourceId = sourceId ?? Guid.NewGuid().ToString();
        }

        /// <summary>
        /// 检查效果是否已过期
        /// </summary>
        public bool IsExpired => RemainingDuration <= 0f;

        /// <summary>
        /// 更新效果时间
        /// </summary>
        /// <param name="deltaTime">时间增量</param>
        /// <returns>是否触发了 Tick（用于 DoT）</returns>
        public bool Tick(float deltaTime)
        {
            RemainingDuration -= deltaTime;
            TimeToNextTick -= deltaTime;

            if (TimeToNextTick <= 0f)
            {
                TimeToNextTick += TickInterval;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 刷新效果持续时间（用于效果叠加/刷新）
        /// </summary>
        /// <param name="newDuration">新的持续时间</param>
        public void Refresh(float newDuration)
        {
            RemainingDuration = newDuration;
        }

        /// <summary>
        /// 叠加效果数值（用于可堆叠效果）
        /// </summary>
        /// <param name="additionalValue">额外数值</param>
        /// <param name="maxStacks">最大叠加层数对应的最大值（可选）</param>
        public void Stack(float additionalValue, float? maxValue = null)
        {
            Value += additionalValue;
            if (maxValue.HasValue && Value > maxValue.Value)
            {
                Value = maxValue.Value;
            }
        }
    }
}
