using UnityEngine;
using FarmGame.Combat.Core;
using FarmGame.Combat.Data;
using FarmGame.Combat.Entity;
using FarmGame.Combat.Handler;
using FarmGame.Combat.Spawn;

namespace FarmGame.Combat.AI
{
    /// <summary>
    /// 敌人AI控制器 - 基于状态机的简单AI行为
    /// </summary>
    [RequireComponent(typeof(CharEntity))]
    public class EnemyAI : MonoBehaviour
    {
        #region AI状态枚举

        /// <summary>AI行为状态</summary>
        public enum AIState
        {
            /// <summary>空闲 - 无目标时徘徊</summary>
            Idle,
            /// <summary>追击 - 向目标移动</summary>
            Chase,
            /// <summary>攻击 - 在攻击范围内释放技能</summary>
            Attack,
            /// <summary>逃跑 - 血量过低时远离目标</summary>
            Flee,
            /// <summary>死亡 - 停止所有行为</summary>
            Dead
        }

        #endregion

        #region 序列化字段

        [Header("AI配置")]
        [SerializeField]
        private float mDetectionRange = CombatConfig.ENEMY_DETECTION_RANGE;

        [SerializeField]
        private float mAttackRange = CombatConfig.ENEMY_ATTACK_RANGE;

        [SerializeField]
        private float mAttackCooldown = CombatConfig.ENEMY_ATTACK_COOLDOWN;

        [SerializeField]
        private float mFleeHPThreshold = CombatConfig.ENEMY_FLEE_HP_THRESHOLD;

        [Header("技能配置")]
        [SerializeField]
        private SkillAtomData mDefaultSkill;

        [Header("行为配置")]
        [SerializeField]
        private float mIdleWanderRadius = 3f;

        [SerializeField]
        private float mIdleWanderInterval = 2f;

        #endregion

        #region 私有字段

        private CharEntity mCharEntity;
        private AIState mCurrentState = AIState.Idle;
        private CharEntity mTarget;
        private float mLastAttackTime;
        private float mLastStateUpdateTime;
        private float mLastWanderTime;
        private Vector3 mWanderTarget;
        private Vector3 mSpawnPosition;

        #endregion

        #region 公共属性

        /// <summary>当前AI状态</summary>
        public AIState CurrentState => mCurrentState;

        /// <summary>当前目标</summary>
        public CharEntity Target => mTarget;

        /// <summary>关联的CharEntity</summary>
        public CharEntity CharEntity => mCharEntity;

        #endregion

        #region 生命周期

        private void Awake()
        {
            mCharEntity = GetComponent<CharEntity>();
        }

        private void Start()
        {
            mSpawnPosition = transform.position;
            mWanderTarget = mSpawnPosition;

            // 初始化默认技能（如果未配置）
            if (mDefaultSkill == null)
            {
                mDefaultSkill = CreateDefaultSkill();
            }
        }

        private void Update()
        {
            if (mCharEntity == null || !mCharEntity.IsAlive)
            {
                SetState(AIState.Dead);
                return;
            }

            // AI更新节流
            if (Time.time - mLastStateUpdateTime < CombatConfig.ENEMY_AI_UPDATE_INTERVAL)
            {
                return;
            }
            mLastStateUpdateTime = Time.time;

            // 状态机更新
            UpdateStateMachine();
        }

        #endregion

        #region 状态机

        private void UpdateStateMachine()
        {
            // 查找目标
            UpdateTarget();

            // 检查状态转换
            CheckStateTransition();

            // 执行当前状态行为
            ExecuteCurrentState();
        }

        private void UpdateTarget()
        {
            // 使用 TrackingHandler 的目标查找逻辑
            mTarget = TrackingHandler.FindNearestInRange(
                transform.position,
                mDetectionRange,
                EntityType.Player
            );
        }

        private void CheckStateTransition()
        {
            // 死亡检查
            if (!mCharEntity.IsAlive)
            {
                SetState(AIState.Dead);
                return;
            }

            // 血量过低 -> 逃跑
            if (mCharEntity.Stats.HPPercent < mFleeHPThreshold && mTarget != null)
            {
                SetState(AIState.Flee);
                return;
            }

            // 无目标 -> 空闲
            if (mTarget == null || !mTarget.IsAlive)
            {
                SetState(AIState.Idle);
                return;
            }

            // 计算与目标的距离
            float distanceToTarget = Vector3.Distance(transform.position, mTarget.Position);

            // 在攻击范围内 -> 攻击
            if (distanceToTarget <= mAttackRange)
            {
                SetState(AIState.Attack);
                return;
            }

            // 在检测范围内但不在攻击范围 -> 追击
            if (distanceToTarget <= mDetectionRange)
            {
                SetState(AIState.Chase);
                return;
            }

            // 超出检测范围 -> 空闲
            SetState(AIState.Idle);
        }

        private void ExecuteCurrentState()
        {
            switch (mCurrentState)
            {
                case AIState.Idle:
                    ExecuteIdle();
                    break;
                case AIState.Chase:
                    ExecuteChase();
                    break;
                case AIState.Attack:
                    ExecuteAttack();
                    break;
                case AIState.Flee:
                    ExecuteFlee();
                    break;
                case AIState.Dead:
                    ExecuteDead();
                    break;
            }
        }

        private void SetState(AIState newState)
        {
            if (mCurrentState == newState) return;

            // 退出旧状态
            OnExitState(mCurrentState);

            var oldState = mCurrentState;
            mCurrentState = newState;

            // 进入新状态
            OnEnterState(newState);

            Debug.Log($"[EnemyAI] {gameObject.name} 状态切换: {oldState} -> {newState}");
        }

        private void OnEnterState(AIState state)
        {
            switch (state)
            {
                case AIState.Idle:
                    mWanderTarget = mSpawnPosition;
                    break;
                case AIState.Dead:
                    mCharEntity.StopMoving();
                    break;
            }
        }

        private void OnExitState(AIState state)
        {
            switch (state)
            {
                case AIState.Chase:
                case AIState.Flee:
                    // 离开移动状态时减速
                    break;
            }
        }

        #endregion

        #region 状态行为

        private void ExecuteIdle()
        {
            // 空闲时在出生点附近徘徊
            if (Time.time - mLastWanderTime > mIdleWanderInterval)
            {
                mLastWanderTime = Time.time;
                Vector2 randomOffset = Random.insideUnitCircle * mIdleWanderRadius;
                mWanderTarget = mSpawnPosition + new Vector3(randomOffset.x, randomOffset.y, 0f);
            }

            // 向徘徊目标移动
            Vector3 toTarget = mWanderTarget - transform.position;
            if (toTarget.sqrMagnitude > 0.25f)
            {
                mCharEntity.SetMoveDirection(toTarget.normalized);
            }
            else
            {
                mCharEntity.StopMoving();
            }
        }

        private void ExecuteChase()
        {
            if (mTarget == null) return;

            // 向目标移动
            Vector3 toTarget = mTarget.Position - transform.position;
            mCharEntity.SetMoveDirection(toTarget.normalized);
        }

        private void ExecuteAttack()
        {
            if (mTarget == null) return;

            // 停止移动或减速
            mCharEntity.StopMoving();

            // 检查攻击冷却
            if (Time.time - mLastAttackTime < mAttackCooldown) return;

            // 检查沉默
            if (mCharEntity.Stats.IsSilenced) return;

            // 执行攻击
            TryAttack();
        }

        private void ExecuteFlee()
        {
            if (mTarget == null)
            {
                SetState(AIState.Idle);
                return;
            }

            // 远离目标
            Vector3 awayFromTarget = transform.position - mTarget.Position;
            mCharEntity.SetMoveDirection(awayFromTarget.normalized);
        }

        private void ExecuteDead()
        {
            // 死亡状态不做任何事
            mCharEntity.StopMoving();
        }

        #endregion

        #region 攻击逻辑

        private void TryAttack()
        {
            if (mDefaultSkill == null || mTarget == null) return;

            // 计算攻击方向
            Vector3 toTarget = mTarget.Position - transform.position;
            Vector3 direction = toTarget.normalized;

            // 计算旋转（2D中使用Z轴旋转）
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

            // 创建技能生成请求
            var request = new SpawnRequest(
                mDefaultSkill.Clone(),
                transform.position + direction * 0.5f,
                rotation,
                0f,
                mCharEntity
            );

            // 入队生成
            SpawnQueue.Instance.Enqueue(request);

            mLastAttackTime = Time.time;

            Debug.Log($"[EnemyAI] {gameObject.name} 发动攻击");
        }

        /// <summary>
        /// 创建默认技能数据
        /// </summary>
        private SkillAtomData CreateDefaultSkill()
        {
            return new SkillAtomData
            {
                displayName = "敌人基础攻击",
                directHP = -CombatConfig.DEFAULT_ENEMY_ATTACK,
                projectileSpeed = 8f,
                pierce = 0,
                bounce = 0,
                split = 0,
                tracking = 0f,
                aoeRadius = 0f,
                shape = ShapeType.Point,
                target = TargetType.SingleEnemy,
                trigger = TriggerType.Immediate
            };
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 设置AI使用的技能
        /// </summary>
        /// <param name="skill">技能数据</param>
        public void SetSkill(SkillAtomData skill)
        {
            if (skill != null)
            {
                mDefaultSkill = skill;
            }
        }

        /// <summary>
        /// 强制切换到指定状态
        /// </summary>
        /// <param name="state">目标状态</param>
        public void ForceState(AIState state)
        {
            SetState(state);
        }

        /// <summary>
        /// 设置逃跑血量阈值
        /// </summary>
        /// <param name="threshold">阈值（0-1）</param>
        public void SetFleeThreshold(float threshold)
        {
            mFleeHPThreshold = Mathf.Clamp01(threshold);
        }

        /// <summary>
        /// 设置攻击范围
        /// </summary>
        /// <param name="range">攻击范围</param>
        public void SetAttackRange(float range)
        {
            mAttackRange = Mathf.Max(0f, range);
        }

        /// <summary>
        /// 设置检测范围
        /// </summary>
        /// <param name="range">检测范围</param>
        public void SetDetectionRange(float range)
        {
            mDetectionRange = Mathf.Max(0f, range);
        }

        /// <summary>
        /// 重置AI状态
        /// </summary>
        public void Reset()
        {
            mCurrentState = AIState.Idle;
            mTarget = null;
            mLastAttackTime = 0f;
            mLastStateUpdateTime = 0f;
            mLastWanderTime = 0f;
            mSpawnPosition = transform.position;
            mWanderTarget = mSpawnPosition;
        }

        #endregion

        #region 编辑器辅助

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // 绘制检测范围
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, mDetectionRange);

            // 绘制攻击范围
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, mAttackRange);

            // 绘制徘徊范围
            if (Application.isPlaying)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(mSpawnPosition, mIdleWanderRadius);
            }

            // 绘制到目标的线
            if (mTarget != null)
            {
                Gizmos.color = mCurrentState == AIState.Flee ? Color.blue : Color.red;
                Gizmos.DrawLine(transform.position, mTarget.Position);
            }
        }
#endif

        #endregion
    }
}
