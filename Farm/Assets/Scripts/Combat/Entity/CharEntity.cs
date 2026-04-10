using UnityEngine;
using FarmGame.Combat.Data;

namespace FarmGame.Combat.Entity
{
    /// <summary>
    /// 角色实体 - 战斗中的角色单位
    /// 玩家和敌人都使用此组件
    /// </summary>
    public class CharEntity : MonoBehaviour
    {
        #region 序列化字段

        [Header("实体配置")]
        [SerializeField]
        private EntityType mEntityType = EntityType.Enemy;

        [Header("初始属性")]
        [SerializeField]
        private float mInitialMaxHP = 100f;

        [SerializeField]
        private float mInitialMoveSpeed = 5f;

        [SerializeField]
        private float mInitialAttack = 10f;

        [SerializeField]
        private float mInitialDefense = 5f;

        #endregion

        #region 私有字段

        private EntityStats mStats;
        private bool mIsInitialized;
        private Rigidbody2D mRigidbody;
        private Collider2D mCollider;

        #endregion

        #region 公共属性

        /// <summary>实体类型（玩家/敌人）</summary>
        public EntityType EntityType => mEntityType;

        /// <summary>实体属性</summary>
        public EntityStats Stats => mStats;

        /// <summary>是否存活</summary>
        public bool IsAlive => mStats != null && mStats.IsAlive;

        /// <summary>当前位置</summary>
        public Vector3 Position => transform.position;

        /// <summary>Rigidbody2D 组件</summary>
        public Rigidbody2D Rigidbody => mRigidbody;

        #endregion

        #region 生命周期

        private void Awake()
        {
            mRigidbody = GetComponent<Rigidbody2D>();
            mCollider = GetComponent<Collider2D>();
        }

        private void Start()
        {
            if (!mIsInitialized)
            {
                Initialize();
            }
        }

        private void Update()
        {
            if (mStats != null && mStats.IsAlive)
            {
                mStats.Tick(Time.deltaTime);
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 初始化角色实体
        /// </summary>
        /// <param name="entityType">实体类型</param>
        /// <param name="stats">自定义属性（为空则使用默认值）</param>
        public void Initialize(EntityType? entityType = null, EntityStats stats = null)
        {
            if (entityType.HasValue)
            {
                mEntityType = entityType.Value;
            }

            if (stats != null)
            {
                mStats = stats;
            }
            else
            {
                mStats = new EntityStats
                {
                    MaxHP = mInitialMaxHP,
                    CurrentHP = mInitialMaxHP,
                    BaseMoveSpeed = mInitialMoveSpeed,
                    BaseAttack = mInitialAttack,
                    BaseDefense = mInitialDefense
                };
            }

            // 绑定事件
            mStats.OnHPChanged += OnHPChanged;
            mStats.OnDeath += OnDeath;
            mStats.OnEffectTick += OnEffectTick;

            mIsInitialized = true;
        }

        /// <summary>
        /// 受到伤害
        /// </summary>
        /// <param name="baseDamage">基础伤害值</param>
        /// <param name="ignoreDefense">是否忽略防御</param>
        /// <returns>实际造成的伤害</returns>
        public float TakeDamage(float baseDamage, bool ignoreDefense = false)
        {
            if (!IsAlive || baseDamage <= 0) return 0f;

            float damage = baseDamage;

            // 应用防御减伤
            if (!ignoreDefense && mStats.Defense > 0)
            {
                // 简单的防御公式：伤害 = 基础伤害 * (100 / (100 + 防御))
                damage = baseDamage * (100f / (100f + mStats.Defense));
            }

            // 应用易伤/减伤倍率
            damage *= mStats.DamageMultiplier;

            // 确保伤害为正
            damage = Mathf.Max(0f, damage);

            mStats.ApplyHPChange(-damage);

            return damage;
        }

        /// <summary>
        /// 治疗
        /// </summary>
        /// <param name="amount">治疗量</param>
        /// <returns>实际治疗量</returns>
        public float Heal(float amount)
        {
            if (!IsAlive || amount <= 0) return 0f;

            float previousHP = mStats.CurrentHP;
            mStats.ApplyHPChange(amount);

            return mStats.CurrentHP - previousHP;
        }

        /// <summary>
        /// 应用状态效果
        /// </summary>
        /// <param name="effect">状态效果</param>
        /// <param name="stackable">是否可叠加</param>
        public void ApplyStatus(StatusEffect effect, bool stackable = false)
        {
            if (!IsAlive || effect == null) return;

            mStats.ApplyEffect(effect, stackable);
        }

        /// <summary>
        /// 移除状态效果
        /// </summary>
        /// <param name="effectType">效果类型</param>
        public void RemoveStatus(StatusEffectType effectType)
        {
            mStats?.RemoveEffect(effectType);
        }

        /// <summary>
        /// 设置移动方向（由 AI 或玩家输入调用）
        /// </summary>
        /// <param name="direction">移动方向（归一化）</param>
        public void SetMoveDirection(Vector2 direction)
        {
            if (mRigidbody == null || !IsAlive) return;

            if (mStats.IsSilenced)
            {
                // 沉默时可以移动但速度减半
                mRigidbody.velocity = direction.normalized * mStats.MoveSpeed * 0.5f;
            }
            else
            {
                mRigidbody.velocity = direction.normalized * mStats.MoveSpeed;
            }
        }

        /// <summary>
        /// 停止移动
        /// </summary>
        public void StopMoving()
        {
            if (mRigidbody != null)
            {
                mRigidbody.velocity = Vector2.zero;
            }
        }

        /// <summary>
        /// 重置实体状态（用于对象池回收后重用）
        /// </summary>
        public void Reset()
        {
            mStats?.Reset();
            StopMoving();
            gameObject.SetActive(true);
        }

        /// <summary>
        /// 应用力（用于击退/吸引效果）
        /// </summary>
        /// <param name="force">力的方向和大小</param>
        /// <param name="mode">力模式</param>
        public void ApplyForce(Vector2 force, ForceMode2D mode = ForceMode2D.Impulse)
        {
            if (mRigidbody != null && IsAlive)
            {
                mRigidbody.AddForce(force, mode);
            }
        }

        #endregion

        #region 事件处理

        private void OnHPChanged(float change)
        {
            // TODO: 触发 UI 更新、伤害/治疗飘字等
            Debug.Log($"[CharEntity] {gameObject.name} HP 变化: {change:+0.#;-0.#}, 当前 HP: {mStats.CurrentHP:F1}/{mStats.MaxHP}");
        }

        private void OnDeath()
        {
            Debug.Log($"[CharEntity] {gameObject.name} 死亡");
            StopMoving();

            // 禁用碰撞
            if (mCollider != null)
            {
                mCollider.enabled = false;
            }

            // TODO: 播放死亡动画，触发掉落等
            // 暂时直接隐藏
            gameObject.SetActive(false);
        }

        private void OnEffectTick(StatusEffect effect)
        {
            // DoT 伤害在 EntityStats.Tick 中已处理
            // 这里可以添加视觉效果等
        }

        #endregion

        #region Unity 回调

        private void OnDestroy()
        {
            if (mStats != null)
            {
                mStats.OnHPChanged -= OnHPChanged;
                mStats.OnDeath -= OnDeath;
                mStats.OnEffectTick -= OnEffectTick;
            }
        }

        #endregion
    }
}
