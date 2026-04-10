using System;
using UnityEngine;
using QFramework;
using Cysharp.Threading.Tasks;
using FarmGame.Combat.Data;
using FarmGame.Combat.Entity;
using FarmGame.Combat.Pool;
using FarmGame.Combat.Spawn;
using FarmGame.Combat.LLM;

namespace FarmGame.Combat.Core
{
    /// <summary>
    /// 战斗状态
    /// </summary>
    public enum CombatState
    {
        /// <summary>空闲（无战斗）</summary>
        Idle,
        /// <summary>准备中（加载资源、初始化）</summary>
        Preparing,
        /// <summary>倒计时</summary>
        Countdown,
        /// <summary>战斗中</summary>
        Fighting,
        /// <summary>暂停</summary>
        Paused,
        /// <summary>战斗结束</summary>
        Ended
    }

    /// <summary>
    /// 战斗结果
    /// </summary>
    public enum CombatResult
    {
        /// <summary>未决定</summary>
        None,
        /// <summary>玩家胜利</summary>
        Victory,
        /// <summary>玩家失败</summary>
        Defeat,
        /// <summary>平局/超时</summary>
        Draw
    }

    /// <summary>
    /// 战斗管理器 - 战斗循环、状态机、全局调度
    /// </summary>
    public class CombatManager : MonoSingleton<CombatManager>
    {
        #region 事件

        /// <summary>战斗状态变化事件</summary>
        public event Action<CombatState> OnStateChanged;

        /// <summary>战斗结束事件</summary>
        public event Action<CombatResult> OnCombatEnded;

        /// <summary>敌人死亡事件</summary>
        public event Action<CharEntity> OnEnemyDied;

        /// <summary>玩家受伤事件</summary>
        public event Action<float> OnPlayerDamaged;

        #endregion

        #region 私有字段

        private bool mIsInitialized;
        private CombatState mCurrentState = CombatState.Idle;
        private CombatResult mCombatResult = CombatResult.None;
        private CharEntity mPlayerEntity;
        private System.Collections.Generic.List<CharEntity> mEnemies;
        private float mCombatTimer;

        #endregion

        #region 公共属性

        /// <summary>当前战斗状态</summary>
        public CombatState CurrentState => mCurrentState;

        /// <summary>战斗结果</summary>
        public CombatResult Result => mCombatResult;

        /// <summary>玩家实体</summary>
        public CharEntity PlayerEntity => mPlayerEntity;

        /// <summary>敌人列表</summary>
        public System.Collections.Generic.IReadOnlyList<CharEntity> Enemies => mEnemies;

        /// <summary>战斗计时器</summary>
        public float CombatTimer => mCombatTimer;

        /// <summary>是否正在战斗</summary>
        public bool IsInCombat => mCurrentState == CombatState.Fighting;

        #endregion

        #region 生命周期

        /// <summary>
        /// 初始化战斗管理器
        /// </summary>
        public void Initialize()
        {
            if (mIsInitialized) return;

            mEnemies = new System.Collections.Generic.List<CharEntity>();

            // 初始化子系统
            CombatEntityPool.Instance.Initialize();
            SpawnScheduler.Instance.Initialize();
            LLMBridge.Instance.Initialize();

            mIsInitialized = true;
            Debug.Log("[CombatManager] Initialized");
        }

        private void Update()
        {
            if (!mIsInitialized) return;

            switch (mCurrentState)
            {
                case CombatState.Fighting:
                    UpdateCombat();
                    break;
            }
        }

        #endregion

        #region 公共方法 - 战斗控制

        /// <summary>
        /// 开始战斗
        /// </summary>
        /// <param name="enemyCount">敌人数量</param>
        public async UniTask StartCombatAsync(int enemyCount = 3)
        {
            if (mCurrentState != CombatState.Idle)
            {
                Debug.LogWarning("[CombatManager] Cannot start combat, current state: " + mCurrentState);
                return;
            }

            SetState(CombatState.Preparing);

            try
            {
                // 准备战斗
                await PrepareCombatAsync(enemyCount);

                // 倒计时
                SetState(CombatState.Countdown);
                await CountdownAsync();

                // 开始战斗
                SetState(CombatState.Fighting);
                mCombatTimer = 0f;
                mCombatResult = CombatResult.None;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CombatManager] Failed to start combat: {ex.Message}");
                SetState(CombatState.Idle);
            }
        }

        /// <summary>
        /// 结束战斗
        /// </summary>
        /// <param name="result">战斗结果</param>
        public void EndCombat(CombatResult result)
        {
            if (mCurrentState == CombatState.Idle || mCurrentState == CombatState.Ended)
            {
                return;
            }

            mCombatResult = result;
            SetState(CombatState.Ended);

            // 暂停生成
            SpawnScheduler.Instance.Pause();
            SpawnQueue.Instance.Clear();

            OnCombatEnded?.Invoke(result);

            Debug.Log($"[CombatManager] Combat ended with result: {result}");

            // 延迟清理
            CleanupAfterDelay().Forget();
        }

        /// <summary>
        /// 暂停战斗
        /// </summary>
        public void PauseCombat()
        {
            if (mCurrentState != CombatState.Fighting) return;

            SetState(CombatState.Paused);
            SpawnScheduler.Instance.Pause();
            Time.timeScale = 0f;
        }

        /// <summary>
        /// 恢复战斗
        /// </summary>
        public void ResumeCombat()
        {
            if (mCurrentState != CombatState.Paused) return;

            SetState(CombatState.Fighting);
            SpawnScheduler.Instance.Resume();
            Time.timeScale = 1f;
        }

        /// <summary>
        /// 释放技能
        /// </summary>
        /// <param name="data">技能数据</param>
        /// <param name="position">起始位置</param>
        /// <param name="direction">方向</param>
        public void CastSkill(SkillAtomData data, Vector3 position, Vector2 direction)
        {
            if (!IsInCombat || data == null) return;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            var rotation = Quaternion.Euler(0, 0, angle);

            var request = new SpawnRequest(
                data,
                position,
                rotation,
                data.delay,
                mPlayerEntity
            );

            SpawnQueue.Instance.Enqueue(request);
        }

        #endregion

        #region 私有方法 - 战斗流程

        private async UniTask PrepareCombatAsync(int enemyCount)
        {
            // 预热对象池
            await CombatEntityPool.Instance.PrewarmAsync(20);

            // 创建玩家实体
            mPlayerEntity = CreatePlayerEntity();

            // 创建敌人
            for (int i = 0; i < enemyCount; i++)
            {
                var enemy = CreateEnemyEntity(i, enemyCount);
                mEnemies.Add(enemy);
            }

            Debug.Log($"[CombatManager] Combat prepared with {enemyCount} enemies");
        }

        private async UniTask CountdownAsync()
        {
            float countdown = CombatConfig.BATTLE_START_COUNTDOWN;
            while (countdown > 0)
            {
                // TODO: 更新 UI 倒计时显示
                Debug.Log($"[CombatManager] Countdown: {Mathf.CeilToInt(countdown)}");
                await UniTask.Delay(1000);
                countdown -= 1f;
            }
        }

        private void UpdateCombat()
        {
            mCombatTimer += Time.deltaTime;

            // 检查胜负条件
            CheckCombatConditions();
        }

        private void CheckCombatConditions()
        {
            // 检查玩家是否死亡
            if (mPlayerEntity != null && !mPlayerEntity.IsAlive)
            {
                EndCombat(CombatResult.Defeat);
                return;
            }

            // 检查所有敌人是否死亡
            bool allEnemiesDead = true;
            foreach (var enemy in mEnemies)
            {
                if (enemy != null && enemy.IsAlive)
                {
                    allEnemiesDead = false;
                    break;
                }
            }

            if (allEnemiesDead && mEnemies.Count > 0)
            {
                EndCombat(CombatResult.Victory);
            }
        }

        private async UniTaskVoid CleanupAfterDelay()
        {
            await UniTask.Delay((int)(CombatConfig.BATTLE_END_DELAY * 1000));

            Cleanup();
            SetState(CombatState.Idle);
        }

        private void Cleanup()
        {
            // 回收所有实体
            CombatEntityPool.Instance.ReturnAllSkillEntities();

            if (mPlayerEntity != null)
            {
                CombatEntityPool.Instance.ReturnCharEntity(mPlayerEntity);
                mPlayerEntity = null;
            }

            foreach (var enemy in mEnemies)
            {
                if (enemy != null)
                {
                    CombatEntityPool.Instance.ReturnCharEntity(enemy);
                }
            }
            mEnemies.Clear();

            // 清空生成队列
            SpawnQueue.Instance.Clear();

            Debug.Log("[CombatManager] Combat cleanup completed");
        }

        #endregion

        #region 私有方法 - 实体创建

        private CharEntity CreatePlayerEntity()
        {
            var entity = CombatEntityPool.Instance.GetCharEntity(EntityType.Player);
            if (entity == null) return null;

            // 设置属性
            var stats = new EntityStats
            {
                MaxHP = CombatConfig.DEFAULT_PLAYER_HP,
                CurrentHP = CombatConfig.DEFAULT_PLAYER_HP,
                BaseAttack = CombatConfig.DEFAULT_PLAYER_ATTACK,
                BaseDefense = CombatConfig.DEFAULT_PLAYER_DEFENSE,
                BaseMoveSpeed = CombatConfig.DEFAULT_PLAYER_MOVE_SPEED
            };

            entity.Initialize(EntityType.Player, stats);

            // 设置位置
            entity.transform.position = new Vector3(
                CombatConfig.PLAYER_SPAWN_X,
                0f,
                0f
            );

            // 绑定事件
            stats.OnHPChanged += (change) =>
            {
                if (change < 0) OnPlayerDamaged?.Invoke(-change);
            };

            return entity;
        }

        private CharEntity CreateEnemyEntity(int index, int total)
        {
            var entity = CombatEntityPool.Instance.GetCharEntity(EntityType.Enemy);
            if (entity == null) return null;

            var stats = new EntityStats
            {
                MaxHP = CombatConfig.DEFAULT_ENEMY_HP,
                CurrentHP = CombatConfig.DEFAULT_ENEMY_HP,
                BaseAttack = CombatConfig.DEFAULT_ENEMY_ATTACK,
                BaseDefense = CombatConfig.DEFAULT_ENEMY_DEFENSE,
                BaseMoveSpeed = CombatConfig.DEFAULT_ENEMY_MOVE_SPEED
            };

            entity.Initialize(EntityType.Enemy, stats);

            // 分散敌人位置
            float ySpread = CombatConfig.BATTLE_AREA_HEIGHT * 0.6f;
            float yOffset = total > 1
                ? ((float)index / (total - 1) - 0.5f) * ySpread
                : 0f;

            entity.transform.position = new Vector3(
                CombatConfig.ENEMY_SPAWN_X,
                yOffset,
                0f
            );

            // 绑定死亡事件
            stats.OnDeath += () => OnEnemyDied?.Invoke(entity);

            return entity;
        }

        #endregion

        #region 私有方法 - 状态管理

        private void SetState(CombatState newState)
        {
            if (mCurrentState == newState) return;

            var oldState = mCurrentState;
            mCurrentState = newState;

            Debug.Log($"[CombatManager] State changed: {oldState} -> {newState}");
            OnStateChanged?.Invoke(newState);
        }

        #endregion
    }
}
