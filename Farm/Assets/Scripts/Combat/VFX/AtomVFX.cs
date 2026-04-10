using UnityEngine;
using FarmGame.Combat.Data;

namespace FarmGame.Combat.VFX
{
    /// <summary>
    /// 原子 VFX 基类 - 根据技能数据决定是否激活
    /// </summary>
    public abstract class AtomVFX : MonoBehaviour
    {
        /// <summary>
        /// 判断此 VFX 模块是否应该激活
        /// </summary>
        /// <param name="data">技能原子数据</param>
        /// <returns>是否激活</returns>
        public abstract bool ShouldActivate(SkillAtomData data);

        /// <summary>
        /// 初始化 VFX（在激活时调用）
        /// </summary>
        /// <param name="data">技能原子数据</param>
        public virtual void Initialize(SkillAtomData data)
        {
            // 子类可重写以进行特定初始化
        }

        /// <summary>
        /// 重置 VFX（在回收时调用）
        /// </summary>
        public virtual void Reset()
        {
            // 子类可重写以进行重置
        }
    }
}
