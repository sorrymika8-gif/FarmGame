using UnityEngine;
using FarmGame.Combat.Data;

namespace FarmGame.Combat.VFX
{
    /// <summary>
    /// 基础原子 VFX - 包含通用视觉效果
    /// 如 TrailRenderer、基础粒子等
    /// </summary>
    [RequireComponent(typeof(TrailRenderer))]
    public class BasicAtomVFX : AtomVFX
    {
        #region 序列化字段

        [Header("Trail 配置")]
        [SerializeField]
        private Gradient mDefaultTrailColor;

        [SerializeField]
        private float mTrailTime = 0.3f;

        [SerializeField]
        private float mTrailStartWidth = 0.3f;

        [SerializeField]
        private float mTrailEndWidth = 0.05f;

        [Header("粒子配置")]
        [SerializeField]
        private ParticleSystem mHitParticle;

        [SerializeField]
        private ParticleSystem mTrailParticle;

        #endregion

        #region 私有字段

        private TrailRenderer mTrailRenderer;
        private SkillAtomData mCurrentData;

        #endregion

        #region 生命周期

        private void Awake()
        {
            mTrailRenderer = GetComponent<TrailRenderer>();
            SetupDefaultTrail();
        }

        #endregion

        #region AtomVFX 实现

        public override bool ShouldActivate(SkillAtomData data)
        {
            // 基础 VFX 总是激活
            return true;
        }

        public override void Initialize(SkillAtomData data)
        {
            mCurrentData = data;
            ConfigureTrail(data);
            ConfigureParticles(data);
        }

        public override void Reset()
        {
            if (mTrailRenderer != null)
            {
                mTrailRenderer.Clear();
            }

            if (mHitParticle != null)
            {
                mHitParticle.Stop();
                mHitParticle.Clear();
            }

            if (mTrailParticle != null)
            {
                mTrailParticle.Stop();
                mTrailParticle.Clear();
            }

            mCurrentData = null;
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 播放命中特效
        /// </summary>
        public void PlayHitEffect()
        {
            if (mHitParticle != null)
            {
                mHitParticle.Play();
            }
        }

        /// <summary>
        /// 播放命中特效（在指定位置）
        /// </summary>
        /// <param name="position">位置</param>
        public void PlayHitEffectAt(Vector3 position)
        {
            if (mHitParticle != null)
            {
                mHitParticle.transform.position = position;
                mHitParticle.Play();
            }
        }

        #endregion

        #region 私有方法

        private void SetupDefaultTrail()
        {
            if (mTrailRenderer == null) return;

            mTrailRenderer.time = mTrailTime;
            mTrailRenderer.startWidth = mTrailStartWidth;
            mTrailRenderer.endWidth = mTrailEndWidth;

            if (mDefaultTrailColor != null)
            {
                mTrailRenderer.colorGradient = mDefaultTrailColor;
            }
        }

        private void ConfigureTrail(SkillAtomData data)
        {
            if (mTrailRenderer == null || data == null) return;

            // 根据技能属性调整轨迹
            float speedFactor = data.projectileSpeed / AtomConstants.DefaultProjectileSpeed;

            // 速度越快，轨迹越长
            mTrailRenderer.time = mTrailTime * (1f + (speedFactor - 1f) * 0.5f);

            // 根据伤害调整颜色强度（可选）
            if (data.directHP < 0)
            {
                // 伤害技能 - 可以调整为更红的颜色
                // 这里保持默认，实际项目可以根据需求调整
            }
        }

        private void ConfigureParticles(SkillAtomData data)
        {
            if (data == null) return;

            // 配置轨迹粒子
            if (mTrailParticle != null)
            {
                var emission = mTrailParticle.emission;

                // 追踪技能发射更多粒子
                if (data.tracking > 0)
                {
                    emission.rateOverTime = 15f;
                }
                else
                {
                    emission.rateOverTime = 5f;
                }

                mTrailParticle.Play();
            }

            // 配置命中粒子
            if (mHitParticle != null)
            {
                var main = mHitParticle.main;

                // AOE 技能命中粒子更大
                if (data.aoeRadius > 0)
                {
                    main.startSize = 1f + data.aoeRadius * 0.2f;
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// 追踪 VFX - 追踪技能的视觉效果
    /// </summary>
    public class TrackingVFX : AtomVFX
    {
        [SerializeField]
        private ParticleSystem mTrackingParticle;

        public override bool ShouldActivate(SkillAtomData data)
        {
            return data != null && data.tracking > 0;
        }

        public override void Initialize(SkillAtomData data)
        {
            if (mTrackingParticle != null)
            {
                var emission = mTrackingParticle.emission;
                // 追踪角度越大，粒子越多
                emission.rateOverTime = 5f + data.tracking / 36f;
                mTrackingParticle.Play();
            }
        }

        public override void Reset()
        {
            if (mTrackingParticle != null)
            {
                mTrackingParticle.Stop();
                mTrackingParticle.Clear();
            }
        }
    }

    /// <summary>
    /// AOE VFX - 范围效果的视觉
    /// </summary>
    public class AOEVFX : AtomVFX
    {
        [SerializeField]
        private ParticleSystem mAOEParticle;

        [SerializeField]
        private SpriteRenderer mAOEIndicator;

        public override bool ShouldActivate(SkillAtomData data)
        {
            return data != null && data.aoeRadius > 0;
        }

        public override void Initialize(SkillAtomData data)
        {
            // 根据 AOE 半径调整粒子范围
            if (mAOEParticle != null)
            {
                var shape = mAOEParticle.shape;
                shape.radius = data.aoeRadius;
            }

            // 调整指示器大小
            if (mAOEIndicator != null)
            {
                float scale = data.aoeRadius * 2f;
                mAOEIndicator.transform.localScale = new Vector3(scale, scale, 1f);
            }
        }

        public override void Reset()
        {
            if (mAOEParticle != null)
            {
                mAOEParticle.Stop();
                mAOEParticle.Clear();
            }

            if (mAOEIndicator != null)
            {
                mAOEIndicator.transform.localScale = Vector3.one;
            }
        }

        /// <summary>
        /// 播放 AOE 爆发效果
        /// </summary>
        public void PlayExplosion()
        {
            if (mAOEParticle != null)
            {
                mAOEParticle.Play();
            }
        }
    }
}
