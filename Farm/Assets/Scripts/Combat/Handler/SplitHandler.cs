using UnityEngine;
using FarmGame.Combat.Entity;
using FarmGame.Combat.Data;
using FarmGame.Combat.Spawn;

namespace FarmGame.Combat.Handler
{
    /// <summary>
    /// 分裂处理器 - 命中时分裂出多个子投射物
    /// </summary>
    public static class SplitHandler
    {
        /// <summary>分裂角度间隔</summary>
        private const float SPLIT_ANGLE_SPREAD = 15f;

        /// <summary>
        /// 执行分裂逻辑
        /// </summary>
        /// <param name="entity">技能实体</param>
        public static void Execute(SkillEntity entity)
        {
            if (entity == null || entity.Data == null) return;
            if (entity.Data.split <= 0) return;

            int splitCount = entity.Data.split;

            for (int i = 0; i < splitCount; i++)
            {
                // 创建子弹数据的副本
                var newData = entity.Data.Clone();
                newData.split = 0;  // 子弹不再分裂（防止同帧无限分裂）

                // 计算分裂角度偏移
                float angleOffset = (i - (splitCount - 1) / 2f) * SPLIT_ANGLE_SPREAD;
                Quaternion offsetRotation = Quaternion.Euler(0, 0, angleOffset);
                Quaternion newRotation = entity.transform.rotation * offsetRotation;

                // 创建生成请求入队
                var request = new SpawnRequest(
                    newData,
                    entity.transform.position,
                    newRotation,
                    0f,  // 立即生成
                    entity.Owner
                );

                SpawnQueue.Instance.Enqueue(request);
            }
        }

        /// <summary>
        /// 执行延迟分裂（指定延迟时间）
        /// </summary>
        /// <param name="entity">技能实体</param>
        /// <param name="delay">延迟秒数</param>
        public static void ExecuteDelayed(SkillEntity entity, float delay)
        {
            if (entity == null || entity.Data == null) return;
            if (entity.Data.split <= 0) return;

            int splitCount = entity.Data.split;

            for (int i = 0; i < splitCount; i++)
            {
                var newData = entity.Data.Clone();
                newData.split = 0;

                float angleOffset = (i - (splitCount - 1) / 2f) * SPLIT_ANGLE_SPREAD;
                Quaternion offsetRotation = Quaternion.Euler(0, 0, angleOffset);
                Quaternion newRotation = entity.transform.rotation * offsetRotation;

                var request = new SpawnRequest(
                    newData,
                    entity.transform.position,
                    newRotation,
                    delay,
                    entity.Owner
                );

                SpawnQueue.Instance.Enqueue(request);
            }
        }
    }
}
