using UnityEngine;
using QFramework;
using FarmGame.Core;
using FarmGame.Combat.Entity;
using FarmGame.Combat.Data;

namespace FarmGame.Combat.Pool
{
    /// <summary>
    /// 战斗实体对象池 - 封装 ResourceManager 的战斗实体池管理
    /// </summary>
    public class CombatEntityPool : MonoSingleton<CombatEntityPool>
    {
        #region 配置

        [Header("预制体路径")]
        [SerializeField]
        private string mSkillEntityPrefabPath = "Combat/SkillEntity";

        [SerializeField]
        private string mCharEntityPrefabPath = "Combat/CharEntity";

        [Header("对象池配置")]
        [SerializeField]
        private int mPrewarmCount = 20;

        #endregion

        #region 私有字段

        private bool mIsInitialized;
        private int mActiveSkillEntityCount;
        private int mActiveCharEntityCount;

        // 缓存已激活的实体，用于统计
        private System.Collections.Generic.HashSet<SkillEntity> mActiveSkillEntities;

        #endregion

        #region 公共属性

        /// <summary>当前激活的技能实体数量</summary>
        public int ActiveCount => mActiveSkillEntityCount;

        /// <summary>剩余可用容量</summary>
        public int RemainingCapacity => AtomConstants.EntityPoolCapacity - mActiveSkillEntityCount;

        /// <summary>是否已达到容量上限</summary>
        public bool IsAtCapacity => mActiveSkillEntityCount >= AtomConstants.EntityPoolCapacity;

        #endregion

        #region 生命周期

        /// <summary>
        /// 初始化对象池
        /// </summary>
        public void Initialize()
        {
            if (mIsInitialized) return;

            mActiveSkillEntities = new System.Collections.Generic.HashSet<SkillEntity>();
            mActiveSkillEntityCount = 0;
            mActiveCharEntityCount = 0;

            // 预热对象池
            if (mPrewarmCount > 0)
            {
                PrewarmPool(mPrewarmCount);
            }

            mIsInitialized = true;
            Debug.Log($"[CombatEntityPool] Initialized with prewarm count: {mPrewarmCount}");
        }

        #endregion

        #region 技能实体池接口

        /// <summary>
        /// 从对象池获取技能实体
        /// </summary>
        /// <returns>技能实体，如果达到上限则返回 null</returns>
        public SkillEntity GetSkillEntity()
        {
            if (!mIsInitialized)
            {
                Debug.LogError("[CombatEntityPool] Not initialized");
                return null;
            }

            // 检查容量限制
            if (IsAtCapacity)
            {
                Debug.LogWarning("[CombatEntityPool] At capacity, cannot spawn more entities");
                return null;
            }

            // 从 ResourceManager 获取
            var go = ResourceManager.Instance.Spawn(mSkillEntityPrefabPath, transform);
            if (go == null)
            {
                Debug.LogError($"[CombatEntityPool] Failed to spawn SkillEntity from path: {mSkillEntityPrefabPath}");
                return null;
            }

            var entity = go.GetComponent<SkillEntity>();
            if (entity == null)
            {
                Debug.LogError("[CombatEntityPool] Spawned object has no SkillEntity component");
                ResourceManager.Instance.Despawn(mSkillEntityPrefabPath, go);
                return null;
            }

            mActiveSkillEntities.Add(entity);
            mActiveSkillEntityCount++;

            return entity;
        }

        /// <summary>
        /// 归还技能实体到对象池
        /// </summary>
        /// <param name="entity">技能实体</param>
        public void ReturnSkillEntity(SkillEntity entity)
        {
            if (entity == null) return;

            // 重置状态
            entity.ResetState();
            entity.gameObject.SetActive(false);

            // 从追踪集合移除
            if (mActiveSkillEntities.Remove(entity))
            {
                mActiveSkillEntityCount--;
            }

            // 归还到 ResourceManager
            ResourceManager.Instance.Despawn(mSkillEntityPrefabPath, entity.gameObject);
        }

        /// <summary>
        /// 归还所有激活的技能实体
        /// </summary>
        public void ReturnAllSkillEntities()
        {
            foreach (var entity in mActiveSkillEntities)
            {
                if (entity != null)
                {
                    entity.ResetState();
                    entity.gameObject.SetActive(false);
                    ResourceManager.Instance.Despawn(mSkillEntityPrefabPath, entity.gameObject);
                }
            }

            mActiveSkillEntities.Clear();
            mActiveSkillEntityCount = 0;
        }

        #endregion

        #region 角色实体池接口

        /// <summary>
        /// 从对象池获取角色实体
        /// </summary>
        /// <param name="entityType">实体类型</param>
        /// <returns>角色实体</returns>
        public CharEntity GetCharEntity(EntityType entityType = EntityType.Enemy)
        {
            if (!mIsInitialized)
            {
                Debug.LogError("[CombatEntityPool] Not initialized");
                return null;
            }

            var go = ResourceManager.Instance.Spawn(mCharEntityPrefabPath, transform);
            if (go == null)
            {
                Debug.LogError($"[CombatEntityPool] Failed to spawn CharEntity from path: {mCharEntityPrefabPath}");
                return null;
            }

            var entity = go.GetComponent<CharEntity>();
            if (entity == null)
            {
                Debug.LogError("[CombatEntityPool] Spawned object has no CharEntity component");
                ResourceManager.Instance.Despawn(mCharEntityPrefabPath, go);
                return null;
            }

            entity.Initialize(entityType);
            mActiveCharEntityCount++;

            return entity;
        }

        /// <summary>
        /// 归还角色实体到对象池
        /// </summary>
        /// <param name="entity">角色实体</param>
        public void ReturnCharEntity(CharEntity entity)
        {
            if (entity == null) return;

            entity.Reset();
            entity.gameObject.SetActive(false);
            mActiveCharEntityCount--;

            ResourceManager.Instance.Despawn(mCharEntityPrefabPath, entity.gameObject);
        }

        #endregion

        #region 预热

        /// <summary>
        /// 预热对象池
        /// </summary>
        /// <param name="count">预热数量</param>
        public void PrewarmPool(int count)
        {
            ResourceManager.Instance.PrewarmPool(mSkillEntityPrefabPath, count);
            Debug.Log($"[CombatEntityPool] Prewarmed {count} SkillEntities");
        }

        /// <summary>
        /// 异步预热对象池
        /// </summary>
        /// <param name="count">预热数量</param>
        public async Cysharp.Threading.Tasks.UniTask PrewarmAsync(int count)
        {
            int batchSize = 10;
            for (int i = 0; i < count; i += batchSize)
            {
                int batch = Mathf.Min(batchSize, count - i);
                ResourceManager.Instance.PrewarmPool(mSkillEntityPrefabPath, batch);
                await Cysharp.Threading.Tasks.UniTask.Yield();
            }

            Debug.Log($"[CombatEntityPool] Async prewarmed {count} SkillEntities");
        }

        #endregion

        #region 清理

        /// <summary>
        /// 清理所有战斗实体
        /// </summary>
        public void ClearAll()
        {
            ReturnAllSkillEntities();
            mActiveCharEntityCount = 0;

            ResourceManager.Instance.ClearPool(mSkillEntityPrefabPath);
            ResourceManager.Instance.ClearPool(mCharEntityPrefabPath);

            Debug.Log("[CombatEntityPool] Cleared all entities");
        }

        protected override void OnDestroy()
        {
            ClearAll();
            base.OnDestroy();
        }

        #endregion
    }
}
