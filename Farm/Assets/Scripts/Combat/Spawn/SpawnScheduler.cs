using UnityEngine;
using QFramework;
using FarmGame.Combat.Data;
using FarmGame.Combat.Pool;

namespace FarmGame.Combat.Spawn
{
    /// <summary>
    /// 生成调度器 - 每帧从队列中取出请求并生成实体
    /// 通过三个阀门控制生成速率，防止帧率抖动
    /// </summary>
    public class SpawnScheduler : MonoSingleton<SpawnScheduler>
    {
        #region 阀门配置

        [Header("阀门配置")]
        [Tooltip("每帧生成上限")]
        [SerializeField]
        private int mMaxPerFrame = AtomConstants.MaxSpawnPerFrame;

        [Tooltip("低帧率时降级到的每帧生成上限")]
        [SerializeField]
        private int mAdaptiveFloor = AtomConstants.AdaptiveSpawnFloor;

        [Tooltip("高帧率时提升到的每帧生成上限")]
        [SerializeField]
        private int mAdaptiveCeiling = AtomConstants.AdaptiveSpawnCeiling;

        [Tooltip("FPS 自适应阈值")]
        [SerializeField]
        private float mFPSAdaptiveThreshold = AtomConstants.FPSAdaptiveThreshold;

        #endregion

        #region 私有字段

        private bool mIsInitialized;
        private bool mIsPaused;
        private int mSpawnedThisFrame;

        #endregion

        #region 公共属性

        /// <summary>当前每帧预算</summary>
        public int CurrentBudget { get; private set; }

        /// <summary>本帧已生成数量</summary>
        public int SpawnedThisFrame => mSpawnedThisFrame;

        /// <summary>是否暂停</summary>
        public bool IsPaused => mIsPaused;

        #endregion

        #region 生命周期

        /// <summary>
        /// 初始化调度器
        /// </summary>
        public void Initialize()
        {
            if (mIsInitialized) return;

            mIsInitialized = true;
            mIsPaused = false;
            Debug.Log("[SpawnScheduler] Initialized");
        }

        private void Update()
        {
            if (!mIsInitialized || mIsPaused) return;

            // 处理延迟请求
            SpawnQueue.Instance.ProcessDelayedRequests();

            // 重置计数
            mSpawnedThisFrame = 0;

            // 计算本帧预算
            CalculateBudget();

            // 计算实际可生成数量
            int poolRemaining = CombatEntityPool.Instance != null
                ? AtomConstants.EntityPoolCapacity - CombatEntityPool.Instance.ActiveCount
                : AtomConstants.EntityPoolCapacity;

            int spawnCount = Mathf.Min(
                SpawnQueue.Instance.Count,
                CurrentBudget,
                poolRemaining
            );

            // 生成实体
            for (int i = 0; i < spawnCount; i++)
            {
                if (!SpawnQueue.Instance.TryDequeue(out var request))
                {
                    break;
                }

                SpawnEntity(request);
                mSpawnedThisFrame++;
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 暂停调度
        /// </summary>
        public void Pause()
        {
            mIsPaused = true;
        }

        /// <summary>
        /// 恢复调度
        /// </summary>
        public void Resume()
        {
            mIsPaused = false;
        }

        /// <summary>
        /// 清空队列
        /// </summary>
        public void ClearQueue()
        {
            SpawnQueue.Instance.Clear();
        }

        /// <summary>
        /// 设置每帧上限
        /// </summary>
        /// <param name="max">上限值</param>
        public void SetMaxPerFrame(int max)
        {
            mMaxPerFrame = Mathf.Max(1, max);
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 计算本帧预算
        /// </summary>
        private void CalculateBudget()
        {
            float fps = 1f / Time.deltaTime;

            if (fps > mFPSAdaptiveThreshold)
            {
                // 高帧率，可以多生成
                CurrentBudget = mAdaptiveCeiling;
            }
            else if (fps < mFPSAdaptiveThreshold * 0.5f)
            {
                // 低帧率，降级
                CurrentBudget = mAdaptiveFloor;
            }
            else
            {
                // 正常帧率
                CurrentBudget = mMaxPerFrame;
            }
        }

        /// <summary>
        /// 生成实体
        /// </summary>
        /// <param name="request">生成请求</param>
        private void SpawnEntity(SpawnRequest request)
        {
            if (request.Data == null)
            {
                Debug.LogWarning("[SpawnScheduler] SpawnRequest has null data, skipping");
                return;
            }

            // 从对象池获取实体
            var entity = CombatEntityPool.Instance?.GetSkillEntity();
            if (entity == null)
            {
                Debug.LogWarning("[SpawnScheduler] Failed to get SkillEntity from pool");
                return;
            }

            // 设置位置和旋转
            entity.transform.SetPositionAndRotation(request.Position, request.Rotation);

            // 初始化实体
            entity.Init(request.Data, request.Owner);
        }

        #endregion
    }
}
