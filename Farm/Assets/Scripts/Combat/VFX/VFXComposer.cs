using UnityEngine;
using FarmGame.Combat.Data;

namespace FarmGame.Combat.VFX
{
    /// <summary>
    /// VFX 组合器 - 根据技能数据自动激活对应的视觉模块
    /// </summary>
    public class VFXComposer : MonoBehaviour
    {
        #region 私有字段

        private AtomVFX[] mAllVFX;
        private bool mIsInitialized;

        #endregion

        #region 生命周期

        private void Awake()
        {
            // 获取所有子物体上的 VFX 组件（包括未激活的）
            mAllVFX = GetComponentsInChildren<AtomVFX>(includeInactive: true);
            mIsInitialized = true;
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 根据技能数据组合 VFX
        /// </summary>
        /// <param name="data">技能原子数据</param>
        public void Compose(SkillAtomData data)
        {
            if (!mIsInitialized || mAllVFX == null) return;

            foreach (var vfx in mAllVFX)
            {
                if (vfx == null) continue;

                bool shouldActivate = vfx.ShouldActivate(data);
                vfx.gameObject.SetActive(shouldActivate);

                if (shouldActivate)
                {
                    vfx.Initialize(data);
                }
            }
        }

        /// <summary>
        /// 重置所有 VFX
        /// </summary>
        public void ResetAll()
        {
            if (mAllVFX == null) return;

            foreach (var vfx in mAllVFX)
            {
                if (vfx == null) continue;

                vfx.Reset();
                vfx.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 激活指定类型的 VFX
        /// </summary>
        /// <typeparam name="T">VFX 类型</typeparam>
        /// <param name="data">技能数据</param>
        public void ActivateVFX<T>(SkillAtomData data) where T : AtomVFX
        {
            if (mAllVFX == null) return;

            foreach (var vfx in mAllVFX)
            {
                if (vfx is T typedVFX)
                {
                    typedVFX.gameObject.SetActive(true);
                    typedVFX.Initialize(data);
                }
            }
        }

        #endregion
    }
}
