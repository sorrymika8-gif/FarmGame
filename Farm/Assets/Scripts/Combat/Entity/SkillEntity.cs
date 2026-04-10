using UnityEngine;
using FarmGame.Combat.Data;
using FarmGame.Combat.Handler;

namespace FarmGame.Combat.Entity
{
    /// <summary>
    /// 技能实体 - 投射物
    /// 挂载在投射物 Prefab 上，每帧按数据驱动行为
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class SkillEntity : MonoBehaviour
    {
        #region 私有字段

        private SkillAtomData mData;
        private CharEntity mOwner;
        private Rigidbody2D mRigidbody;
        private Collider2D mCollider;

        // 运行时状态
        private int mPierceRemaining;
        private int mBounceRemaining;
        private bool mIsReturning;
        private Vector3 mStartPosition;
        private float mTraveledDistance;
        private float mLifetime;

        // 已命中的目标（用于防止重复命中）
        private System.Collections.Generic.HashSet<CharEntity> mHitTargets;

        // 配置
        private const float MAX_LIFETIME = 10f;  // 最大存活时间
        private const float MAX_DISTANCE = 50f;  // 最大飞行距离

        #endregion

        #region 公共属性

        /// <summary>技能原子数据</summary>
        public SkillAtomData Data => mData;

        /// <summary>技能所有者</summary>
        public CharEntity Owner => mOwner;

        /// <summary>剩余穿透次数</summary>
        public int PierceRemaining => mPierceRemaining;

        /// <summary>剩余弹射次数</summary>
        public int BounceRemaining => mBounceRemaining;

        /// <summary>是否正在返回</summary>
        public bool IsReturning => mIsReturning;

        /// <summary>起始位置</summary>
        public Vector3 StartPosition => mStartPosition;

        /// <summary>当前移动方向</summary>
        public Vector3 MoveDirection => transform.right;

        #endregion

        #region 生命周期

        private void Awake()
        {
            mRigidbody = GetComponent<Rigidbody2D>();
            mCollider = GetComponent<Collider2D>();
            mHitTargets = new System.Collections.Generic.HashSet<CharEntity>();

            // 配置 Rigidbody
            mRigidbody.gravityScale = 0f;
            mRigidbody.bodyType = RigidbodyType2D.Kinematic;

            // 确保碰撞器是触发器
            mCollider.isTrigger = true;
        }

        private void Update()
        {
            if (mData == null) return;

            mLifetime += Time.deltaTime;

            // 超时销毁
            if (mLifetime > MAX_LIFETIME)
            {
                ReturnToPool();
                return;
            }

            // 1. 追踪行为
            if (mData.tracking > 0 && !mIsReturning)
            {
                TrackingHandler.Execute(this);
            }

            // 2. 弹道移动
            Vector3 movement = transform.right * mData.projectileSpeed * Time.deltaTime;
            transform.position += movement;
            mTraveledDistance += movement.magnitude;

            // 3. 吸引/排斥
            if (mData.attract != 0)
            {
                AttractHandler.Execute(this);
            }

            // 4. 返回检测
            if (mData.returning)
            {
                ReturnHandler.Execute(this);
            }

            // 5. 距离检测
            if (mTraveledDistance > MAX_DISTANCE)
            {
                ReturnToPool();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var target = other.GetComponent<CharEntity>();
            if (target == null) return;

            // 不攻击友方
            if (mOwner != null && target.EntityType == mOwner.EntityType) return;

            // 检查是否已命中过（穿透时防止重复伤害）
            if (mHitTargets.Contains(target)) return;
            mHitTargets.Add(target);

            // 应用效果
            EffectApplier.Apply(mData, target, mOwner);

            // AOE 效果
            if (mData.aoeRadius > 0)
            {
                AOEHandler.Execute(this, transform.position);
            }

            // 弹射
            if (mBounceRemaining > 0)
            {
                if (BounceHandler.Execute(this, target))
                {
                    mBounceRemaining--;
                    return;  // 弹射成功，不继续其他判断
                }
            }

            // 分裂
            if (mData.split > 0)
            {
                SplitHandler.Execute(this);
            }

            // 穿透检查
            mPierceRemaining--;
            if (mPierceRemaining < 0)
            {
                ReturnToPool();
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 初始化技能实体
        /// </summary>
        /// <param name="data">技能原子数据</param>
        /// <param name="owner">所有者（可选）</param>
        public void Init(SkillAtomData data, CharEntity owner = null)
        {
            mData = data;
            mOwner = owner;

            // 初始化运行时状态
            mPierceRemaining = data.pierce;
            mBounceRemaining = data.bounce;
            mIsReturning = false;
            mStartPosition = transform.position;
            mTraveledDistance = 0f;
            mLifetime = 0f;
            mHitTargets.Clear();

            // 设置速度默认值
            if (mData.projectileSpeed <= 0)
            {
                mData.projectileSpeed = AtomConstants.DefaultProjectileSpeed;
            }

            // TODO: 初始化 VFX
            // GetComponent<VFXComposer>()?.Compose(data);

            gameObject.SetActive(true);
        }

        /// <summary>
        /// 设置返回状态
        /// </summary>
        public void SetReturning(bool returning)
        {
            mIsReturning = returning;

            if (returning)
            {
                // 返回时清除已命中列表，允许再次命中
                mHitTargets.Clear();
            }
        }

        /// <summary>
        /// 设置移动方向
        /// </summary>
        /// <param name="direction">方向向量</param>
        public void SetDirection(Vector2 direction)
        {
            if (direction.sqrMagnitude > 0.001f)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        /// <summary>
        /// 旋转方向
        /// </summary>
        /// <param name="angleDelta">角度变化量</param>
        public void RotateDirection(float angleDelta)
        {
            transform.Rotate(0, 0, angleDelta);
        }

        /// <summary>
        /// 获取所有者位置
        /// </summary>
        /// <returns>所有者位置，如果所有者不存在则返回起始位置</returns>
        public Vector3 GetOwnerPosition()
        {
            if (mOwner != null && mOwner.IsAlive)
            {
                return mOwner.Position;
            }
            return mStartPosition;
        }

        /// <summary>
        /// 消耗一次弹射计数
        /// </summary>
        public void ConsumeBounce()
        {
            mBounceRemaining--;
        }

        /// <summary>
        /// 返回对象池
        /// </summary>
        public void ReturnToPool()
        {
            gameObject.SetActive(false);

            // TODO: 通过 CombatEntityPool 回收
            // CombatEntityPool.Instance.ReturnSkillEntity(this);

            // 临时处理：直接销毁
            // Destroy(gameObject);
        }

        /// <summary>
        /// 重置实体状态（对象池回收后重用）
        /// </summary>
        public void ResetState()
        {
            mData = null;
            mOwner = null;
            mPierceRemaining = 0;
            mBounceRemaining = 0;
            mIsReturning = false;
            mTraveledDistance = 0f;
            mLifetime = 0f;
            mHitTargets.Clear();
            transform.rotation = Quaternion.identity;
        }

        #endregion
    }
}
